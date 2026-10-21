using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Advanced;
using UglyToad.PdfPig.Content;

namespace EstateFlow.Api.Services.Forms;

// One fillable box on a form template.
public class PdfFieldInfo
{
    // Unique within the template (the name can repeat on a form, so a counter is added).
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Page { get; set; }
    public double Left { get; set; }
    public double Bottom { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    // Text, Checkbox or Choice
    public string Type { get; set; } = "Text";
    public bool Multiline { get; set; }
    public bool Comb { get; set; }
    public int MaxLength { get; set; }
    public double FontSize { get; set; }

    // The wording printed next to or above the box, used as its heading in the editor.
    public string Label { get; set; } = string.Empty;
}

// Finds the fillable boxes on a PDF form, with their position and a best guess at their label.
public static class PdfFormReader
{
    public static List<PdfFieldInfo> ReadFields(byte[] pdf)
    {
        var fields = new List<PdfFieldInfo>();

        using var sharp = PdfReader.Open(new MemoryStream(pdf), PdfDocumentOpenMode.Import);
        using var pig = UglyToad.PdfPig.PdfDocument.Open(pdf);

        var seen = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var p = 0; p < sharp.PageCount; p++)
        {
            var annots = sharp.Pages[p].Elements.GetArray("/Annots");
            if (annots is null)
                continue;

            var words = pig.GetPage(p + 1).GetWords().ToList();
            var pageFields = new List<PdfFieldInfo>();

            for (var i = 0; i < annots.Elements.Count; i++)
            {
                var widget = Resolve(annots.Elements[i]);

                if (widget is null || widget.Elements.GetName("/Subtype") != "/Widget")
                    continue;

                var rect = widget.Elements.GetRectangle("/Rect");
                if (rect.Width < 3 || rect.Height < 3)
                    continue;

                var fieldType = Inherited(widget, d => d.Elements.GetName("/FT"));
                if (string.IsNullOrEmpty(fieldType))
                    continue;

                var flags = Inherited(widget, d => d.Elements.ContainsKey("/Ff") ? (int?)d.Elements.GetInteger("/Ff") : null) ?? 0;
                var readOnly = (flags & 1) != 0;
                if (readOnly)
                    continue;

                var name = FullName(widget);
                seen[name] = seen.TryGetValue(name, out var n) ? n + 1 : 1;

                var info = new PdfFieldInfo
                {
                    Id = seen[name] == 1 ? name : $"{name}#{seen[name]}",
                    Name = name,
                    Page = p + 1,
                    Left = rect.X1,
                    Bottom = rect.Y1,
                    Width = rect.Width,
                    Height = rect.Height,
                    MaxLength = Inherited(widget, d => d.Elements.ContainsKey("/MaxLen") ? (int?)d.Elements.GetInteger("/MaxLen") : null) ?? 0
                };

                switch (fieldType)
                {
                    case "/Tx":
                        info.Type = "Text";
                        info.Multiline = (flags & 4096) != 0;
                        info.Comb = (flags & 16777216) != 0 && info.MaxLength > 0;
                        break;

                    case "/Btn":
                        if ((flags & 65536) != 0)
                            continue;
                        info.Type = "Checkbox";
                        break;

                    case "/Ch":
                        info.Type = "Choice";
                        break;

                    default:
                        continue;
                }

                info.FontSize = ReadFontSize(widget);
                pageFields.Add(info);
            }

            foreach (var field in pageFields)
                field.Label = GuessLabel(field, pageFields, words);

            fields.AddRange(pageFields);
        }

        return fields;
    }

    // ───────────────── helpers ─────────────────

    internal static PdfDictionary? Resolve(PdfItem item) =>
        item switch
        {
            PdfReference r => r.Value as PdfDictionary,
            PdfDictionary d => d,
            _ => null
        };

    internal static T? Inherited<T>(PdfDictionary widget, Func<PdfDictionary, T?> read)
    {
        var current = widget;

        for (var depth = 0; current is not null && depth < 12; depth++)
        {
            var value = read(current);
            if (value is not null && !(value is string s && s.Length == 0))
                return value;

            current = Resolve(current.Elements["/Parent"]!);
        }

        return default;
    }

