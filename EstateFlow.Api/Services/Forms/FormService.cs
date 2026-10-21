using System.Collections.Concurrent;
using EstateFlow.Core.Dtos;
using EstateFlow.Core.Interfaces;

namespace EstateFlow.Api.Services.Forms;

// Estate Documents: the Master's Office forms, filled in from an estate's details.
public class FormService
{
    private static readonly ConcurrentDictionary<string, List<PdfFieldInfo>> FieldCache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly IEstateRepository _repository;
    private readonly string _templateFolder;

    public FormService(IEstateRepository repository, IWebHostEnvironment environment)
    {
        _repository = repository;
        _templateFolder = Path.Combine(environment.ContentRootPath, "FormTemplates");
    }

    public List<FormGroupDto> GetCatalog() => FormCatalog.Groups();

    // The blank form exactly as the Master's Office publishes it.
    public (byte[] Content, string FileName)? GetBlank(string id)
    {
        var form = FormCatalog.Find(id);
        var bytes = form is null ? null : ReadTemplate(form.Id);

        if (form is null || bytes is null)
            return null;

        return (bytes, FileNameFor(form, null, blank: true));
    }

    // The form with every box that the estate can answer already filled in.
    public async Task<EstateFormDto> GetEstateFormAsync(Guid estateId, string id)
    {
        var form = FormCatalog.Find(id)
            ?? throw new InvalidOperationException($"Form '{id}' was not found.");

        var result = new EstateFormDto
        {
            Id = form.Id,
            Code = form.Code,
            Title = form.Title,
            CanFill = form.CanFill,
            Message = form.Note
        };

        if (!form.CanFill)
            return result;

        var estate = await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException($"Estate {estateId} not found.");

        var template = ReadTemplate(form.Id)
            ?? throw new InvalidOperationException($"The template for form '{form.Id}' is missing on the server.");

        var fields = FieldsOf(form.Id, template);
        var data = EstateFormData.Build(estate, DateTime.Today);
        var suggested = FormAutoFill.Suggest(form.Id, fields, data);

        foreach (var field in Ordered(fields))
        {
            var filled = suggested.TryGetValue(field.Id, out var value);

            result.Fields.Add(new FormFieldDto
            {
                Id = field.Id,
                Label = string.IsNullOrWhiteSpace(field.Label) ? Tidy(field.Name) : field.Label,
                Type = field.Type == "Checkbox" ? "Checkbox" : "Text",
                Multiline = field.Multiline,
                Page = field.Page,
                MaxLength = field.Comb ? field.MaxLength : 0,
                Value = filled ? value! : string.Empty,
                AutoFilled = filled
            });
        }

        if (result.Fields.Count == 0)
        {
            result.CanFill = false;
            result.Message = "This form has no fillable boxes. Open it blank and complete it by hand.";
        }

        return result;
    }

    // The finished PDF, with the (possibly edited) values written onto the form.
    public async Task<(byte[] Content, string FileName)> GenerateAsync(
        Guid estateId,
        string id,
        IReadOnlyDictionary<string, string> values)
    {
        var form = FormCatalog.Find(id)
            ?? throw new InvalidOperationException($"Form '{id}' was not found.");

        if (!form.CanFill)
            throw new ArgumentException("This form cannot be filled in here.");

        var estate = await _repository.GetByIdAsync(estateId)
            ?? throw new InvalidOperationException($"Estate {estateId} not found.");

        var template = ReadTemplate(form.Id)
            ?? throw new InvalidOperationException($"The template for form '{form.Id}' is missing on the server.");

        var fields = FieldsOf(form.Id, template);

        var known = fields.Select(f => f.Id).ToHashSet(StringComparer.Ordinal);
        var accepted = values
            .Where(kv => known.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        var pdf = PdfFormFiller.Fill(template, fields, accepted);

        return (pdf, FileNameFor(form, estate.FileNumber, blank: false));
    }

    // ───────────────────── helpers ─────────────────────

    private byte[]? ReadTemplate(string id)
    {
        var path = Path.Combine(_templateFolder, id + ".pdf");
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    private static List<PdfFieldInfo> FieldsOf(string id, byte[] template) =>
        FieldCache.GetOrAdd(id, _ => PdfFormReader.ReadFields(template));

    // reading order: page, then top to bottom, then left to right
    private static IEnumerable<PdfFieldInfo> Ordered(IEnumerable<PdfFieldInfo> fields) =>
        fields.OrderBy(f => f.Page)
              .ThenByDescending(f => Math.Round(f.Bottom / 4))
              .ThenBy(f => f.Left);

    private static string Tidy(string name) =>
        name.Replace('_', ' ').Replace('.', ' ').Trim();

    private static string FileNameFor(FormSummaryDto form, string? fileNumber, bool blank)
    {
        static string Safe(string text) =>
            string.Concat(text.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_'));

        var name = Safe(string.IsNullOrWhiteSpace(form.Code) ? form.Id : form.Code);

        return blank
            ? $"{name}_blank.pdf"
            : $"{name}_{Safe(fileNumber ?? "estate")}_{DateTime.Today:yyyyMMdd}.pdf";
    }
}
