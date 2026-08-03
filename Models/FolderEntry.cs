using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Helpers;
using Foldicon.Struct;
using Microsoft.UI.Xaml.Media;

namespace Foldicon.Models;

public partial class FolderEntry : ObservableObject
{
    private readonly string _fullPath;
    [ObservableProperty] public partial ImageSource CurrentIcon { get; set; }

    [ObservableProperty] public partial ObservableCollection<BitmapIcon> OptionalIcons { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    public partial int SelectedIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    public partial int AppliedIndex { get; set; }

    public bool IsSystemIcon { get; private set; }
    public string FolderName => Path.GetFileName(_fullPath);
    public bool IsSelectedIconChanged => SelectedIndex != AppliedIndex;

    public static async Task<FolderEntry> CreateAsync(string fullPath, byte[] currentIcon, List<BytesIcon> exeIcons)
    {
        var selection = await Task.Run(() => ComputeSelection(fullPath, exeIcons));
        return new FolderEntry(fullPath, currentIcon, exeIcons, selection);
    }

    private FolderEntry(string fullPath, byte[] currentIcon, List<BytesIcon> exeIcons, SelectionResult selection)
    {
        _fullPath = fullPath;
        var selectedIndex = selection.SelectedIndex;

        // OptionalIcons
        foreach (var icon in exeIcons)
        {
            // byte[] => BitmapImage
            OptionalIcons.Add(new BitmapIcon
            {
                FullPath = icon.FullPath,
                Icon = IconHelper.CreateBitmapImage(icon.Icon!)
            });
        }

        // 若文件夹当前图标不在 exeIcons 中，则单独创建条目
        if (selection is { IsSysIcon: false, ExtraIcon: not null })
        {
            OptionalIcons.Add(new BitmapIcon
            {
                FullPath = selection.ExtraIcon.FullPath,
                Icon = IconHelper.CreateBitmapImage(selection.ExtraIcon.Icon!)
            });
            selectedIndex = OptionalIcons.Count - 1;
        }

        // CurrentIcon
        CurrentIcon = selection.IsSysIcon
            ? IconHelper.CreateBitmapImage(currentIcon)
            : OptionalIcons[selectedIndex].Icon; // 避免多次创建 BitmapImage


        SelectedIndex = selectedIndex;
        AppliedIndex = selectedIndex;
        IsSystemIcon = selectedIndex == -1;
    }

    /// <summary>
    ///     应用当前选中的图标到文件夹
    /// </summary>
    public void Apply()
    {
        if (!IsSelectedIconChanged) return;

        var fullPath = OptionalIcons[SelectedIndex].FullPath;
        if (fullPath is not null) IconHelper.SetFolderIcon(_fullPath, fullPath);

        Refresh();
    }

    /// <summary>
    ///     刷新数据： AppliedIndex、IsSystemIcon、CurrentIcon
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
    ///     添加自定义图标并选中
    /// </summary>
    /// <param name="fullPath">图标完整路径</param>
    public void AddCustomIcon(string fullPath)
    {
        var icon = IconHelper.GetFileIcon(fullPath);
        if (icon is null) return;
        OptionalIcons.Add(new BitmapIcon { FullPath = fullPath, Icon = IconHelper.CreateBitmapImage(icon) });
        SelectedIndex = OptionalIcons.Count - 1;
    }

    /// <param name="SelectedIndex">文件夹图标在 exeIcon 的索引 (-1:不在)</param>
    /// <param name="ExtraIcon">文件夹图标不在 exeIcon 中情况下，需额外创建的图标条目</param>
    /// <param name="IsSysIcon">是否为系统默认图标</param>
    private sealed record SelectionResult(int SelectedIndex, BytesIcon? ExtraIcon, bool IsSysIcon);

    /// <summary>
    ///     获取当前文件夹图标在 exeIcons 中的索引
    /// </summary>
    /// <param name="fullPath">文件夹路径</param>
    /// <param name="exeIcons">文件夹中所有的exe图标</param>
    /// <returns><see cref="SelectionResult"/>实例</returns>
    private static SelectionResult ComputeSelection(string fullPath, List<BytesIcon> exeIcons)
    {
        // 尝试 UTF-8 读取路径
        var iconPath = IconHelper.GetFolderCustomIconPath(fullPath, Encoding.UTF8);
        // 若路径或图标为 null，尝试 GBK 读取路径
        if (iconPath is null || !TryGetIcon(iconPath, out var icon))
        {
            iconPath = IconHelper.GetFolderCustomIconPath(fullPath, Encoding.GetEncoding("GBK"));
            // GBK 也失败：文件夹为系统默认图标
            if (iconPath is null || !TryGetIcon(iconPath, out icon))
                return new SelectionResult(-1, null, true);
        }

        // 尝试在自定义图标可选项里查找
        for (var i = 0; i < exeIcons.Count; i++)
        {
            var path = exeIcons[i].FullPath;
            if (path != iconPath) continue;
            // 找到：直接返回其索引
            return new SelectionResult(i, null, false);
        }

        // 未找到：返回额外自定义图标字节，由构造函数补进 OptionalIcons 并选中
        return new SelectionResult(-1, new BytesIcon { FullPath = iconPath, Icon = icon }, false);

        bool TryGetIcon(string path, out byte[]? bitmap)
        {
            bitmap = IconHelper.GetFileIcon(path);
            return bitmap is not null;
        }
    }
}