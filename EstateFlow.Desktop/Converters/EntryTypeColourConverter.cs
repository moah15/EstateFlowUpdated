using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EstateFlow.Desktop.Converters;

public class EntryTypeColourConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && s == "Receipt"
            ? new SolidColorBrush(Color.FromRgb(0x3F, 0x5B, 0x45))
            : new SolidColorBrush(Color.FromRgb(0x6E, 0x2A, 0x2A));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
