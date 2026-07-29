using System;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converter;

/// <summary>
///     将 int 转换为 string（失败返回空白）
/// </summary>
public partial class IntToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}