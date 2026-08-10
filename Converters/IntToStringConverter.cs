using System;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converters;

/// <summary>
///     将 int 转换为 string
/// </summary>
public partial class IntToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        return value?.ToString()??throw new Exception("Cannot convert null value to int.");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}