    internal static string FullName(PdfDictionary widget)
    {
        var parts = new List<string>();
        var current = widget;

        for (var depth = 0; current is not null && depth < 12; depth++)
        {
            var title = current.Elements.GetString("/T");
            if (!string.IsNullOrEmpty(title))
                parts.Insert(0, title);

            current = Resolve(current.Elements["/Parent"]!);
        }

        return string.Join(".", parts);
    }

    // "/Helv 10 Tf 0 g" -> 10 (0 means automatic size)
    private static double ReadFontSize(PdfDictionary widget)
    {
        var da = Inherited(widget, d => d.Elements.GetString("/DA"));
        if (string.IsNullOrWhiteSpace(da))
            return 0;

        var tokens = da.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tf = Array.IndexOf(tokens, "Tf");

        return tf >= 1 && double.TryParse(tokens[tf - 1], System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var size)
            ? size
            : 0;
    }

    private static bool IsFiller(string text) =>
        text.All(c => c is '.' or '…' or '_' or '-' or '–' or '—' or ':' or ';' or ',' or ' ');

    // The printed words just left of the box on the same line; failing that, the line above it.
    private static string GuessLabel(PdfFieldInfo field, List<PdfFieldInfo> siblings, List<Word> words)
    {
        var mid = field.Bottom + field.Height / 2;
        var top = field.Bottom + field.Height;

        double LeftLimit()
        {
            // Do not read past another box that sits to the left on the same line.
            var left = siblings
                .Where(o => o != field && o.Left + o.Width <= field.Left + 1 &&
                            o.Bottom < mid && o.Bottom + o.Height > mid)
                .Select(o => o.Left + o.Width)
                .DefaultIfEmpty(0)
                .Max();

            return left;
        }

        var limit = LeftLimit();

        var sameLine = words
            .Where(w => !IsFiller(w.Text) &&
                        w.BoundingBox.Right <= field.Left + 6 &&
                        w.BoundingBox.Left >= limit - 1 &&
                        w.BoundingBox.Bottom < top - 1 &&
                        w.BoundingBox.Top > field.Bottom + 1 &&
                        Math.Abs((w.BoundingBox.Bottom + w.BoundingBox.Top) / 2 - mid) < Math.Max(8, field.Height))
            .OrderBy(w => w.BoundingBox.Left)
            .ToList();

        // keep the words closest to the box (the last few on the line)
        var line = new List<Word>();
        for (var i = sameLine.Count - 1; i >= 0 && line.Count < 14; i--)
        {
            if (line.Count > 0 && line[0].BoundingBox.Left - sameLine[i].BoundingBox.Right > 40)
                break;

            line.Insert(0, sameLine[i]);
        }

        var label = string.Join(" ", line.Select(w => w.Text));

        if (label.Count(char.IsLetter) >= 3)
            return Clean(label);

        var above = words
            .Where(w => !IsFiller(w.Text) &&
                        w.BoundingBox.Bottom >= top - 2 &&
                        w.BoundingBox.Bottom <= top + 26 &&
                        w.BoundingBox.Right > field.Left - 4 &&
                        w.BoundingBox.Left < field.Left + field.Width + 4)
            .OrderBy(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        if (above.Count > 0)
        {
            var nearest = above[0].BoundingBox.Bottom;
            var row = above
                .Where(w => Math.Abs(w.BoundingBox.Bottom - nearest) < 3)
                .OrderBy(w => w.BoundingBox.Left)
                .Select(w => w.Text);

            var text = string.Join(" ", row);
            if (text.Count(char.IsLetter) >= 2)
                return Clean(text);
        }

        return string.Empty;
    }

    private static string Clean(string text)
    {
        var cleaned = text.Replace("…", " ").Replace("....", " ");
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"[._]{2,}", " ");
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim(' ', ':', ',', ';', '-');
        return cleaned.Length > 90 ? cleaned[^90..] : cleaned;
    }
}
