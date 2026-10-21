namespace EstateFlow.Core.Dtos;

// ─── Master's Office forms ("Estate Documents") ───────────────────────────────────────────

// One heading on the Estate Documents list, for example "Deceased Estates".
public class FormGroupDto
{
    public string Heading { get; set; } = string.Empty;
    public List<FormSummaryDto> Forms { get; set; } = new();
}

public class FormSummaryDto
{
    // Stable key used in requests (the template file name without ".pdf").
    public string Id { get; set; } = string.Empty;

    // The Master's Office form number, for example "J294". Empty when the form has none.
    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;

    // False for forms that cannot be filled in here (for example Adobe LiveCycle GPW forms
    // or flat information sheets). They can still be opened and downloaded blank.
    public bool CanFill { get; set; }

    public string? Note { get; set; }

    public string DisplayName =>
        string.IsNullOrWhiteSpace(Code) ? Title : $"{Code} – {Title}";
}

// A form with the estate's details already filled in, ready to be reviewed and changed.
public class EstateFormDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool CanFill { get; set; }
    public string? Message { get; set; }
    public List<FormFieldDto> Fields { get; set; } = new();
}

public class FormFieldDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    // Text or Checkbox
    public string Type { get; set; } = "Text";
    public bool Multiline { get; set; }
    public int Page { get; set; }
    public int MaxLength { get; set; }

    public string Value { get; set; } = string.Empty;

    // True when the value came from the estate records rather than being left blank.
    public bool AutoFilled { get; set; }
}

public class GenerateFormRequest
{
    // Field id -> text ("true" / "false" for tick boxes)
    public Dictionary<string, string> Values { get; set; } = new();
}
