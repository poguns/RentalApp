using System.Globalization;
namespace StarterApp.Converters;

public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var parts = parameter?.ToString()?.Split('|');
        return (bool)value ? parts?[0] ?? "Yes" : parts?[1] ?? "No";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}