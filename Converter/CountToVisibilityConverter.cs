using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converter;

public partial class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not int count) return Visibility.Visible;
        if (bool.TryParse((string?)parameter, out var reverse) && reverse)
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;

        return count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new InvalidCastException();
    }
}