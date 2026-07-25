using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Struct;
using Foldicon.Tool;
using Microsoft.UI.Xaml.Media;

namespace Foldicon.Class;

public partial class FolderEntry : ObservableObject
{
    [ObservableProperty] private ImageSource _currentIcon;
    [ObservableProperty] private ObservableCollection<BitmapIcon> _optionalIcons = [];

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    private int _selectedIndex = -1;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    private int _appliedIndex = -1;

    private readonly string _fullPath;
    public bool IsSystemIcon { get; private set; }
    public string FolderName => Path.GetFileName(_fullPath);
    public bool IsSelectedIconChanged => SelectedIndex != AppliedIndex;


    /// <param name="fullPath">文件夹完整路径</param>
    /// <param name="currentIcon">文件夹图标（null则自动获取）</param>
    /// <param name="exeIcons">可选exe路径及图标(null则自动获取)</param>
    public FolderEntry(string fullPath, byte[]? currentIcon = null, List<BytesIcon>? exeIcons = null)
    {
        _fullPath = fullPath;

        // OptionalIcons
        var icons =
            exeIcons ??
            IconHelper.GetExeIconsAsync(fullPath, false).Result;
        foreach (var icon in icons)
            OptionalIcons.Add(new BitmapIcon
                { FullPath = icon.FullPath, Icon = IconHelper.CreateBitmapImage(icon.Icon!) }); // 此处不可能 null

        // CurrentIcon
        CurrentIcon = currentIcon is null
            ? IconHelper.CreateBitmapImage(IconHelper.GetFolderIcon(fullPath) ??
                                           throw new Exception($"读取'{fullPath}'文件夹图标失败"))
            : IconHelper.CreateBitmapImage(currentIcon);

        InitIconSelectorIndex();
        Refresh();
    }

    /// <summary>
    /// 应用当前选中的图标到文件夹
    /// </summary>
    public void Apply()
    {
        if (!IsSelectedIconChanged) return;
        IconHelper.SetFolderIcon(_fullPath, OptionalIcons[SelectedIndex].FullPath);
        Refresh();
    }

    /// <summary>
    /// 刷新数据： AppliedIndex、IsSystemIcon、CurrentIcon
    /// </summary>
    private void Refresh()
    {
        AppliedIndex = SelectedIndex;
        IsSystemIcon = SelectedIndex == -1; // 若 SelectedIndex 为 -1，说明文件夹使用系统默认图标

        if (SelectedIndex >= 0 && SelectedIndex < OptionalIcons.Count)
        {
            CurrentIcon = OptionalIcons[SelectedIndex].Icon;
        }
        else
        {
            var icon = IconHelper.GetFolderIcon(_fullPath) ?? throw new Exception($"读取'{_fullPath}'文件夹图标失败");
            CurrentIcon = IconHelper.CreateBitmapImage(icon);
        }
    }

    /// <summary>
    /// 添加自定义图标并选中
    /// </summary>
    /// <param name="fullPath">图标完整路径</param>
    public void AddCustomIcon(string fullPath)
    {
        var icon = IconHelper.GetFileIcon(fullPath);
        if (icon is null) return;
        OptionalIcons.Add(new BitmapIcon { FullPath = fullPath, Icon = IconHelper.CreateBitmapImage(icon) });
        SelectedIndex = OptionalIcons.Count - 1;
    }

    /// <summary>
    /// 读 desktop.ini，把当前已应用图标定位到可选列表的索引
    /// </summary>
    public void InitIconSelectorIndex()
    {
        // 尝试 UTF-8 读取路径
        var iconPath = IconHelper.GetFolderCustomIconPath(_fullPath, Encoding.UTF8);
        // 若路径或图标为 null，尝试 GBK 读取路径
        if (iconPath is null || !TryGetIcon(iconPath, out var icon))
        {
            iconPath = IconHelper.GetFolderCustomIconPath(_fullPath, Encoding.GetEncoding("GBK"));
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
        OptionalIcons.Add(new BitmapIcon { FullPath = iconPath, Icon = IconHelper.CreateBitmapImage(icon) });
        SelectedIndex = OptionalIcons.Count - 1;

        return;

        bool TryGetIcon(string path, out byte[]? bitmap)
        {
            bitmap = IconHelper.GetFileIcon(path);
            return bitmap is not null;
        }
    }
}