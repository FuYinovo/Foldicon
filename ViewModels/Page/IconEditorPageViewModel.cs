using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Contracts;
using Foldicon.Enums;
using Foldicon.Helpers;
using FolderEntry = Foldicon.Models.FolderEntry;

namespace Foldicon.ViewModels.Page;

public partial class IconEditorPageViewModel(IDialogService dialogService) : ObservableObject
{
    public readonly IDialogService DialogService = dialogService;

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

    // 与 XAML 中 SegmentedItem 的排列一致
    private static readonly TypeFilterEnum[] TypeFilterMapping =
        [TypeFilterEnum.All, TypeFilterEnum.System, TypeFilterEnum.Custom];

    private static readonly StatusFilterEnum[] StatusFilterMapping =
        [StatusFilterEnum.All, StatusFilterEnum.Unmodified, StatusFilterEnum.Unapplied];

    [ObservableProperty] public partial string NameFilter { get; set; } = string.Empty;

    [ObservableProperty] public partial TypeFilterEnum TypeFilter { get; set; } = TypeFilterEnum.All;

    [ObservableProperty] public partial StatusFilterEnum StatusFilter { get; set; } = StatusFilterEnum.All;

    [ObservableProperty] public partial int StatusFilterIndex { get; set; }

    [ObservableProperty] public partial int TypeFilterIndex { get; set; }

    partial void OnTypeFilterChanged(TypeFilterEnum value) => ApplyFilter();

    partial void OnStatusFilterChanged(StatusFilterEnum value) => ApplyFilter();

    partial void OnNameFilterChanged(string value) => ApplyFilter();

    partial void OnStatusFilterIndexChanged(int value)
    {
        if (value >= 0 && value < StatusFilterMapping.Length)
            StatusFilter = StatusFilterMapping[value];
    }

    partial void OnTypeFilterIndexChanged(int value)
    {
        if (value >= 0 && value < TypeFilterMapping.Length)
            TypeFilter = TypeFilterMapping[value];
    }

    #endregion

    #region Command

    /// <summary>
    ///     加载指定文件夹的子文件夹图标
    /// </summary>
    [RelayCommand]
    public async Task LoadFolderAsync()
    {
        var fullPath = await StoragePicker.PickFolder(DialogService.WindowId);
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
            if (!folder.FolderName.Contains(NameFilter)) continue;

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
        var subfolderIcons = await IconHelper.GetSubfolderIconsAsync(ParentFolder);

        // 并行创建 FolderEntry 实例
        var tasks = subfolderIcons.Select(async icon =>
        {
            try
            {
                var exeIcons = await IconHelper.GetExeIconsAsync(icon.FullPath);
                return await FolderEntry.CreateAsync(icon.FullPath, icon.Icon!, exeIcons); // try-catch 处理 null 导致的异常
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        });

        foreach (var entry in await Task.WhenAll(tasks))
            if (entry is not null)
                SubFolders.Add(entry);

        ApplyFilter();
    }

    #endregion
}