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
using Foldicon.Models.Icon;
using Foldicon.Struct;
using Foldicon.Views.Dialog;
using Foldicon.Views.Page;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.ViewModels.Page;

public partial class IconGroupPageViewModel(
    IIconGroupService iconGroupService,
    IDialogService dialogService,
    INavigationService navigationService) : ObservableObject
{
    public readonly IIconGroupService IconGroupService = iconGroupService;

    /// <summary>
    ///     移除图标组
    /// </summary>
    [RelayCommand]
    private async Task RemoveGroupAsync(IconGroup group)
    {
        // 二次确认
        var result = await dialogService.ShowMessageAsync(
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
    private async Task EditGroupInfoAsync(IconGroup group)
    {
        var content = IconGroupInfoDialog.Create_EditDialog(group);
        var result = await dialogService.ShowDialogAsync(title: "编辑图标组", content: content);
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await dialogService.ShowMessageAsync("编辑失败", "名称不能为空");
            return;
        }

        // 编辑操作
        IconGroupService.Edit(group, content.GroupName, content.Description, content.Logo);
    }

    /// <summary>
    ///     导入图标
    /// </summary>
    [RelayCommand]
    private async Task ImportIconAsync(IconGroup group)
    {
        // 选取图标
        var iconPaths = await StoragePicker.PickFiles(Services.IconGroupService.IconExtensions,
            dialogService.WindowId);

        // 导入图标
        foreach (var path in iconPaths)
        {
            var icon = new FolderFileIcon(new BitmapImage(new Uri(path)), path);
            // 检查是否已导入重复文件名的图标
            var existedIcon = group.Icons.FirstOrDefault(x =>
                x is FolderFileIcon file && Path.GetFileName(file.FilePath) == Path.GetFileName(icon.FilePath));
            if (existedIcon is not null)
            {
                var result = await dialogService.ShowMessageAsync(
                    "是否覆盖重复图标？",
                    $"{group.Name}图标组中已经存在{icon.ShortDisplayName}图标，是否将其覆盖？",
                    true
                );
                if (result == ContentDialogResult.Primary) group.Remove(existedIcon);
                else return;
            }

            group.Add(icon);
        }
    }

    /// <summary>
    ///     导入图标组
    /// </summary>
    [RelayCommand]
    private async Task ImportGroupAsync()
    {
        // 选取文件夹
        var folder = await StoragePicker.PickFolder(dialogService.WindowId);
        if (folder is null) return; // 取消操作

        // 筛选有效图标
        var files = Directory.GetFiles(folder);
        List<IFolderIcon> icons =
        [
            .. files
                .Where(file => Services.IconGroupService.IconExtensions.Contains(Path.GetExtension(file))) // 检查拓展名
                .Select(file => new FolderFileIcon(new BitmapImage(new Uri(file)), file)) // 创建 FolderFileIcon 实例
        ];

        // 设置图标组信息
        var defaultIcon = icons.Count > 0 // 选取第一个图标作为默认 Logo
            ? new IconGroupLogo { Path = ((FolderFileIcon)icons.First()).FilePath, Bitmap = icons.First().Bitmap }
            : null;
        var content = IconGroupInfoDialog.Create_CreateDialog(Path.GetFileName(folder), null, defaultIcon);
        var result = await dialogService.ShowDialogAsync(title: "创建图标组", content: content);
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await dialogService.ShowMessageAsync("导入失败", "名称不能为空");
            return;
        }

        // 创建操作
        IconGroupService.Add(content.GroupName, content.Description, content.Logo, icons);
    }

    /// <summary>
    ///    创建图标组
    /// </summary>
    [RelayCommand]
    private async Task CreateGroupAsync()
    {
        var content = IconGroupInfoDialog.Create_CreateDialog();
        var result = await dialogService.ShowDialogAsync(title: "创建图标组", content: content);
        if (result == ContentDialogResult.None) return; // 取消操作

        // 名称不可为空
        if (string.IsNullOrWhiteSpace(content.GroupName))
        {
            await dialogService.ShowMessageAsync("创建失败", "名称不能为空");
            return;
        }

        // 创建操作
        IconGroupService.Add(content.GroupName, content.Description, content.Logo, []);
    }

    /// <summary>
    ///     跳转图标组到详情页
    /// </summary>
    [RelayCommand]
    private void NavigateGroupPage(IconGroup group)
    {
        navigationService.Navigate(typeof(IconGroupsDetailPage), group);
    }

    /// <summary>
    ///     打开图标组所在文件夹
    /// </summary>
    [RelayCommand]
    private static void OpenGroupFolder(IconGroup group)
    {
        Process.Start("explorer.exe", group.RootPath);
    }
}