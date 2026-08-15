using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Contracts;
using Foldicon.Helpers;
using Foldicon.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using IconGroup = Foldicon.Models.IconGroup;
using System.Threading.Tasks;
using Foldicon.Models.Icon;

namespace Foldicon.ViewModels.Page;

public partial class IconGroupsDetailPageViewModel(
    IDialogService dialogService,
    IOptionService optionService,
    INotificationService notificationService)
    : ObservableObject
{
    public IconGroup Group = new();

    #region Filter

    [ObservableProperty] public partial ObservableCollection<IFolderIcon> FilteredIcons { get; set; } = [];
    [ObservableProperty] public partial string IconNameFilter { get; set; } = string.Empty;
    partial void OnIconNameFilterChanged(string value) => ApplyFilter();
    private void IconsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ApplyFilter();

    #endregion

    /// <summary>
    ///     导入图标
    /// </summary>
    [RelayCommand]
    private async Task ImportIcon()
    {
        Group.Icons.CollectionChanged -= IconsOnCollectionChanged; // 避免导入大量图标时频频更新UI

        // 选取图标
        var iconPaths = await StoragePicker.PickFiles(IconGroupService.IconExtensions, dialogService.WindowId);

        // 导入图标
        foreach (var path in iconPaths)
        {
            var icon = new FolderFileIcon(path);
            // 检查是否已导入重复文件名的图标
            var check = IconGroupService.IsFileIconExists(Group, icon);
            if (check.IsExists)
            {
                var result = await dialogService.ShowMessageAsync(
                    "是否覆盖重复图标？",
                    $"{Group.Name}图标组中已经存在{icon.ShortDisplayName}图标，是否将其覆盖？",
                    true
                );
                if (result == ContentDialogResult.Primary) Group.Remove(check.existedIcons);
                else continue;
            }

            Group.Add(icon);
        }

        Group.Icons.CollectionChanged += IconsOnCollectionChanged;
        IconsOnCollectionChanged(null, null!);
    }

    /// <summary>
    ///     删除图标
    /// </summary>
    [RelayCommand]
    private async Task DeleteIcon(IFolderIcon icon)
    {
        // 二次确认
        var result = await dialogService.ShowMessageAsync($"确定删除\"{icon.DisplayName}\"吗？", "此操作将无法从回收站恢复", true);
        if (result == ContentDialogResult.None) return;

        // 延迟到下一个UI帧移除（让ContextFlyout先关闭），防止 E_FAIL (0x80004005) 崩溃
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Group.Remove(icon));
    }

    /// <summary>
    ///     打开图标
    /// </summary>
    [RelayCommand]
    private static void OpenIconFile(FolderFileIcon icon)
    {
        Process.Start("explorer.exe", icon.FilePath);
    }

    /// <summary>
    ///     打开图标所处文件夹
    /// </summary>
    [RelayCommand]
    private static void OpenIconFolder(FolderFileIcon icon)
    {
        Process.Start("explorer.exe", $"/select, {icon.FilePath}");
    }

    /// <summary>
    ///     给多个文件夹应用同一个图标
    /// </summary>
    [RelayCommand]
    private void ApplyToFolders((string[] folders, IFolderIcon icon) args)
    {
        if (args.folders.Length == 0) return;
        foreach (var folder in args.folders) args.icon.ApplyTo(folder);

        notificationService.SendToApp(
            "成功设置图标",
            $"成功将{args.folders.Length}个文件夹的设为{args.icon.ShortDisplayName}",
            InfoBarSeverity.Success);
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
            var comparison = optionService.Options.IsCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            if (!icon.DisplayName.Contains(IconNameFilter, comparison)) continue;

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
        Group.Icons.CollectionChanged += IconsOnCollectionChanged;
    }
}