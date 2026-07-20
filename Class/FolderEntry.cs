using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Struct;
using Foldicon.Tool;
using IniFileSharp;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Class;

public partial class FolderEntry : ObservableObject
{
    public readonly string FullPath;
    public string FolderName => Path.GetFileName(FullPath);

    [ObservableProperty] private ImageSource _currentIcon;
    [ObservableProperty] private ObservableCollection<FileIcon> _optionalIcons;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    private int _selectedIndex = -1;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    private int _appliedIndex = -1;

    public bool IsSelectedIconChanged => SelectedIndex != AppliedIndex;

    public FolderEntry(string fullPath)
    {
        FullPath = fullPath;
        CurrentIcon = IconHelper.GetFolderIcon(fullPath) ?? throw new Exception($"读取'{fullPath}'文件夹图标失败");
        OptionalIcons = new ObservableCollection<FileIcon>(IconHelper.GetExeIcons(fullPath));

        InitIconSelectorIndex();
        AppliedIndex = SelectedIndex;
    }

    /// <summary>
    /// 应用当前选中的图标到文件夹
    /// </summary>
    public void Apply()
    {
        if (!IsSelectedIconChanged) return;
        IconHelper.SetFolderIcon(FullPath, OptionalIcons[SelectedIndex].FullPath);
        AppliedIndex = SelectedIndex;
        Refresh();
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    private void Refresh()
    {
        CurrentIcon = IconHelper.GetFolderIcon(FullPath) ?? throw new Exception($"读取'{FullPath}'文件夹图标失败");
    }

    /// <summary>
    /// 添加自定义图标并选中
    /// </summary>
    /// <param name="fullPath">图标完整路径</param>
    public void AddCustomIcon(string fullPath)
    {
        var icon = IconHelper.GetFileIcon(fullPath);
        if (icon is null) return;
        OptionalIcons.Add(new FileIcon(fullPath, icon));
        SelectedIndex = OptionalIcons.Count - 1;
    }

    /// <summary>
    /// 读 desktop.ini，把当前已应用图标定位到可选列表的索引
    /// </summary>
    public void InitIconSelectorIndex()
    {
        // 读取 desktop.ini 中指定的文件夹图标
        var iniPath = Path.Combine(FullPath, "desktop.ini");
        if (!File.Exists(iniPath)) return;

        // 尝试 UTF-8 读取路径
        var iconPath = GetIconPath(iniPath, Encoding.UTF8);
        // 若路径或图标为 null，尝试 GBK 读取路径
        if (iconPath is null || !TryGetIcon(iconPath, out var icon))
        {
            iconPath = GetIconPath(iniPath, Encoding.GetEncoding("GBK"));
            // GBK 也失败，直接返回
            if (iconPath is null || !TryGetIcon(iconPath, out icon)) return;
        }

        // 尝试在自定义图标可选项里查找
        for (var i = 0; i < OptionalIcons.Count; i++)
        {
            var path = OptionalIcons[i].FullPath;
            if (path != iconPath) continue;
            // 找到：直接设为初始选择项
            SelectedIndex = i;
            return;
        }

        // 未找到：创建新的自定义图标可选项
        if (icon is null) return;
        OptionalIcons.Add(new FileIcon(iconPath, icon));
        SelectedIndex = OptionalIcons.Count - 1;

        return;

        string? GetIconPath(string iniFile, Encoding encoding)
        {
            var iniSharp = new IniSharp(iniFile, encoding);
            try
            {
                var resource = iniSharp.GetValue(".ShellClassInfo", "IconResource");
                return resource is null ? null : ConsumeResource(resource);
            }
            catch (ArgumentNullException)
            {
                // IniSharp.GetValue 在键/节不存在且 defaultValue 为 null 时会抛出 ArgumentNullException
                return null;
            }
        }

        string? ConsumeResource(string resource)
        {
            var path = resource.Split(",").FirstOrDefault(string.Empty);
            if (string.IsNullOrEmpty(path)) return null;

            // 忽略 .dll 图标
            if (path.EndsWith(".dll")) return null;

            // 相对路径 -> 绝对路径
            if (!Path.IsPathRooted(path)) path = Path.Combine(FullPath, path);

            return path;
        }

        bool TryGetIcon(string path, out BitmapImage? bitmap)
        {
            bitmap = IconHelper.GetFileIcon(path);
            return bitmap is not null;
        }
    }
}