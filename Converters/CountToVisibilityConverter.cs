using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converters;

/// <summary>
/// 将数量转换为可视性
/// <example>当数字为0时不可见，可以传入true反转此规则</example>
/// </summary>
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