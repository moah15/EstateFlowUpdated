using System.Globalization;
using System.Windows.Data;

namespace EstateFlow.Desktop.Converters;

public class ReconLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? "Unrecon" : "Reconcile";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
