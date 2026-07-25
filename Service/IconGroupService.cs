using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Foldicon.Class;
using Foldicon.Tool;
using Microsoft.UI.Xaml.Media;

namespace Foldicon.Service;

public class IconGroupService
{
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
            if(!Path.Exists(jsonPath)) continue;

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
    /// 添加一个图标组
    /// </summary>
    public void Add(string name, string description, ImageSource logo)
    {
        Groups.Add(new IconGroup { Name = name, Description = description, Logo = logo });
    }

    /// <summary>
    /// 删除一个导入的图标组
    /// </summary>
    /// <param name="index"><see cref="Groups"/>>索引</param>
    public void Remove(int index)
    {
        if (index >= 0 && index < Groups.Count) Groups.RemoveAt(index);
    }
}