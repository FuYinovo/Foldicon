using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Controls;
using Foldicon.Class;
using Foldicon.Enum;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Xaml.Page;

[ObservableObject]
public sealed partial class IconEditorPage // 回调方法
{
    /// <summary>
    ///     「选择文件夹」按钮
    /// </summary>
    private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.IsEnabled = false;

        var fullPath = await StoragePicker.PickFolder(btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (fullPath is not null)
        {
            ParentFolder = fullPath;
            IsSubFoldersLoaded = false;
            IsSubFoldersLoading = true;
            await RefreshSubfoldersAsync();
            IsSubFoldersLoaded = true;
            IsSubFoldersLoading = false;
        }

        btn.IsEnabled = true;
    }

    /// <summary>
    ///     「应用所有图标」按钮
    /// </summary>
    private void ApplyAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var entry in SubFolders) entry.Apply();
    }
}

public sealed partial class IconEditorPage // 普通方法
{
    /// <summary>
    ///     刷新子文件夹
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
                return await FolderEntry.CreateAsync(icon.FullPath, icon.Icon, exeIcons);
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

    /// <summary>
    ///     应用文件夹筛选
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

            // 通过筛选
            FilteredSubFolders.Add(folder);
        }
    }
}

public sealed partial class IconEditorPage // 属性、属性 OnChanged 方法、构造方法
{
    public IconEditorPage()
    {
        InitializeComponent();
    }

    #region Filter

    [ObservableProperty] public partial string NameFilter { get; set; } = string.Empty;
    [ObservableProperty] public partial TypeFilterEnum TypeFilter { get; set; } = TypeFilterEnum.All;
    [ObservableProperty] public partial StatusFilterEnum StatusFilter { get; set; } = StatusFilterEnum.All;
    [ObservableProperty] public partial int StatusFilterIndex { get; set; } = 0; // Index 更新触发 Enum 更新
    [ObservableProperty] public partial int TypeFilterIndex { get; set; } = 0;
    partial void OnTypeFilterChanged(TypeFilterEnum value) => ApplyFilter();
    partial void OnStatusFilterChanged(StatusFilterEnum value) => ApplyFilter();
    partial void OnNameFilterChanged(string value) => ApplyFilter();

    partial void OnStatusFilterIndexChanged(int value)
    {
        try
        {
            var item = (SegmentedItem)StatusFilterSegmented.Items[value];
            StatusFilter = (StatusFilterEnum)item.Tag;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    partial void OnTypeFilterIndexChanged(int value)
    {
        try
        {
            var item = (SegmentedItem)TypeFilterSegmented.Items[value];
            TypeFilter = (TypeFilterEnum)item.Tag;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    #endregion

    [ObservableProperty] public partial string ParentFolder { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSubFoldersLoaded { get; set; } = false;
    [ObservableProperty] public partial bool IsSubFoldersLoading { get; set; } = false;
    public ObservableCollection<FolderEntry> FilteredSubFolders { get; } = []; // UI 显示
    private List<FolderEntry> SubFolders { get; } = []; // 数据源
}