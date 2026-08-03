using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Contracts;
using Foldicon.Helpers;
using Foldicon.Views.Dialog;
using Foldicon.Views.Page;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.ViewModels.Page;

public partial class IconGroupPageViewModel(
    IIconGroupService iconGroupService,
    IDialogService dialogService,
    INavigationService navigationService) : ObservableObject
{
    public readonly IIconGroupService IconGroupService = iconGroupService;
    public readonly IDialogService DialogService = dialogService;
    public readonly INavigationService NavigationService = navigationService;

    /// <summary>
    ///     移除图标组
    /// </summary>
    [RelayCommand]
    public async Task RemoveGroupAsync(IconGroup group)
    {
        // 二次确认
        var result = await DialogService.ShowMessageAsync(
            $"确定删除\"{group.Name}\"吗？",
            $"导入的{group.Icons.Count}个图标将无法从回收站恢复",
            true);
        if (result == ContentDialogResult.None) return;

        // 延迟到下一个UI帧移除（让ContextFlyout先关闭），防止 E_FAIL (0x80004005) 崩溃
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => IconGroupService.Remove(group));
    }

    /// <summary>
    ///     编辑图标组信息
    /// </summary>
    [RelayCommand]
    public async Task EditGroupInfoAsync(IconGroup group)
    {
        var content = IconGroupInfoDialog.Create_EditDialog(group);
        var result = await DialogService.ShowDialogAsync(title: "编辑图标组", content: content);
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await DialogService.ShowMessageAsync("编辑失败", "名称不能为空");
            return;
        }

        // 编辑操作
        IconGroupService.Edit(group, content.GroupName, content.Description, content.Logo);
    }

    /// <summary>
    ///     导入图标
    /// </summary>
    [RelayCommand]
    public async Task ImportIconAsync(IconGroup group)
    {
        // 选取图标
        var icons = await StoragePicker.PickFiles(Services.IconGroupService.IconExtensions,
            DialogService.WindowId);
        if (icons.Length == 0) return;

        // 导入图标
        foreach (var icon in icons) group.Add(icon);
    }

    /// <summary>
    ///     导入图标组
    /// </summary>
    [RelayCommand]
    public async Task ImportGroupAsync()
    {
        // 选取文件夹
        var folder = await StoragePicker.PickFolder(DialogService.WindowId);
        if (folder is null) return; // 取消操作

        // 筛选有效图标
        var files = Directory.GetFiles(folder);
        List<string> icons =
        [
            .. files.Where(file => Services.IconGroupService.IconExtensions.Contains(Path.GetExtension(file)))
        ]; // 检查拓展名

        // 设置图标组信息
        BitmapIcon? defaultIcon = icons.Count > 0 // 选取第一个图标作为默认 Logo
            ? new BitmapIcon { FullPath = icons.First(), Icon = new BitmapImage(new Uri(icons.First())) }
            : null;
        var content = IconGroupInfoDialog.Create_CreateDialog(Path.GetFileName(folder), null, defaultIcon);
        var result = await DialogService.ShowDialogAsync(title: "创建图标组", content: content);
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await DialogService.ShowMessageAsync("导入失败", "名称不能为空");
            return;
        }

        // 创建操作
        IconGroupService.Add(content.GroupName, content.Description, content.Logo, icons);
    }

    /// <summary>
    ///    创建图标组
    /// </summary>
    [RelayCommand]
    public async Task CreateGroupAsync()
    {
        var content = IconGroupInfoDialog.Create_CreateDialog();
        var result = await DialogService.ShowDialogAsync(title: "创建图标组", content: content);
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await DialogService.ShowMessageAsync("创建失败", "名称不能为空");
            return;
        }

        // 创建操作
        IconGroupService.Add(content.GroupName, content.Description, content.Logo);
    }

    /// <summary>
    ///     跳转图标组到详情页
    /// </summary>
    [RelayCommand]
    private void NavigateGroupPage(IconGroup group)
    {
        NavigationService.Navigate(typeof(IconGroupsDetailPage), group);
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