using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Struct;
using Foldicon.Tool;
using Microsoft.UI.Xaml.Media;

namespace Foldicon.Class;

public partial class FolderEntry(string fullPath) : ObservableObject
{
    public readonly string FullPath = fullPath;
    public string FolderName => Path.GetFileName(FullPath);

    [ObservableProperty] private ImageSource _currentIcon =
        IconHelper.GetFolderIcon(fullPath) ?? throw new Exception($"读取'{fullPath}'文件夹图标失败");

    [ObservableProperty] private ObservableCollection<FileIcon> _optionalIcons = new(IconHelper.GetExeIcons(fullPath));
    [ObservableProperty] private FileIcon? _selectedIcon;


    /// <summary>
    /// 应用当前选中的图标（<see cref="SelectedIcon"/>）到文件夹
    /// </summary>
    public void Apply()
    {
        if (SelectedIcon is null) return;
        IconHelper.SetFolderIcon(FullPath, SelectedIcon.Value.FullPath);
        Refresh();
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    private void Refresh()
    {
        CurrentIcon = IconHelper.GetFolderIcon(FullPath) ?? throw new Exception($"读取'{FullPath}'文件夹图标失败");
    }
}