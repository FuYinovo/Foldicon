using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Contracts;
using Foldicon.Converters;
using Foldicon.Helpers;
using Foldicon.Services;
using Foldicon.Struct;

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

    #region JsonIgnore

    [JsonIgnore] public string RootPath = string.Empty; // 在 Init() 初始化
    [JsonIgnore] public ObservableCollection<IFolderIcon> Icons { get; set; } = []; // 在 Init() 初始化
    [JsonIgnore] [ObservableProperty] public partial IconGroupLogo Logo { get; set; }

    #endregion

    #region Json

    [ObservableProperty] public partial string Name { get; set; } = string.Empty;
    [ObservableProperty] public partial string Description { get; set; } = string.Empty;

    [JsonConverter(typeof(StringCategoryJsonConverter))]
    public ObservableCollection<Category<string>> Categories { get; set; } = [];

    #endregion

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
            if(IconHelper.TryGetFileIcon(iconPath,Const.GroupIconSize, out var icon)) Icons.Add(icon);
        }
    }

    /// <summary>
    ///     导入一个图标
    /// </summary>
    /// <param name="icon">图标完整路径</param>
    public void Add(IFolderIcon icon)
    {
        icon.Save(Path.Combine(RootPath, IconGroupService.IconsFolderName));
        Icons.Add(icon);
    }

    /// <summary>
    ///     移除一个图标
    /// </summary>
    /// <param name="icon">图标实例</param>
    public void Remove(IFolderIcon icon)
    {
        icon.Remove(Path.Combine(RootPath, IconGroupService.IconsFolderName));
        Icons.Remove(icon);
    }

    public override string ToString() => Name;
}