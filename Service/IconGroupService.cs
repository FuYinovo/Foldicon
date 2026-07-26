using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Foldicon.Class;
using Foldicon.Struct;
using Foldicon.Tool;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Service;

/// <summary>
/// 负责管理图标组的名称、简介、Logo（读写操作）
/// </summary>
public class IconGroupService
{
    public const string LogoFileName = "logo.png";
    public const string InfoFileName = "info.json";
    public const string IconsFolderName = "Icons";
    public const string RootFolderName = "IconGroups";
    public static readonly string[] IconExtensions = [".ico"];
    public static readonly string[] LogoExtensions = [".ico", ".png", ".jpg", ".jpeg", ".bmp"];
    public static readonly string RootPath = UriHelper.GetFolderPathFromAssets(RootFolderName);
    public static IconGroupService Instance { get; } = new();
    public readonly ObservableCollection<IconGroup> Groups = [];
    private IconGroupService() => Load();

    /// <summary>
    ///  从 Assets 加载所有图标组
    /// </summary>
    private void Load()
    {
        var groups = UriHelper.GetSubFoldersPathFromAssets(RootFolderName);
        foreach (var groupPath in groups)
        {
            // 读取 Json 信息
            var jsonPath = Path.Combine(groupPath, InfoFileName);
            if (!Path.Exists(jsonPath)) continue;

            var jsonText = File.ReadAllText(jsonPath);
            var group = JsonSerializer.Deserialize<IconGroup>(jsonText);
            if (group is null) continue;

            // 读取 Logo
            var logoPath = Path.Combine(groupPath, LogoFileName);
            if (File.Exists(logoPath)) group.Logo = new BitmapImage(new Uri(logoPath));

            // 初始化
            group.Init(groupPath);

            // 添加到 Service
            Groups.Add(group);
        }
    }

    /// <summary>
    /// 添加一个图标组并保存到 Assets
    /// </summary>
    public async void Add(string name, string description, BitmapIcon logo)
    {
        // 创建目录
        var folderPath = Path.Combine(RootPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(folderPath, IconsFolderName));

        // 创建 IconGroup 实例
        var group = new IconGroup(folderPath)
        {
            Name = name,
            Description = description,
            Logo = logo.Icon,
        };

        // 创建 Json
        var jsonPath = Path.Combine(folderPath, InfoFileName);
        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(group));

        // 创建 Logo
        var logoPath = Path.Combine(folderPath, LogoFileName);
        if (logo.FullPath is not null) File.Copy(logo.FullPath, logoPath);

        // 添加到 Service
        Groups.Add(group);
    }

    /// <summary>
    /// 从 Assets 删除一个导入的图标组
    /// </summary>
    /// <param name="group"><see cref="Groups"/>>图标组实例</param>
    public void Remove(IconGroup group)
    {
        // 删除文件夹
        Directory.Delete(group.RootPath, true);

        // 移除已加载的实例
        Groups.Remove(group);
    }

    /// <summary>
    /// 编辑并保存一个已导入到 Assets 的图标组信息
    /// </summary>
    /// <param name="group">图标组实例</param>
    /// <param name="name">新名称</param>
    /// <param name="description">新简介</param>
    /// <param name="logo">新Logo</param>
    public async void Edit(IconGroup group, string name, string description, BitmapIcon logo)
    {
        if (!Groups.Contains(group)) return;

        // 修改属性
        group.Name = name;
        group.Description = description;
        group.Logo = logo.Icon;

        // 覆盖 Logo 文件
        var logoPath = Path.Combine(group.RootPath, LogoFileName);
        if (File.Exists(logoPath)) File.Delete(logoPath);
        if (logo.FullPath is not null) // 用户可能未修改 Logo
            File.Copy(logo.FullPath, logoPath);


        // 覆盖 Json 文件
        var jsonPath = Path.Combine(group.RootPath, InfoFileName);
        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(group));
    }
}