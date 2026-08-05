using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Foldicon.Struct;

namespace Foldicon.Helpers;

public static class LocalizationHelper
{
    /// <summary>
    ///     获取一个枚举类的所有成员，将<see cref="DescriptionAttribute"/>作为本地化字符串
    /// </summary>
    public static List<LocalizationItem<TEnum>> GetEnums<TEnum>() where TEnum : struct, Enum
    {
        var enums = Enum.GetValues<TEnum>();
        return
        [
            .. enums.Select(enumValue => new LocalizationItem<TEnum>
                {
                    Item = enumValue,
                    Localization = GetEnumDescription(enumValue) ?? enumValue.ToString()
                }
            )
        ];
    }


    private static string? GetEnumDescription<TEnum>(TEnum enumValue)
    {
        return enumValue
            ?.GetType()
            .GetField(enumValue.ToString() ?? string.Empty)
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description;
    }
}