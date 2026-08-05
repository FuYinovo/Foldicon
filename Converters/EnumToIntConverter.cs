using System;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converters;

/// <summary>
///     将枚举成员转换为其对应的Int值
/// </summary>
public class EnumToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not Enum enumValue
            ? throw new Exception($"{value}必须是一个枚举成员")
            : System.Convert.ToInt32(enumValue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new Exception("不能将一个Int值转换为确定的枚举成员");
    }
}