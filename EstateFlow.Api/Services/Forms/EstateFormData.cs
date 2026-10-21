using System.Globalization;
using EstateFlow.Core.Models;

namespace EstateFlow.Api.Services.Forms;

// Turns an estate into the named values that form templates are filled from,
// for example "deceased.surname" or "executor.fullName".
public static class EstateFormData
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static Dictionary<string, string> Build(Estate estate, DateTime today)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void Set(string key, string? value) => d[key] = (value ?? string.Empty).Trim();

        string Long(DateTime? date) => date.HasValue ? date.Value.ToString("dd MMMM yyyy", Culture) : string.Empty;
        string Short(DateTime? date) => date.HasValue ? date.Value.ToString("yyyy/MM/dd", Culture) : string.Empty;
        string Compact(DateTime? date) => date.HasValue ? date.Value.ToString("yyyyMMdd", Culture) : string.Empty;

        // ── estate ──
        Set("estate.fileNumber", estate.FileNumber);
        Set("estate.mastersRef", estate.MastersReference);
        Set("estate.number", string.IsNullOrWhiteSpace(estate.MastersReference) ? estate.FileNumber : estate.MastersReference);
        Set("estate.mastersOffice", estate.MastersOffice);
        Set("estate.lettersDate", Long(estate.LettersOfExecutorshipDate));
        Set("estate.bankName", estate.EstateBankName);
        Set("estate.bankBranchCode", estate.EstateBankBranchCode);
        Set("estate.bankAccountNumber", estate.EstateBankAccountNumber);

        // ── deceased ──
        var dec = estate.Deceased;

        var surname = dec?.Surname;
        var firstNames = string.Join(" ", new[] { dec?.FirstName, dec?.MiddleName }.Where(s => !string.IsNullOrWhiteSpace(s)));

        // Older records only have a full name.
        if (string.IsNullOrWhiteSpace(surname) && string.IsNullOrWhiteSpace(firstNames) && !string.IsNullOrWhiteSpace(dec?.FullName))
        {
            var parts = dec!.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            surname = parts.Length > 1 ? parts[^1] : string.Empty;
            firstNames = parts.Length > 1 ? string.Join(" ", parts[..^1]) : parts.FirstOrDefault() ?? string.Empty;
        }

        Set("deceased.fullName", dec?.FullName);
        Set("deceased.surname", surname);
        Set("deceased.firstNames", firstNames);
        Set("deceased.idNumber", dec?.IdNumber);
        Set("deceased.dob", Long(dec?.DateOfBirth));
        Set("deceased.dobShort", Short(dec?.DateOfBirth));
        Set("deceased.dobCompact", Compact(dec?.DateOfBirth));
        Set("deceased.dod", Long(estate.DateOfDeath == default ? null : estate.DateOfDeath));
        Set("deceased.dodShort", Short(estate.DateOfDeath == default ? null : estate.DateOfDeath));
        Set("deceased.dodCompact", Compact(estate.DateOfDeath == default ? null : estate.DateOfDeath));
        Set("deceased.placeOfDeath", dec?.PlaceOfDeath);
        Set("deceased.maritalStatus", dec?.MaritalStatus);
        Set("deceased.regime", dec?.MatrimonialRegime);
        Set("deceased.spouseName", dec?.SpouseFullName);
        Set("deceased.spouseId", dec?.SpouseIdNumber);
        Set("deceased.gender", dec?.Gender);
        Set("deceased.hasWill", estate.IsTestate ? "Yes" : "No");
        Set("deceased.willDate", Long(dec?.WillDate));

        var inCommunity = string.Equals(dec?.MatrimonialRegime, "In community of property", StringComparison.OrdinalIgnoreCase);
        Set("deceased.spouseNameInCommunity", inCommunity ? dec?.SpouseFullName : string.Empty);
        Set("deceased.spouseNameIfFemale",
            string.Equals(dec?.Gender, "Female", StringComparison.OrdinalIgnoreCase) ? dec?.SpouseFullName : string.Empty);

        // one box per digit (file cover)
        var idDigits = (dec?.IdNumber ?? string.Empty).Trim();
        for (var i = 1; i <= 13; i++)
            Set($"deceased.idDigit{i}", i <= idDigits.Length ? idDigits[i - 1].ToString() : string.Empty);

        // ── executor (the person completing the forms) ──
        var ex = estate.Executors.FirstOrDefault();

        Set("executor.fullName", ex?.FullName);
        Set("executor.idNumber", ex?.IdNumber);
        Set("executor.address", ex?.Address);
        Set("executor.phone", ex?.Phone);
        Set("executor.email", ex?.Email);
        Set("executor.role", ex?.Role);
        Set("executor.mastersOffice", ex?.MastersOffice);
        Set("executor.lettersRef", ex?.LettersOfExecutorshipRef);
        Set("executor.lettersDate", Long(ex?.LettersOfExecutorshipDate));
        Set("executors.all", string.Join(", ", estate.Executors.Select(e => e.FullName)));

        // ── agent / staff ──
        Set("agent.name", estate.FirmActsAsAgent ? estate.AgentName : string.Empty);
        Set("agent.reference", estate.FirmActsAsAgent ? estate.AgentReference : string.Empty);
        Set("agent.email", estate.FirmActsAsAgent ? estate.AgentEmail : string.Empty);
        Set("agent.phone", estate.FirmActsAsAgent ? estate.AgentPhone : string.Empty);
        Set("agent.nameAndPhone",
            estate.FirmActsAsAgent ? string.Join(", ", new[] { estate.AgentName, estate.AgentPhone }.Where(s => !string.IsNullOrWhiteSpace(s))) : string.Empty);
        Set("controller.name", estate.Controller?.StaffName);
        Set("controller.email", estate.Controller?.Email);

        // ── today / place ──
        Set("today.long", Long(today));
        Set("today.short", Short(today));
        Set("today.day", today.Day.ToString(Culture));
        Set("today.monthName", today.ToString("MMMM", Culture));
        Set("today.dayMonth", today.ToString("d MMMM", Culture));
        Set("today.year", today.Year.ToString(Culture));
        Set("place", string.IsNullOrWhiteSpace(estate.PlaceOfAccount) ? estate.MastersOffice : estate.PlaceOfAccount);

        // ── heirs ──
        var heirs = estate.Beneficiaries.Where(b => !b.IsPredeceased).OrderBy(b => b.DisplayOrder).ToList();

        Set("heirs.all", string.Join(", ", heirs.Select(h => h.FullName)));

        for (var i = 0; i < heirs.Count; i++)
        {
            var h = heirs[i];
            var n = i + 1;
            Set($"heir{n}.name", h.FullName);
            Set($"heir{n}.id", h.IdNumber);
            Set($"heir{n}.relationship", string.IsNullOrWhiteSpace(h.RelationshipDescription) ? h.Relationship : h.RelationshipDescription);
            Set($"heir{n}.dob", Long(h.DateOfBirth));
            Set($"heir{n}.email", h.Email);
        }

        string Describe(Beneficiary h) =>
            h.DateOfBirth.HasValue ? $"{h.FullName} (born {Long(h.DateOfBirth)})" : h.FullName;

        bool Rel(Beneficiary h, params string[] words)
        {
            var text = $"{h.Relationship} {h.RelationshipDescription}".ToLowerInvariant();
            return words.Any(text.Contains);
        }

        Set("heirs.spouse", string.Join("\n", heirs.Where(h => h.IsSurvivingSpouse || Rel(h, "spouse", "wife", "husband", "partner")).Select(Describe)));
        Set("heirs.children", string.Join("\n", heirs.Where(h => Rel(h, "child", "son", "daughter")).Select(Describe)));
        Set("heirs.parents", string.Join("\n", heirs.Where(h => Rel(h, "parent", "father", "mother")).Select(Describe)));
        Set("heirs.siblings", string.Join("\n", heirs.Where(h => Rel(h, "sibling", "brother", "sister")).Select(Describe)));

        // ── assets (for the inventory) ──
        var immovable = estate.ImmovableProperties.ToList();
        for (var i = 0; i < immovable.Count; i++)
        {
            var p = immovable[i];
            var n = i + 1;

            var parts = new List<string> { p.Description };
            if (!string.IsNullOrWhiteSpace(p.ErfNumber)) parts.Add($"Erf {p.ErfNumber}");
            if (!string.IsNullOrWhiteSpace(p.SchemeName)) parts.Add($"{p.SchemeName} Section {p.SectionNumber}");
            if (!string.IsNullOrWhiteSpace(p.Township)) parts.Add(p.Township!);
            if (!string.IsNullOrWhiteSpace(p.TitleDeedNumber)) parts.Add($"Title deed {p.TitleDeedNumber}");

            var share = p.ShareNumerator.HasValue && p.ShareDenominator is > 0
                ? (decimal)p.ShareNumerator.Value / p.ShareDenominator.Value
                : 1m;

            Set($"immovable.{n}.description", string.Join(", ", parts.Where(s => !string.IsNullOrWhiteSpace(s))));
            Set($"immovable.{n}.value", Money((p.MarketValue ?? p.MunicipalValue) * share));
        }

        var movable = estate.MovableAssets.ToList();
        for (var i = 0; i < movable.Count; i++)
        {
            var m = movable[i];
            var n = i + 1;

            var text = m.Description;
            if (!string.IsNullOrWhiteSpace(m.RegistrationNumber)) text += $" ({m.RegistrationNumber})";
            if (!string.IsNullOrWhiteSpace(m.ShareCompany)) text += $" – {m.NumberOfShares} {m.ShareClass} shares in {m.ShareCompany}";

            Set($"movable.{n}.description", text);
            Set($"movable.{n}.value", Money(m.Value));
        }

        var claims = estate.FinancialClaims.Where(c => c.IsInFavourOfEstate).ToList();
        for (var i = 0; i < claims.Count; i++)
        {
            var c = claims[i];
            var n = i + 1;

            var text = c.InstitutionName;
            if (!string.IsNullOrWhiteSpace(c.ClaimType)) text += $" – {c.ClaimType}";
            if (!string.IsNullOrWhiteSpace(c.AccountReference)) text += $" (ref {c.AccountReference})";

            Set($"claims.{n}.description", text);
            Set($"claims.{n}.value", Money(c.Value + c.AccruedInterest));
        }

        return d;
    }

    private static string Money(decimal value) => value.ToString("N2", Culture).Replace(",", " ");
}
