using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Converter;
using Foldicon.Struct;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Class;

public partial class IconGroup : ObservableObject
{
    [JsonIgnore] public ObservableCollection<BitmapIcon> Icons { get; set; } = []; // 在 Init() 初始化
    [JsonIgnore] public string RootPath = string.Empty; // 在 Init() 初始化
    [JsonIgnore] [ObservableProperty] private ImageSource _logo = new BitmapImage();
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;

    [JsonConverter(typeof(StringCategoryJsonConverter))]
    public ObservableCollection<Category<string>> Categories { get; set; } = [];


    /// <summary>
    /// 初始化 - 从根目录加载Logo和图标
    /// </summary>
    /// <param name="rootPath">图标组根目录的完整路径</param>
    public void Init(string rootPath)
    {
        Categories.Insert(0, new Category<string> { Name = "全部" });
        RootPath = rootPath;

        // 加载 Logo
        string[] supported = [".png", ".ico"]; // 尝试不同格式
        var loaded = false;
        foreach (var extension in supported)
        {
            var iconPath = Path.Combine(RootPath, $"logo{extension}");
            if (!File.Exists(iconPath)) continue;
            Logo = new BitmapImage(new Uri(iconPath));
            loaded = true;
            break;
        }

        if (!loaded) Logo = new BitmapImage();


        // 加载所有图标
        var iconsPath = Path.Combine(rootPath, "Icons");
        if (!Directory.Exists(iconsPath)) Directory.CreateDirectory(iconsPath);
        foreach (var iconPath in Directory.GetFiles(iconsPath))
        {
            var bitmap = new BitmapImage(new Uri(iconPath));
            Icons.Add(new BitmapIcon { FullPath = iconPath, Icon = bitmap });
        }
    }

    /// <summary>
    /// 导入一个图标
    /// </summary>
    /// <param name="icon">BitmapIcon 实例</param>
    public void Add(BitmapIcon icon)
    {
        Icons.Add(icon);
    }

    /// <summary>
    /// 移除一个图标
    /// </summary>
    /// <param name="index">索引</param>
    public void Remove(int index)
    {
        if (index >= 0 && index < Icons.Count) Icons.RemoveAt(index);
    }

    public override string ToString()
    {
        return Name;
    }
}