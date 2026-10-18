using System.Globalization;

namespace Mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Color.FromArgb("#388E3C");
    public Color FalseColor { get; set; } = Color.FromArgb("#E53935");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueColor : FalseColor;
        }
        return FalseColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return false;
    }
}

public class InvertedBoolConverter : InverseBoolConverter
{
}

public class TabActiveColorConverter : IValueConverter
{
    public Color ActiveColor { get; set; } = Color.FromArgb("#4152B0");
    public Color InactiveColor { get; set; } = Color.FromArgb("#EDEFFB");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b) return ActiveColor;
        return InactiveColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class TabActiveTextColorConverter : IValueConverter
{
    public Color ActiveColor { get; set; } = Color.FromArgb("#FFFFFF");
    public Color InactiveColor { get; set; } = Color.FromArgb("#4152B0");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b) return ActiveColor;
        return InactiveColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

