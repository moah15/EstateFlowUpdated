using System.Text.RegularExpressions;

namespace EstateFlow.Api.Services.Forms;

// Decides what goes into each box of a form from the estate's details.
//
// Each form has its own table (field id -> template). A template is plain text with {tokens}
// taken from EstateFormData, for example "{executor.fullName}". A few general rules cover boxes
// on forms that have no table. Everything stays editable in the app, so a missing or wrong guess
// only costs the user a correction.
public static class FormAutoFill
{
    private static readonly Dictionary<string, Dictionary<string, string>> Tables =
        new(StringComparer.OrdinalIgnoreCase);

    // Inventory pages that list assets: form -> (page, token prefix)
    private static readonly Dictionary<string, (int Page, string Prefix)[]> AssetTables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["J243"] = new[] { (2, "immovable"), (3, "movable"), (4, "claims") }
        };

    static FormAutoFill()
    {
        // ── J034 File cover ──
        var j034 = new Dictionary<string, string>
        {
            ["Text1"] = "{estate.number}",
            ["Text2"] = "{deceased.surname}",
            ["Text3"] = "{deceased.firstNames}",
            ["Text6"] = "{deceased.dod}"
        };
        for (var i = 0; i < 13; i++)
            j034[$"Text4{(char)('a' + i)}"] = $"{{deceased.idDigit{i + 1}}}";
        Tables["J034"] = j034;

        // ── J155 Undertaking and acceptance of Master's directions ──
        Tables["J155"] = new()
        {
            ["Text1"] = "{estate.number}",
            ["Text2"] = "{deceased.fullName}",
            ["Text3"] = "{deceased.dod}",
            ["Text4-1"] = "{deceased.idNumber}",
            ["Text6"] = "{executor.fullName}",
            ["Text7-1"] = "{executor.idNumber}",
            ["Text9-1"] = "{executor.address}",
            ["Text10-1"] = "{executor.address}",
            ["Text9-5"] = "{executor.phone}",
            ["Text11-1"] = "{agent.name}",
            ["Text11-4"] = "{agent.phone}"
        };

        // ── J190 Acceptance of trust as Executor ──
        Tables["J190"] = new()
        {
            ["Estate No"] = "{estate.number}",
            ["lWe full names and surname"] = "{executor.fullName}",
            ["Residential address.0"] = "{executor.address}",
            ["Telephone numbers.0"] = "{executor.phone}",
            ["ID"] = "{executor.idNumber}",
            ["Full names and surname"] = "{deceased.fullName}",
            ["Date of birth"] = "{deceased.dob}",
            ["Date of death"] = "{deceased.dod}",
            ["Identity No"] = "{deceased.idNumber}",
            ["Name of surviving spouse in case of deceased having been a married woman"] = "{deceased.spouseNameIfFemale}",
            ["signed at"] = "{place}",
            ["signed on"] = "{today.dayMonth}",
            ["year"] = "{today.year}",
            ["PRINT NAME AND SURNAME"] = "{executor.fullName}"
        };

        // ── J192 Affidavit: particulars of next-of-kin ──
        Tables["J192"] = new()
        {
            ["I"] = "{executor.fullName}",
            ["of 1"] = "{executor.address}",
            ["Print Name and Surname"] = "{executor.fullName}",
            ["at"] = "{place}",
            ["this.0"] = "{today.day}",
            ["day of"] = "{today.monthName}",
            ["year"] = "{today.year}",
            ["Names and addresses of the nextofkin of the late.0"] = "{deceased.fullName}",
            ["died at"] = "{deceased.placeOfDeath}",
            ["date"] = "{deceased.dod}",
            ["Names of relatives and degree of relationship1  Surviving spouse"] = "{heirs.spouse}",
            ["Names of relatives and degree of relationship2 Children and date of their birth Also state names of predeceased children and their dates of death"] = "{heirs.children}",
            ["Names of relatives and degree of relationshipIgnore questions 3 4 and 5 if the deceased left children or descendants 3 Father of deceased Mother of deceased"] = "{heirs.parents}",
            ["Names of relatives and degree of relationshipIgnore questions 4 and 5 if the parents are both alive 4  Brothers and sisters of the deceased  State whether full or half"] = "{heirs.siblings}"
        };

        // ── J262 Undertaking and bond of security ──
        Tables["J262"] = new()
        {
            ["In the estate of"] = "{deceased.fullName}",
            ["I  We"] = "{executor.fullName}",
            ["of 1"] = "{executor.address}",
            ["Not a post box number"] = "{executor.address}"
        };

        // ── J312 Undertaking and bond of security (executor) ──
        Tables["J312"] = new()
        {
            ["Text1.0"] = "{deceased.fullName}",
            ["Text1.1"] = "{executor.fullName}",
            ["Text1.3"] = "{executor.address}",
            ["Text1.10"] = "{place}",
            ["Text1.11"] = "{today.day}",
            ["Text1.12"] = "{today.monthName}",
            ["Text1.13"] = "{today.year}"
        };

        // ── J294 Death notice (English) ──
        Tables["J294"] = new()
        {
            ["1"] = "{deceased.surname}",
            ["2"] = "{deceased.firstNames}",
            ["Text1.0"] = "{deceased.idNumber}",
            ["Text1.1"] = "{deceased.dobCompact}",
            ["Text1.2"] = "{deceased.dodCompact}",
            ["11Has the deceased left a will"] = "{deceased.hasWill}",
            ["12 Marital status at time of death"] = "{deceased.maritalStatus}",
            ["14 Full names of surviving spouse"] = "{deceased.spouseName}",
            ["and hisher IDPassport number"] = "{deceased.spouseId}",
            ["15 State whether marriage was in or out of community of propertywhether accrual system is applicable"] = "{deceased.regime}",
            ["19 Name and address of person signing the death notice"] = "{executor.fullName}, {executor.address}",
            ["20 Capacity"] = "{executor.role}",
            ["day of.1"] = "{place}",
            ["day of.2"] = "{today.day}",
            ["day of.0"] = "{today.monthName}",
            ["in the year"] = "{today.year}"
        };

        // ── J294 Death notice (Afrikaans) ──
        Tables["J294_a"] = new()
        {
            ["1 Surname of deceased"] = "{deceased.surname}",
            ["2 Full first names"] = "{deceased.firstNames}",
            ["3 Identity number"] = "{deceased.idNumber}",
            ["8 Date of birth"] = "{deceased.dobShort}",
            ["10 Date of death"] = "{deceased.dodShort}",
            ["12 Marital status at time of death"] = "{deceased.maritalStatus}",
            ["14 Full names of surviving spouse and hisher occupation"] = "{deceased.spouseName}",
            ["19 Name and address of person signing the death notice 1"] = "{executor.fullName}",
            ["19 Name and address of person signing the death notice 2"] = "{executor.address}",
            ["20 Capacity"] = "{executor.role}",
            ["Dated at"] = "{place}",
            ["day of"] = "{today.day}",
            ["month"] = "{today.monthName}",
            ["in the year"] = "{today.year}"
        };

        // ── JM33_42 Application for endorsement (section 42(2)) ──
        Tables["JM33_42"] = new()
        {
            ["ESTATE LATE"] = "{deceased.fullName}",
            ["NO"] = "{estate.number}",
            ["PRINT NAME AND SURNAME"] = "{executor.fullName}",
            ["ESTATE CONTROLLER"] = "{controller.name}",
            ["DATE"] = "{today.short}",
            ["DATE_2"] = "{today.short}"
        };

        // ── Affidavit for appointment as executor ──
        Tables["ReportingAffidavit_moh"] = new()
        {
            ["I"] = "{executor.fullName}",
            ["MOH.0"] = "{estate.mastersOffice}",
            ["estate"] = "{deceased.fullName}",
            ["the late"] = "{deceased.fullName}",
            ["Signed and sworn toaffirmed before me at"] = "{place}",
            ["This"] = "{today.day}",
            ["Day of"] = "{today.monthName}",
            ["in the year"] = "{today.year}"
        };

        // ── Declaration of subsisting marriages ──
        Tables["Declaration_SubsistingMarriages_moh"] = new()
        {
            ["Deseaced"] = "{deceased.fullName}",
            ["Signed and sworn toaffirmed before me at"] = "{place}",
            ["This"] = "{today.day}",
            ["Day of"] = "{today.monthName}",
            ["in the year"] = "{today.year}"
        };

        // ── Nomination to act as executor ──
        var nomination = new Dictionary<string, string>
        {
            ["Estate late"] = "{deceased.fullName}",
            ["I  We the undersigned hereby nominates"] = "{executor.fullName}"
        };
        for (var n = 1; n <= 17; n++)
        {
            nomination[$"NameRow{n}"] = $"{{heir{n}.name}}";
            nomination[$"Relationship  CapacityRow{n}"] = $"{{heir{n}.relationship}}";
        }
        Tables["moh-LetterOfNomination"] = nomination;

        // ── Chief Master directive letters ──
        Tables["moh-MBU-9A-AnnexB"] = new()
        {
            ["Ref"] = "{estate.number}",
            ["Date"] = "{today.long}",
            ["ESTATE LATE"] = "{deceased.fullName}",
            ["Name of Account Holder 1"] = "{deceased.fullName}",
            ["Name of Account Holder 2"] = "{deceased.idNumber}"
        };
        Tables["moh-MBU-9B-AnnexC"] = new()
        {
            ["Ref"] = "{estate.number}",
            ["Date"] = "{today.long}",
            ["ESTATE LATE"] = "{deceased.fullName}"
        };
        Tables["moh-MBU-12-AnnexD"] = new()
        {
            ["Ref"] = "{estate.number}",
            ["Date"] = "{today.long}",
            ["ESTATE LATE"] = "{deceased.fullName}"
        };
        Tables["moh-MBU-19-AnnexA-Affidavit-Permanent-Life-Partnership"] = new()
        {
            ["I full names"] = "{deceased.spouseName}",
            ["Identity number"] = "{deceased.spouseId}",
            ["The deceased name  ID number"] = "{deceased.fullName}, {deceased.idNumber}",
            ["and I name of surviving partner"] = "{deceased.spouseName}",
            ["Text.12"] = "{place}",
            ["Text.13"] = "{today.day}",
            ["Text.14"] = "{today.monthName} {today.year}"
        };

        // ── Application for certified copies ──
        Tables["JM46-annex-c"] = new()
        {
            ["Master"] = "{estate.mastersOffice}",
            ["ESTATE.0"] = "{deceased.fullName}",
            ["NO"] = "{estate.number}",
            ["Name and Address of Applicant 1"] = "{executor.fullName}",
            ["Name and Address of Applicant 2"] = "{executor.address}",
            ["DATE"] = "{today.short}",
            ["PRINT NAME AND SURNAME"] = "{executor.fullName}"
        };

        // ── Inventory (J243): the asset rows are filled by position, see AssetTables ──
        Tables["J243"] = new()
        {
            ["name"] = "{deceased.fullName}",
            ["Full name of surviving spouse in a case where spouses were married in community of property"] = "{deceased.spouseNameInCommunity}",
            ["lfull name"] = "{executor.fullName}",
            ["of full address"] = "{executor.address}",
            ["in my capacity as.0"] = "{executor.role}",
            ["Place"] = "{place}",
            ["Date"] = "{today.long}",
            ["name and surname"] = "{executor.fullName}",
            ["furnished in the case of an inventory under section 9 of the Act 1"] = "{heir1.name}",
            ["furnished in the case of an inventory under section 9 of the Act 2"] = "{heir2.name}",
            ["furnished in the case of an inventory under section 9 of the Act 3"] = "{heir3.name}"
        };
    }

    // field id -> suggested value (only boxes that got a value)
    public static Dictionary<string, string> Suggest(
        string formId,
        IReadOnlyList<PdfFieldInfo> fields,
        IReadOnlyDictionary<string, string> data)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        Tables.TryGetValue(formId, out var table);

        foreach (var field in fields)
        {
            if (field.Type != "Text")
                continue;

            string? template = null;

            if (table is not null && table.TryGetValue(field.Id, out var t))
                template = t;
            else if (table is null)
                template = General(field);

            if (template is null)
                continue;

            var value = Expand(template, data);
            if (value.Length > 0)
                result[field.Id] = value;
        }

        if (AssetTables.TryGetValue(formId, out var assetPages))
            FillAssetRows(fields, data, assetPages, result);

        return result;
    }

    // ───────────────────── general rules (forms without a table) ─────────────────────

    private static string? General(PdfFieldInfo field)
    {
        var label = field.Label.Trim().ToLowerInvariant();

        if (Regex.IsMatch(label, @"^(estate\s*(no|number)\.?|master'?s? ref(erence)?( no\.?)?)$"))
            return "{estate.number}";

        if (Regex.IsMatch(label, @"^(in the )?estate( of)?( the)? late\.?$|^in the estate of$|^estate late$"))
            return "{deceased.fullName}";

        if (Regex.IsMatch(label, @"^(date of death|died on)\.?$"))
            return "{deceased.dod}";

        return null;
    }

    // ───────────────────── asset rows (inventory) ─────────────────────

    private static void FillAssetRows(
        IReadOnlyList<PdfFieldInfo> fields,
        IReadOnlyDictionary<string, string> data,
        (int Page, string Prefix)[] pages,
        Dictionary<string, string> result)
    {
        foreach (var (page, prefix) in pages)
        {
            var onPage = fields.Where(f => f.Page == page && f.Type == "Text").ToList();

            // narrow box on the left = description, box on the right = value
            var descriptions = onPage.Where(f => f.Left < 100 && f.Width > 300).OrderByDescending(f => f.Bottom).ToList();
            var values = onPage.Where(f => f.Left > 400).OrderByDescending(f => f.Bottom).ToList();

            for (var row = 0; row < Math.Min(descriptions.Count, values.Count); row++)
            {
                var n = row + 1;

                if (!data.TryGetValue($"{prefix}.{n}.description", out var description) || description.Length == 0)
                    break;

                result[descriptions[row].Id] = description;

                if (data.TryGetValue($"{prefix}.{n}.value", out var value) && value.Length > 0)
                    result[values[row].Id] = value;
            }
        }
    }

    // ───────────────────── template expansion ─────────────────────

    public static string Expand(string template, IReadOnlyDictionary<string, string> data)
    {
        var text = Regex.Replace(template, @"\{([^}]+)\}", m =>
            data.TryGetValue(m.Groups[1].Value, out var v) ? v : string.Empty);

        // tidy up separators left behind by empty parts, e.g. ", " or " ,"
        text = Regex.Replace(text, @"\s*,(\s*,)+", ",");
        text = text.Trim(' ', ',', '–', '-');
        text = Regex.Replace(text, @"[ \t]{2,}", " ");

        return text;
    }
}
