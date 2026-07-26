using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Converter;
using Foldicon.Service;
using Foldicon.Struct;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Class;

public partial class IconGroup : ObservableObject
{
    [JsonIgnore] public ObservableCollection<BitmapIcon> Icons { get; set; } = []; // 在 Init() 初始化
    [JsonIgnore] public string RootPath = string.Empty; // 在 Init() 初始化

    [property: JsonIgnore] [ObservableProperty]
    private ImageSource _logo = new BitmapImage();

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;

    [JsonConverter(typeof(StringCategoryJsonConverter))]
    public ObservableCollection<Category<string>> Categories { get; set; } = [];


    /// <summary>
    /// 创建一个图标组
    /// </summary>
    /// <param name="path">存储路径</param>
    public IconGroup(string path) => RootPath = path;


    /// <summary>
    /// Json 解析创建一个图标组, 需要手动调用 Init()
    /// </summary>
    [JsonConstructor]
    public IconGroup()
    {
    }


    /// <summary>
    /// 初始化 - 从根目录加载Logo和图标
    /// </summary>
    /// <param name="rootPath">图标组根目录的完整路径</param>
    public void Init(string rootPath)
    {
        RootPath = rootPath;

        // 加载 Logo
        foreach (var extension in IconGroupService.SupportedLogoExtensions) // 尝试不同格式
        {
            var iconPath = Path.Combine(RootPath, $"logo{extension}");
            if (!File.Exists(iconPath)) continue;
            Logo = new BitmapImage(new Uri(iconPath));
            break;
        }

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
    /// <param name="icon">图标完整路径</param>
    public void Add(string icon)
    {
        // 复制到目录
        var targetPath = Path.Combine(RootPath, "Icons", Path.GetFileName(icon));
        File.Copy(icon, targetPath);

        // 读取 -> BitmapIcon 实例
        var bitmap = new BitmapImage(new Uri(targetPath));
        Icons.Add(new BitmapIcon { FullPath = targetPath, Icon = bitmap });
    }

    /// <summary>
    /// 移除一个图标
    /// </summary>
    /// <param name="icon">图标实例</param>
    public void Remove(BitmapIcon icon)
    {
        // 删除文件
        File.Delete(icon.FullPath);

        // 删除已加载实例
        Icons.Remove(icon);
    }

    public override string ToString()
    {
        return Name;
    }
}