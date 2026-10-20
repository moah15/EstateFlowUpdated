using System.Globalization;
using System.Windows.Data;

namespace EstateFlow.Desktop.Converters;

public class PauseToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "Off (tap to enable)" : "On (tap to pause)";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}