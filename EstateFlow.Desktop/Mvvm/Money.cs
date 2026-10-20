using System.Globalization;

namespace EstateFlow.Desktop.Mvvm;

// Amounts are typed by people on machines set to South African English, where the decimal
// separator is a comma and the thousands separator is a space. A plain decimal.TryParse silently
// turned "1500.50" into R0.00 on such machines. This accepts "1500.50", "1500,50", "1 500,50",
// "1,500.50", "R 1 500.50" and "1.500,50".
public static class Money
{
    public static bool TryParse(string? text, out decimal value)
    {
        value = 0m;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var chars = text
            .Where(c => !char.IsWhiteSpace(c) && c != '\u00A0' && c != '\u202F' && c != 'R' && c != 'r')
            .ToArray();
        var s = new string(chars);

        if (s.Length == 0)
            return false;

        var lastDot = s.LastIndexOf('.');
        var lastComma = s.LastIndexOf(',');

        if (lastDot >= 0 && lastComma >= 0)
        {
            // Both present: whichever comes last is the decimal separator.
            s = lastComma > lastDot
                ? s.Replace(".", "").Replace(',', '.')
                : s.Replace(",", "");
        }
        else if (lastComma >= 0)
        {
            var commas = s.Count(c => c == ',');
            var digitsAfter = s.Length - lastComma - 1;

            // "1,500,000" or "1,500" are thousands separators; "1500,50" is a decimal comma.
            s = commas > 1 || digitsAfter == 3
                ? s.Replace(",", "")
                : s.Replace(',', '.');
        }
        else if (lastDot >= 0)
        {
            var dots = s.Count(c => c == '.');
            var digitsAfter = s.Length - lastDot - 1;

            // "1.500.000" is thousands; "1.500" with three digits after a single dot is read as
            // a decimal (R1.500 is not a realistic rand amount, but 0.125 style percentages are).
            if (dots > 1)
                s = s.Replace(".", "");
            else if (digitsAfter == 3 && lastDot > 0 && s.Length > 5)
                s = s.Replace(".", "");
        }

        return decimal.TryParse(
            s,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }
}
