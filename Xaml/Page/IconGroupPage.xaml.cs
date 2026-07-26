using System;
using System.Diagnostics;
using Foldicon.Class;
using Foldicon.Service;
using Foldicon.Tool;
using Foldicon.Xaml.Dialog;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.Xaml.Page;

public sealed partial class IconGroupPage
{
    private readonly IconGroupService _viewModel = IconGroupService.Instance;

    public IconGroupPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击「移除图标组」按键
    /// </summary>
    private void RemoveGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;
        if (item.DataContext is not IconGroup group) return;

        // 延迟到下一个UI帧移除（让ContextFlyout先关闭），防止 E_FAIL (0x80004005) 崩溃
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => _viewModel.Remove(group));
    }

    /// <summary>
    /// 点击「编辑图标组信息」按键
    /// </summary>
    private async void EditGroupInfo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: IconGroup group }) return;
        var content = new CreateIconGroupDialog
        {
            Logo = new BitmapIcon { Icon = (BitmapImage)group.Logo },
            Name = group.Name,
            Description = group.Description
        };
        var dialog = new ContentDialog
        {
            Title = "编辑图标组",
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            Content = content
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return; // 取消操作

        // 编辑操作
        IconGroupService.Instance.Edit(group, content.Name, content.Description, content.Logo);
    }

    /// <summary>
    /// 点击「导入图标」按键
    /// </summary>
    private async void ImportIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: IconGroup group } item) return;

        // 选取图标
        var icons = await StoragePicker.PickFiles(IconGroupService.IconExtensions,
            item.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (icons.Length == 0) return;

        // 导入图标
        foreach (var icon in icons) group.Add(icon);
    }

    /// <summary>
    /// 点击「导入图标组」按钮
    /// </summary>
    private void ImportGroup_Click(object sender, RoutedEventArgs e)
    {
        // TODO))
    }

    /// <summary>
    /// 点击「创建图标组」按钮
    /// </summary>
    private async void CreateGroup_Click(object sender, RoutedEventArgs e)
    {
        var content = new CreateIconGroupDialog();
        var dialog = new ContentDialog
        {
            Title = "创建图标组",
            PrimaryButtonText = "创建",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            Content = content
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return; // 取消操作

        // 创建操作
        IconGroupService.Instance.Add(content.Name, content.Description, content.Logo);
    }

    /// <summary>
    /// 点击图标组后跳转到详情页
    /// </summary>
    private void IconGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: IconGroup group }) return;
        App.MainWindow.NavigateTo(typeof(IconGroupsDetailPage), group);
    }
}