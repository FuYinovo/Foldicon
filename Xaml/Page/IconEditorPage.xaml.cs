using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
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
    /// 点击「启用筛选」时应用筛选
    /// </summary>
    private void EnableEnumFilterButton_OnClick(object sender, RoutedEventArgs e) => ApplyFilter();

    /// <summary>
    /// 「选择文件夹」按钮
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
            await RefreshSubfoldersAsync();
            IsSubFoldersLoaded = true;
        }

        btn.IsEnabled = true;
    }

    /// <summary>
    /// 「应用所有图标」按钮
    /// </summary>
    private void ApplyAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var entry in SubFolders) entry.Apply();
    }

    /// <summary>
    /// 点击 TypeFilterGroup 的 Item 时，
    /// 在 OneWay 基础上手动实现 TwoWay，
    /// 防止 <see cref="DependencyProperty.UnsetValue"/> 引发异常
    /// </summary>
    private void TypeFilterItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioMenuFlyoutItem item) return;
        if (System.Enum.TryParse<TypeFilterEnum>(item.Tag.ToString(), out var enumValue))
            FolderTypeFilter = enumValue;
    }

    /// <summary>
    /// 点击 StateFilterGroup 的 Item 时，
    /// 在 OneWay 基础上手动实现 TwoWay，
    /// 防止 <see cref="DependencyProperty.UnsetValue"/> 引发异常
    /// </summary>
    private void StateFilterItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioMenuFlyoutItem item) return;
        if (System.Enum.TryParse<StateFilterEnum>(item.Tag.ToString(), out var enumValue))
            FolderStateFilter = enumValue;
    }
}

public sealed partial class IconEditorPage // 普通方法
{
    /// <summary>
    /// 刷新子文件夹
    /// </summary>
    private async Task RefreshSubfoldersAsync()
    {
        if (!Directory.Exists(ParentFolder)) return;
        SubFolders.Clear();

        // 获取子文件夹图标
        var icons = await IconHelper.GetSubfolderIconsAsync(ParentFolder);

        // 创建 FolderEntry 实例
        foreach (var icon in icons)
        {
            try
            {
                // 获取可选 exe 程序图标
                var exeIcons = await IconHelper.GetExeIconsAsync(icon.FullPath);
                SubFolders.Add(new FolderEntry(icon.FullPath, icon.Icon, exeIcons));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }


        ApplyFilter();
    }

    /// <summary>
    /// 应用文件夹筛选
    /// </summary>
    private void ApplyFilter()
    {
        FilteredSubFolders.Clear();
        foreach (var folder in SubFolders)
        {
            // 名称筛选
            if (!folder.FolderName.Contains((string)FolderNameFilter)) continue;

            // 标签筛选
            if (IsEnumFilterEnabled)
            {
                // 类型筛选
                switch (FolderTypeFilter)
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
                switch (FolderStateFilter)
                {
                    case StateFilterEnum.All:
                        break;
                    case StateFilterEnum.Unmodified:
                        if (folder.IsSelectedIconChanged) continue;
                        break;
                    case StateFilterEnum.Unapplied:
                        if (!folder.IsSelectedIconChanged) continue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

    [ObservableProperty] private string _parentFolder = string.Empty;
    [ObservableProperty] private bool _isSubFoldersLoaded;
    [ObservableProperty] private bool _isEnumFilterEnabled = true;
    [ObservableProperty] private string _folderNameFilter = string.Empty;
    [ObservableProperty] private TypeFilterEnum _folderTypeFilter = TypeFilterEnum.All;
    [ObservableProperty] private StateFilterEnum _folderStateFilter = StateFilterEnum.All;
    public ObservableCollection<FolderEntry> FilteredSubFolders { get; } = []; // UI 显示
    private List<FolderEntry> SubFolders { get; } = []; // 数据源

    partial void OnFolderTypeFilterChanged(TypeFilterEnum value) => ApplyFilter();

    partial void OnFolderStateFilterChanged(StateFilterEnum value) => ApplyFilter();

    partial void OnFolderNameFilterChanged(string value) => ApplyFilter();
}