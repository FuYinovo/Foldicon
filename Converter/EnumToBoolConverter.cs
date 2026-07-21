using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Foldicon.Converter;

/// <summary>
/// 警告：必须使用 Binding 绑定，否则 DependencyProperty.UnsetValue 转换 Enum 抛出异常
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not System.Enum enumValue)
            throw new Exception($"{value} 不是 System.Enum");
        return parameter is not string paramStr
            ? throw new Exception($"ConverterParameter 必须是枚举成员名称字符串(区分大小写), 当前为 {parameter}")
            : enumValue.ToString().Equals(paramStr);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string paramStr || !System.Enum.TryParse(targetType, paramStr, out var enumValue))
            throw new Exception($"ConverterParameter 必须是枚举成员名称字符串(区分大小写), 当前为 {parameter}");
        if (value is not bool boolValue)
            throw new Exception($"{value} 必须是 System.Boolean");

        return boolValue ? enumValue : DependencyProperty.UnsetValue;
    }
}