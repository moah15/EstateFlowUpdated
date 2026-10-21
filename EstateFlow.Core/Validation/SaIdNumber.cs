using System.Globalization;

namespace EstateFlow.Core.Validation;

// South African ID numbers are 13 digits: YYMMDD SSSS C A Z
//   YYMMDD  date of birth
//   Z       Luhn check digit
// Anything that is not exactly 13 digits (for example a passport number) is not checked.
public static class SaIdNumber
{
    public static bool LooksLikeSaId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var s = id.Trim();
        return s.Length == 13 && s.All(c => c is >= '0' and <= '9');
    }

    // 13 digits, a real month and day, and a correct check digit.
    public static bool IsValid(string? id)
    {
        if (!LooksLikeSaId(id))
            return false;

        var s = id!.Trim();
        var month = int.Parse(s.AsSpan(2, 2), CultureInfo.InvariantCulture);
        var day = int.Parse(s.AsSpan(4, 2), CultureInfo.InvariantCulture);

        // 2000 is a leap year, so 29 February is accepted here (the century is not known yet).
        if (month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(2000, month))
            return false;

        var sum = 0;
        for (var i = 0; i < 13; i++)
        {
            var digit = s[12 - i] - '0';
            if (i % 2 == 1)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
        }

        return sum % 10 == 0;
    }

    // The ID only holds a two-digit year, so the century is the latest one that does not put the
    // birth after referenceDate (for example the date of death, or today for a living heir).
    public static bool TryGetDateOfBirth(string? id, DateTime referenceDate, out DateTime dateOfBirth)
    {
        dateOfBirth = default;

        if (!IsValid(id))
            return false;

        var s = id!.Trim();
        var yy = int.Parse(s.AsSpan(0, 2), CultureInfo.InvariantCulture);
        var month = int.Parse(s.AsSpan(2, 2), CultureInfo.InvariantCulture);
        var day = int.Parse(s.AsSpan(4, 2), CultureInfo.InvariantCulture);

        foreach (var year in new[] { 2000 + yy, 1900 + yy })
        {
            if (day > DateTime.DaysInMonth(year, month))
                continue;

            var candidate = new DateTime(year, month, day);
            if (candidate <= referenceDate.Date)
            {
                dateOfBirth = candidate;
                return true;
            }
        }

        return false;
    }

    // Returns an error message, or null when the ID is blank, not a 13-digit number,
    // or valid and consistent with the date of birth.
    public static string? Validate(string? idNumber, DateTime? dateOfBirth, string who)
    {
        if (!LooksLikeSaId(idNumber))
            return null;

        var id = idNumber!.Trim();

        if (!IsValid(id))
            return $"{who}: \"{id}\" is not a valid South African ID number. " +
                   "Check the digits, or clear the field if this is a passport number.";

        if (!dateOfBirth.HasValue)
            return null;

        var fromId = id[..6];
        var entered = dateOfBirth.Value.ToString("yyMMdd", CultureInfo.InvariantCulture);

        if (!string.Equals(fromId, entered, StringComparison.Ordinal))
            return $"{who}: the ID number {id} starts with {fromId} (year, month, day), " +
                   $"but the date of birth entered is {dateOfBirth.Value:dd MMM yyyy}. " +
                   "The ID number and the date of birth must match.";

        return null;
    }
}
