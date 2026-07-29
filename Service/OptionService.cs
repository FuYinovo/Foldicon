using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Enum;
using Foldicon.Tool;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace Foldicon.Service;

public partial class OptionService : ObservableObject
{
    private readonly bool _isLoaded;
    private readonly string _jsonFilePath = UriHelper.GetFilePathFromAssets("options.json");
    private JsonNode _jsonNode;


    private OptionService()
    {
        // 若 JsonNode 为空，
        // LoadOptions() 保持默认值
        // SaveOptionsAsync() 自动创建结构
        var jsonText = File.ReadAllText(_jsonFilePath);
        _jsonNode = JsonNode.Parse(jsonText) ?? new JsonObject();

        LoadOptions();
        _isLoaded = true;
    }

    public static OptionService Instance { get; } = new();


    /// <summary>
    ///     从 Json 文件加载设置选项
    /// </summary>
    private void LoadOptions()
    {
        // 若获取失败，则维持属性默认值不变
        if (JsonHelper.TryGetJsonValue<int>(_jsonNode, out var themeIndex, "appearance", "theme"))
            ThemeOptionIndex = themeIndex;
        if (JsonHelper.TryGetJsonValue<int>(_jsonNode, out var backdropIndex, "appearance", "backdrop"))
            BackdropOptionIndex = backdropIndex;
    }

    /// <summary>
    ///     将设置选项保存到 Json 文件
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="keys">键</param>
    private async Task SaveOptionsAsync<TValue>(TValue value, params string[] keys)
    {
        // 设置 Json 值
        JsonHelper.TrySetJsonValue(ref _jsonNode, value, keys);

        // 保存到文件
        // （从配置文件读取完成前除外，防止数据被默认值覆盖）
        if (_isLoaded) await File.WriteAllTextAsync(_jsonFilePath, _jsonNode.ToJsonString());
    }

    /// <summary>
    ///     获取一个枚举成员的 Description 值
    /// </summary>
    /// <param name="enumValue">枚举成员</param>
    /// <returns>成功：Description 字符串；失败：直接 ToString()</returns>
    private static string GetEnumDescription(System.Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (field is null) return enumValue.ToString();

        var attr = field.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? enumValue.ToString();
    }
}

public partial class OptionService // 设置选项属性
{
    public Dictionary<ThemeOptionEnum, string> ThemeOptions { get; } = new()
    {
        { ThemeOptionEnum.System, GetEnumDescription(ThemeOptionEnum.System) },
        { ThemeOptionEnum.Dark, GetEnumDescription(ThemeOptionEnum.Dark) },
        { ThemeOptionEnum.Light, GetEnumDescription(ThemeOptionEnum.Light) }
    };

    public Dictionary<BackdropOptionEnum, string> BackdropOptions { get; } = new()
    {
        { BackdropOptionEnum.Mica, GetEnumDescription(BackdropOptionEnum.Mica) },
        { BackdropOptionEnum.MicaAlt, GetEnumDescription(BackdropOptionEnum.MicaAlt) },
        { BackdropOptionEnum.Acrylic, GetEnumDescription(BackdropOptionEnum.Acrylic) }
    };

    [ObservableProperty] public partial int ThemeOptionIndex { get; set; }

    [ObservableProperty] public partial int BackdropOptionIndex { get; set; }

    async partial void OnThemeOptionIndexChanged(int value)
    {
        var option = ThemeOptions.Keys.ElementAt(value);
        App.MainWindow.TrySetTheme(option switch
        {
            ThemeOptionEnum.System => ElementTheme.Default,
            ThemeOptionEnum.Dark => ElementTheme.Dark,
            ThemeOptionEnum.Light => ElementTheme.Light,
            _ => throw new ArgumentOutOfRangeException()
        });

        await SaveOptionsAsync(ThemeOptionIndex, "appearance", "theme");
    }

    async partial void OnBackdropOptionIndexChanged(int value)
    {
        var option = BackdropOptions.Keys.ElementAt(value);
        switch (option)
        {
            case BackdropOptionEnum.Mica:
                App.MainWindow.TrySetMicaBackdrop(MicaKind.Base);
                break;
            case BackdropOptionEnum.MicaAlt:
                App.MainWindow.TrySetMicaBackdrop(MicaKind.BaseAlt);
                break;
            case BackdropOptionEnum.Acrylic:
                App.MainWindow.TrySetAcrylicBackdrop();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await SaveOptionsAsync(BackdropOptionIndex, "appearance", "backdrop");
    }
}