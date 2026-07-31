using System;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converters;

public partial class CountToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not int count) return true;
        if (bool.TryParse((string?)parameter, out var reverse) && reverse)
            return count == 0;

        return count != 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new InvalidCastException();
    }
}