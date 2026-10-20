using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EstateFlow.Desktop.Converters;

public class OverdueColourConverter : IValueConverter
{
    private static readonly SolidColorBrush Red  = new(Color.FromRgb(0x6E, 0x2A, 0x2A));
    private static readonly SolidColorBrush Grey = new(Color.FromRgb(0x6B, 0x6E, 0x76));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Red : Grey;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
