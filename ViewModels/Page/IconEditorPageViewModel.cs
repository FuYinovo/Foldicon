using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Contracts;
using Foldicon.Enums;
using Foldicon.Helpers;
using FolderEntry = Foldicon.Models.FolderEntry;

namespace Foldicon.ViewModels.Page;

public partial class IconEditorPageViewModel(IDialogService dialogService, IOptionService optionService)
    : ObservableObject
{
    #region Status Properties

    [ObservableProperty] public partial string ParentFolder { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsSubFoldersLoaded { get; set; }

    [ObservableProperty] public partial bool IsSubFoldersLoading { get; set; }

    /// <summary>
    ///     UI 显示的筛选后列表
    /// </summary>
    public ObservableCollection<FolderEntry> FilteredSubFolders { get; } = [];

    /// <summary>
    ///     完整数据源（未筛选）
    /// </summary>
    private List<FolderEntry> SubFolders { get; } = [];

    #endregion

    #region Filter Propertries

    [ObservableProperty] public partial string NameFilter { get; set; } = string.Empty;

    [ObservableProperty] public partial TypeFilterEnum TypeFilter { get; set; } = TypeFilterEnum.All;

    [ObservableProperty] public partial StatusFilterEnum StatusFilter { get; set; } = StatusFilterEnum.All;

    partial void OnTypeFilterChanged(TypeFilterEnum value) => ApplyFilter();

    partial void OnStatusFilterChanged(StatusFilterEnum value) => ApplyFilter();

    partial void OnNameFilterChanged(string value) => ApplyFilter();

    #endregion

    #region Command

    /// <summary>
    ///     加载指定文件夹的子文件夹图标
    /// </summary>
    [RelayCommand]
    public async Task LoadFolderAsync()
    {
        var fullPath = await StoragePicker.PickFolder(dialogService.WindowId);
        if (fullPath is not null)
        {
            ParentFolder = fullPath;
            IsSubFoldersLoaded = false;
            IsSubFoldersLoading = true;
            await RefreshSubfoldersAsync();
            IsSubFoldersLoaded = true;
            IsSubFoldersLoading = false;
        }
    }

    /// <summary>
    ///     将所有文件夹的选中图标应用到磁盘
    /// </summary>
    [RelayCommand]
    private void ApplyAll()
    {
        foreach (var entry in SubFolders) entry.Apply();
    }

    #endregion

    #region Private

    /// <summary>
    ///     根据当前筛选条件刷新 FilteredSubFolders
    /// </summary>
    private void ApplyFilter()
    {
        FilteredSubFolders.Clear();
        foreach (var folder in SubFolders)
        {
            // 名称筛选
            var comparison = optionService.Options.IsCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            if (!folder.FolderName.Contains(NameFilter, comparison)) continue;

            // 类型筛选
            switch (TypeFilter)
            {
                case TypeFilterEnum.All:
                    break;
                case TypeFilterEnum.System:
                    if (!folder.IsSystemIcon) continue;
                    break;
                case TypeFilterEnum.Custom:
                    if (folder.IsSystemIcon) continue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // 状态筛选
            switch (StatusFilter)
            {
                case StatusFilterEnum.All:
                    break;
                case StatusFilterEnum.Unmodified:
                    if (folder.IsSelectedIconChanged) continue;
                    break;
                case StatusFilterEnum.Unapplied:
                    if (!folder.IsSelectedIconChanged) continue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            FilteredSubFolders.Add(folder);
        }
    }

    /// <summary>
    ///     刷新子文件夹列表
    /// </summary>
    private async Task RefreshSubfoldersAsync()
    {
        if (!Directory.Exists(ParentFolder)) return;
        SubFolders.Clear();

        // 并行获取子文件夹图标
        foreach (var path in Directory.GetDirectories(ParentFolder))
        {
            if (!IconHelper.TryGetFolderIcon(path, out var icon)) continue;
            var maxDepth = optionService.Options.IsRecursive ? optionService.Options.MaxRecursive : 1;
            var exeIcons = await IconHelper.GetExeIconsAsync(path, (uint)maxDepth);
            var entry = new FolderEntry(path, icon, exeIcons);
            SubFolders.Add(entry);
        }

        ApplyFilter();
    }

    #endregion
}