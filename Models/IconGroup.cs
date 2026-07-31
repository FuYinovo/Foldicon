using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Converters;
using Foldicon.Services;
using Foldicon.Struct;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Models;

/// <summary>
///     <para>负责管理图标（读写操作）</para>
/// </summary>
public partial class IconGroup : ObservableObject
{
    /// <summary>
    ///     创建一个图标组
    /// </summary>
    /// <param name="path">存储路径</param>
    public IconGroup(string path)
    {
        Init(path);
    }

    /// <summary>
    ///     Json 解析创建一个图标组, 需要手动调用 Init()
    /// </summary>
    [JsonConstructor]
    public IconGroup()
    {
    }

    [JsonIgnore] public string RootPath = string.Empty; // 在 Init() 初始化
    [JsonIgnore] public ObservableCollection<BitmapIcon> Icons { get; set; } = []; // 在 Init() 初始化
    [ObservableProperty] public partial string Name { get; set; } = string.Empty;
    [ObservableProperty] public partial string Description { get; set; } = string.Empty;

    [JsonIgnore] [ObservableProperty] public partial ImageSource Logo { get; set; } = new BitmapImage();

    [JsonConverter(typeof(StringCategoryJsonConverter))]
    public ObservableCollection<Category<string>> Categories { get; set; } = [];

    /// <summary>
    ///     加载所有图标
    /// </summary>
    /// <param name="rootPath">图标组根目录的完整路径</param>
    public void Init(string rootPath)
    {
        RootPath = rootPath;

        var iconsPath = Path.Combine(rootPath, IconGroupService.IconsFolderName);
        if (!Directory.Exists(iconsPath)) Directory.CreateDirectory(iconsPath);
        foreach (var iconPath in Directory.GetFiles(iconsPath))
        {
            var bitmap = new BitmapImage(new Uri(iconPath));
            Icons.Add(new BitmapIcon { FullPath = iconPath, Icon = bitmap });
        }
    }

    /// <summary>
    ///     导入一个图标
    /// </summary>
    /// <param name="icon">图标完整路径</param>
    public void Add(string icon)
    {
        // 复制覆盖
        var targetPath = Path.Combine(RootPath, IconGroupService.IconsFolderName, Path.GetFileName(icon));
        if (File.Exists(icon))
        {
            if (File.Exists(targetPath)) File.Delete(targetPath);
            File.Copy(icon, targetPath);
        }

        // 读取 -> BitmapIcon 实例
        var bitmap = new BitmapImage(new Uri(targetPath));
        Icons.Add(new BitmapIcon { FullPath = targetPath, Icon = bitmap });
    }

    /// <summary>
    ///     移除一个图标
    /// </summary>
    /// <param name="icon">图标实例</param>
    public void Remove(BitmapIcon icon)
    {
        var path = icon.FullPath;
        if (path is null) return;
        // 删除文件
        if (File.Exists(path)) File.Delete(path);
        // 删除已加载实例
        Icons.Remove(icon);
    }

    public override string ToString()
    {
        return Name;
    }
}