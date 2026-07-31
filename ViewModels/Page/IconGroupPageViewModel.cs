using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Helpers;
using Foldicon.Services;
using Foldicon.Views.Dialog;
using Foldicon.Views.Page;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.ViewModels.Page;

public partial class IconGroupPageViewModel : ObservableObject
{
    public IconGroupService GroupService => IconGroupService.Instance;
    public XamlRoot? XamlRoot { get; set; }

    /// <summary>
    ///     移除图标组
    /// </summary>
    [RelayCommand]
    public async void RemoveGroupAsync(IconGroup group)
    {
        // 二次确认
        var dialog = new ContentDialog
        {
            RequestedTheme = App.MainWindow.GetRequestedTheme(),
            Title = $"确定删除\"{group.Name}\"吗？",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            Content = new DescriptionDialog($"导入的{group.Icons.Count}个图标将无法从回收站恢复")
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return;

        // 延迟到下一个UI帧移除（让ContextFlyout先关闭），防止 E_FAIL (0x80004005) 崩溃
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => GroupService.Remove(group));
    }

    /// <summary>
    ///     编辑图标组信息
    /// </summary>
    [RelayCommand]
    public async void EditGroupInfoAsync(IconGroup group)
    {
        var content = CreateIconGroupDialog.GetEditDialog(group);
        var dialog = new ContentDialog
        {
            RequestedTheme = App.MainWindow.GetRequestedTheme(),
            Title = "编辑图标组",
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            Content = content
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await new ContentDialog
            {
                RequestedTheme = App.MainWindow.GetRequestedTheme(),
                Title = "编辑失败",
                CloseButtonText = "确定",
                XamlRoot = XamlRoot,
                Content = new DescriptionDialog("名称不能为空")
            }.ShowAsync();
            return;
        }

        // 编辑操作
        IconGroupService.Instance.Edit(group, content.GroupName, content.Description, content.Logo);
    }

    /// <summary>
    ///     导入图标
    /// </summary>
    [RelayCommand]
    public async void ImportIconAsync(IconGroup group)
    {
        // 选取图标
        var icons = await StoragePicker.PickFiles(IconGroupService.IconExtensions,
            XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (icons.Length == 0) return;

        // 导入图标
        foreach (var icon in icons) group.Add(icon);
    }

    /// <summary>
    ///     导入图标组
    /// </summary>
    [RelayCommand]
    public async void ImportGroupAsync()
    {
        // 选取文件夹
        var folder = await StoragePicker.PickFolder(XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (folder is null) return; // 取消操作

        // 筛选有效图标
        var files = Directory.GetFiles(folder);
        List<string> icons =
            [.. files.Where(file => IconGroupService.IconExtensions.Contains(Path.GetExtension(file)))]; // 检查拓展名

        // 设置图标组信息
        var content = CreateIconGroupDialog.GetCreateDialog(Path.GetFileName(folder));
        if (icons.Count > 0) // 选取第一个图标作为默认 Logo
            content.Logo = new BitmapIcon
            {
                FullPath = icons.First(),
                Icon = new BitmapImage(new Uri(icons.First()))
            };

        var dialog = new ContentDialog
        {
            RequestedTheme = App.MainWindow.GetRequestedTheme(),
            Title = "创建图标组",
            PrimaryButtonText = "创建",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            Content = content
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await new ContentDialog
            {
                RequestedTheme = App.MainWindow.GetRequestedTheme(),
                Title = "导入失败",
                CloseButtonText = "确定",
                XamlRoot = XamlRoot,
                Content = new DescriptionDialog("名称不能为空")
            }.ShowAsync();
            return;
        }

        // 创建操作
        IconGroupService.Instance.Add(content.GroupName, content.Description, content.Logo, icons);
    }

    /// <summary>
    ///    创建图标组
    /// </summary>
    [RelayCommand]
    public async void CreateGroupAsync()
    {
        var content = CreateIconGroupDialog.GetCreateDialog();
        var dialog = new ContentDialog
        {
            RequestedTheme = App.MainWindow.GetRequestedTheme(),
            Title = "创建图标组",
            PrimaryButtonText = "创建",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            Content = content
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await new ContentDialog
            {
                RequestedTheme = App.MainWindow.GetRequestedTheme(),
                Title = "创建失败",
                CloseButtonText = "确定",
                XamlRoot = XamlRoot,
                Content = new DescriptionDialog("名称不能为空")
            }.ShowAsync();
            return;
        }

        // 创建操作
        IconGroupService.Instance.Add(content.GroupName, content.Description, content.Logo);
    }

    /// <summary>
    ///     跳转图标组到详情页
    /// </summary>
    [RelayCommand]
    public static void NavigateGroupPage(IconGroup group)
    {
        App.MainWindow.NavigateTo(typeof(IconGroupsDetailPage), group);
    }

    /// <summary>
    ///     打开图标组所在文件夹
    /// </summary>
    [RelayCommand]
    public static void OpenGroupFolder(IconGroup group)
    {
        Process.Start("explorer.exe", group.RootPath);
    }
}