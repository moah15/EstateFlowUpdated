using System.Globalization;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace EstateFlow.Api.Services.Forms;

// Writes values onto a fillable PDF form and flattens it, so the result looks the same in every
// PDF viewer and when printed.
public static class PdfFormFiller
{
    private const string FontName = "/EFHelv";

    // Helvetica character widths (1/1000 em) for ASCII 32..126.
    private static readonly int[] Widths =
    {
        278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278,   // space .. /
        556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 278, 278, 584, 584, 584, 556,   // 0 .. ?
        1015, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778,  // @ .. O
        667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 469, 556,   // P .. _
        333, 556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556,   // ` .. o
        556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500, 334, 260, 334, 584         // p .. ~
    };

    static PdfFormFiller()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static byte[] Fill(
        byte[] template,
        IReadOnlyList<PdfFieldInfo> fields,
        IReadOnlyDictionary<string, string> values)
    {
        // Pages are copied into a new document, so forms that are marked read-only by their
        // owner (copy / modify restrictions) can still be filled in.
        using var input = new MemoryStream(template);
        using var source = PdfReader.Open(input, PdfDocumentOpenMode.Import);

        var doc = new PdfDocument();
        for (var i = 0; i < source.PageCount; i++)
            doc.AddPage(source.Pages[i]);

        var font = new PdfDictionary(doc);
        font.Elements["/Type"] = new PdfName("/Font");
        font.Elements["/Subtype"] = new PdfName("/Type1");
        font.Elements["/BaseFont"] = new PdfName("/Helvetica");
        font.Elements["/Encoding"] = new PdfName("/WinAnsiEncoding");
        doc.Internals.AddObject(font);

        for (var p = 0; p < doc.PageCount; p++)
        {
            var page = doc.Pages[p];
            var pageNumber = p + 1;

            var content = new StringBuilder();

            foreach (var field in fields.Where(f => f.Page == pageNumber))
            {
                if (!values.TryGetValue(field.Id, out var value) || string.IsNullOrEmpty(value))
                    continue;

                if (field.Type == "Checkbox")
                {
                    if (IsTicked(value))
                        DrawTick(content, field);
                }
                else if (field.Comb)
                {
                    DrawComb(content, field, value);
                }
                else if (field.Multiline)
                {
                    DrawWrapped(content, field, value);
                }
                else
                {
                    DrawSingleLine(content, field, value);
                }
            }

            if (content.Length > 0)
                AppendToPage(doc, page, content.ToString(), font);

            RemoveWidgets(page);
        }

        doc.Internals.Catalog.Elements.Remove("/AcroForm");

        using var output = new MemoryStream();
        doc.Save(output, false);
        return output.ToArray();
    }

    // ───────────────────────── drawing ─────────────────────────

    private static bool IsTicked(string value) =>
        value.Trim().ToLowerInvariant() is "true" or "yes" or "1" or "x" or "on" or "y";

    private static void DrawTick(StringBuilder sb, PdfFieldInfo field)
    {
        var size = Math.Min(field.Width, field.Height) * 0.8;
        var textWidth = Measure("X", size);

        Text(sb, "X", size,
            field.Left + (field.Width - textWidth) / 2,
            field.Bottom + (field.Height - size * 0.72) / 2);
    }

    private static void DrawSingleLine(StringBuilder sb, PdfFieldInfo field, string value)
    {
        value = value.Replace("\r", " ").Replace("\n", " ").Trim();

        var available = field.Width - 4;
        var size = field.FontSize > 0 ? field.FontSize : 10;
        size = Math.Min(size, Math.Max(5, field.Height - 3));

        while (Measure(value, size) > available && size > 5.5)
            size -= 0.5;

        while (Measure(value, size) > available && value.Length > 1)
            value = value[..^1];

        Text(sb, value, size, field.Left + 2, field.Bottom + (field.Height - size * 0.72) / 2);
    }

