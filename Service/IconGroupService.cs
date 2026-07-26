using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Foldicon.Class;
using Foldicon.Struct;
using Foldicon.Tool;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Service;

public class IconGroupService
{
    public static readonly string[] SupportedLogoExtensions = [".jpg", ".png", ".ico", ".bmp"];
    public static readonly string[] SupportedIconExtensions = [".ico"];
    public static IconGroupService Instance { get; } = new();
    public readonly ObservableCollection<IconGroup> Groups = [];
    private IconGroupService() => Load();

    /// <summary>
    ///  从 Assets 加载所有图标组
    /// </summary>
    private void Load()
    {
        var groups = UriHelper.GetSubFoldersPathFromAssets("IconGroups");
        foreach (var groupPath in groups)
        {
            // 读取 Json 信息
            var jsonPath = Path.Combine(groupPath, "info.json");
            if (!Path.Exists(jsonPath)) continue;

            var jsonText = File.ReadAllText(jsonPath);
            var group = JsonSerializer.Deserialize<IconGroup>(jsonText);
            if (group is null) continue;
            group.Init(groupPath); // 初始化

            Groups.Add(group);
        }
    }

    /// <summary>
    /// 将所有图标组保存到 Assets
    /// </summary>
    private void Save()
    {
        foreach (var group in Groups)
        {
            // 保存 Json 信息
            var jsonPath = Path.Combine(group.RootPath, "info.json");
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(group));
        }
    }

    /// <summary>
    /// 添加一个图标组并保存到 Assets
    /// </summary>
    public async void Add(string name, string description, BitmapIcon logo)
    {
        // 创建目录
        var rootPath = UriHelper.GetFolderPathFromAssets("IconGroups");
        var folderPath = Path.Combine(rootPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(folderPath, "Icons"));

        // 创建 IconGroup 实例
        var group = new IconGroup(folderPath)
        {
            Name = name,
            Description = description,
            Logo = logo.Icon,
        };

        // 创建 Json
        var jsonPath = Path.Combine(folderPath, "info.json");
        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(group));

        // 创建 Logo
        var logoPath = Path.Combine(folderPath, "logo.png");
        File.Copy(logo.FullPath, logoPath);

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
}