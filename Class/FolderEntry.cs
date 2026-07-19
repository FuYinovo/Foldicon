using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Struct;
using Foldicon.Tool;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Class;

public partial class FolderEntry(string fullPath) : ObservableObject
{
    public readonly string FullPath = fullPath;
    public readonly string FolderName = Path.GetFileName(fullPath);

    [ObservableProperty] private ImageSource _currentIcon =
        IconHelper.GetIcon(fullPath, true) ?? throw new Exception($"读取{fullPath}的图标为null");
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
        CurrentIcon = IconHelper.GetIcon(FullPath,true) ?? new BitmapImage();
    }
}