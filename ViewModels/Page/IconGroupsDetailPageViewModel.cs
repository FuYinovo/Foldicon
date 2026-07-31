using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Helpers;
using Foldicon.Services;
using Foldicon.Views.Dialog;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BitmapIcon = Foldicon.Struct.BitmapIcon;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.ViewModels.Page;

public partial class IconGroupsDetailPageViewModel : ObservableObject
{
    public IconGroup Group = new();
    public XamlRoot? XamlRoot { get; set; }

    #region Filter

    [ObservableProperty] public partial ObservableCollection<BitmapIcon> FilteredIcons { get; set; } = [];
    [ObservableProperty] public partial string IconNameFilter { get; set; } = string.Empty;
    partial void OnIconNameFilterChanged(string value) => ApplyFilter();

    #endregion


    /// <summary>
    ///     导入图标
    /// </summary>
    [RelayCommand]
    public async void ImportIcon()
    {
        // 选取图标
        var icons = await StoragePicker.PickFiles(IconGroupService.IconExtensions,
            XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (icons.Length == 0) return;

        // 导入图标
        foreach (var icon in icons) Group.Add(icon);
    }

    /// <summary>
    ///     删除图标
    /// </summary>
    [RelayCommand]
    public async void DeleteIcon(BitmapIcon icon)
    {
        // 二次确认
        var dialog = new ContentDialog
        {
            RequestedTheme = App.MainWindow.GetRequestedTheme(),
            Title = $"确定删除\"{icon.FileName}\"吗？",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = new DescriptionDialog("此操作将无法从回收站恢复"),
            XamlRoot = XamlRoot
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return;

        // 延迟到下一个UI帧移除（让ContextFlyout先关闭），防止 E_FAIL (0x80004005) 崩溃
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Group.Remove(icon));
    }

    /// <summary>
    ///     打开图标
    /// </summary>
    [RelayCommand]
    public void OpenIconFile(BitmapIcon icon)
    {
        if (icon.FullPath is not null) Process.Start("explorer.exe", icon.FullPath);
    }

    /// <summary>
    ///     应用筛选
    /// </summary>
    private void ApplyFilter()
    {
        FilteredIcons.Clear();
        foreach (var icon in Group.Icons)
        {
            // 名称筛选
            if (!icon.FileName.Contains(IconNameFilter)) continue;

            // TODO)) 类型筛选


            FilteredIcons.Add(icon);
        }
    }

    /// <summary>
    ///    更新数据
    /// </summary>
    public void Update(IconGroup group)
    {
        Group = group;
        FilteredIcons = [.. group.Icons];
        Group.Icons.CollectionChanged += (_, _) => ApplyFilter(); // 更新UI（FilteredIcons 同步 _group.Icons)
    }
}