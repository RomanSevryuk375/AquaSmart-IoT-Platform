using System.Globalization;

namespace Mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Color.FromArgb("#388E3C");
    public Color FalseColor { get; set; } = Color.FromArgb("#E53935");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool b = value is bool flag && flag;
        if (parameter is string paramStr && paramStr.Contains('|'))
        {
            var parts = paramStr.Split('|');
            if (parts.Length == 2)
            {
                var truePart = Color.FromArgb(parts[0].Trim());
                var falsePart = Color.FromArgb(parts[1].Trim());
                return b ? truePart : falsePart;
            }
        }
        return b ? TrueColor : FalseColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class GreaterThanZeroConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count) return count > 0;
        if (value is double d) return d > 0;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
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

public class StringIsNotNullOrEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string s && !string.IsNullOrWhiteSpace(s);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
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

public class StepProgressColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int current && parameter != null && int.TryParse(parameter.ToString(), out var target))
        {
            return current >= target ? Color.FromArgb("#4152B0") : Color.FromArgb("#E4E7F4");
        }
        return Color.FromArgb("#E4E7F4");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StepButtonTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int current && current >= 7) return "Готово";
        return "Далее";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class InhabitantBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? Color.FromArgb("#4152B0") : Color.FromArgb("#F6F7FC");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class InhabitantTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#141414");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

