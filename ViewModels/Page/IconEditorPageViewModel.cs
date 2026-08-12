using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Contracts;
using Foldicon.Enums;
using Foldicon.Helpers;
using Foldicon.Struct;
using FolderEntry = Foldicon.Models.FolderEntry;

namespace Foldicon.ViewModels.Page;

public partial class IconEditorPageViewModel(IDialogService dialogService, IOptionService optionService)
    : ObservableObject
{
    #region Status Properties

    [ObservableProperty] public partial string ParentFolder { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsFoldersLoaded { get; set; }
    [ObservableProperty] public partial bool IsFoldersLoading { get; set; }
    [ObservableProperty] public partial double FoldersLoadingPercentage { get; set; } = 0; // 范围:[0,100]
    private List<FolderEntry> SubFolders { get; } = [];

    #endregion

    #region Filter Propertries

    public ObservableCollection<FolderEntry> FilteredFolders { get; } = [];
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
    private async Task LoadFolderAsync()
    {
        var fullPath = await StoragePicker.PickFolder(dialogService.WindowId);
        if (fullPath is not null)
        {
            ParentFolder = fullPath;
            IsFoldersLoaded = false;
            IsFoldersLoading = true;
            await RefreshSubfoldersAsync(new Progress<Percentage>(p => FoldersLoadingPercentage = p.Value * 100));
            IsFoldersLoaded = true;
            IsFoldersLoading = false;
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
    ///     根据当前筛选条件刷新 <see cref="FilteredFolders"/>
    /// </summary>
    private void ApplyFilter()
    {
        FilteredFolders.Clear();
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

            FilteredFolders.Add(folder);
        }
    }

    /// <summary>
    ///     刷新子文件夹列表
    /// </summary>
    private async Task RefreshSubfoldersAsync(IProgress<Percentage>? progress)
    {
        if (!Directory.Exists(ParentFolder)) return;
        SubFolders.Clear();

        // 并行获取子文件夹图标
        var loadedCount = 0; // Progress
        var directories = Directory.GetDirectories(ParentFolder);
        List<Task<FolderEntry?>> tasks =
        [
            .. directories.Select(path => Task.Run(() =>
            {
                if (!IconHelper.TryGetFolderIcon(path, Const.FolderIconSize, out var icon))
                {
                    ReportProgress();
                    return null;
                }

                var maxRecursive =
                    (uint)(optionService.Options.IsRecursive ? optionService.Options.MaxRecursive : 1);
                var exeIcons = IconHelper.GetFilesIcon(path, "*.exe", maxRecursive, Const.FolderIconSize);
                var entry = new FolderEntry(path, icon, exeIcons, optionService.Options.IsAutoSelectIcon);

                ReportProgress();
                return entry;
            }))
        ];

        await Task.WhenAll(tasks);
        SubFolders
            .AddRange(tasks
                .Where(task => task.Result != null)
                .Select(task => task.Result!));
        ApplyFilter();

        return;

        void ReportProgress()
        {
            loadedCount += 1;
            progress?.Report(new Percentage { Now = loadedCount, Max = directories.Length });
        }
    }

    #endregion
}