using System;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converters;

public partial class BoolReverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b) return !b;
        throw new Exception($"Cannot convert {value} to {nameof(Boolean)}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b) return !b;
        throw new Exception($"Cannot convert {value} to {nameof(Boolean)}");
    }
}