    private static void DrawComb(StringBuilder sb, PdfFieldInfo field, string value)
    {
        value = value.Replace(" ", string.Empty).Trim();
        if (value.Length > field.MaxLength)
            value = value[..field.MaxLength];

        var cell = field.Width / field.MaxLength;
        var size = Math.Min(field.FontSize > 0 ? field.FontSize : 11, Math.Max(5, field.Height - 3));

        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i].ToString();
            Text(sb, ch, size,
                field.Left + i * cell + (cell - Measure(ch, size)) / 2,
                field.Bottom + (field.Height - size * 0.72) / 2);
        }
    }

    private static void DrawWrapped(StringBuilder sb, PdfFieldInfo field, string value)
    {
        var available = field.Width - 6;

        for (var size = field.FontSize > 0 ? Math.Min(field.FontSize, 10) : 9.0; size >= 6; size -= 0.5)
        {
            var lines = Wrap(value, size, available);
            var leading = size * 1.2;

            if (lines.Count * leading <= field.Height - 2 || size <= 6)
            {
                var y = field.Bottom + field.Height - size - 1.5;

                foreach (var line in lines)
                {
                    if (y < field.Bottom - 1)
                        break;

                    Text(sb, line, size, field.Left + 3, y);
                    y -= leading;
                }

                return;
            }
        }
    }

    private static List<string> Wrap(string text, double size, double width)
    {
        var lines = new List<string>();

        foreach (var paragraph in text.Replace("\r", string.Empty).Split('\n'))
        {
            var current = string.Empty;

            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = current.Length == 0 ? word : current + " " + word;

                if (Measure(candidate, size) <= width || current.Length == 0)
                    current = candidate;
                else
                {
                    lines.Add(current);
                    current = word;
                }
            }

            lines.Add(current);
        }

        return lines;
    }

    private static double Measure(string text, double size)
    {
        var total = 0;

        foreach (var ch in text)
            total += ch >= 32 && ch <= 126 ? Widths[ch - 32] : 556;

        return total * size / 1000.0;
    }

    private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static void Text(StringBuilder sb, string text, double size, double x, double y)
    {
        sb.Append("BT ").Append(FontName).Append(' ').Append(Num(size)).Append(" Tf ")
          .Append(Num(x)).Append(' ').Append(Num(y)).Append(" Td (")
          .Append(Escape(text)).Append(") Tj ET\n");
    }

    // Text as WinAnsi bytes inside a PDF string literal.
    private static string Escape(string text)
    {
        var bytes = Encoding.GetEncoding(1252).GetBytes(text);
        var sb = new StringBuilder();

        foreach (var b in bytes)
        {
            if (b == '(' || b == ')' || b == '\\')
                sb.Append('\\').Append((char)b);
            else if (b < 32 || b > 126)
                sb.Append('\\').Append(Convert.ToString(b, 8).PadLeft(3, '0'));
            else
                sb.Append((char)b);
        }

        return sb.ToString();
    }

    // ───────────────────────── page plumbing ─────────────────────────

    private static void AppendToPage(PdfDocument doc, PdfPage page, string drawing, PdfDictionary font)
    {
        // The form's own content is kept as it is, between two small streams that save and restore
        // the drawing state (so the form's transforms cannot move or scale what is added) and add
        // the new text on top.
        var parts = new List<PdfItem> { NewStream(doc, "q\n") };

        var original = page.Elements["/Contents"];

        if (original is PdfReference reference && reference.Value is PdfArray referencedArray)
            parts.AddRange(referencedArray.Elements);
        else if (original is PdfArray array)
            parts.AddRange(array.Elements);
        else if (original is not null)
            parts.Add(original);

        parts.Add(NewStream(doc, "\nQ\nq\n0 g\n" + drawing + "Q\n"));

        var contents = new PdfArray(doc);
        foreach (var part in parts)
            contents.Elements.Add(part);

        page.Elements["/Contents"] = contents;

        var resources = page.Elements.GetDictionary("/Resources");
        if (resources is null)
        {
            resources = new PdfDictionary(doc);
            page.Elements["/Resources"] = resources;
        }

        var fonts = resources.Elements.GetDictionary("/Font");
        if (fonts is null)
        {
            fonts = new PdfDictionary(doc);
            resources.Elements["/Font"] = fonts;
        }

        fonts.Elements[FontName] = font.Reference;
    }

    private static PdfReference NewStream(PdfDocument doc, string text)
    {
        var stream = new PdfDictionary(doc);
        stream.CreateStream(Encoding.ASCII.GetBytes(text));
        doc.Internals.AddObject(stream);
        return stream.Reference!;
    }

    private static void RemoveWidgets(PdfPage page)
    {
        var annots = page.Elements.GetArray("/Annots");
        if (annots is null)
            return;

        for (var i = annots.Elements.Count - 1; i >= 0; i--)
        {
            var annot = PdfFormReader.Resolve(annots.Elements[i]);

            if (annot?.Elements.GetName("/Subtype") == "/Widget")
                annots.Elements.RemoveAt(i);
        }
    }
}
