using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Class;
using Foldicon.Service;
using Foldicon.Tool;
using Foldicon.Xaml.Dialog;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.Xaml.Page;

[ObservableObject]
public sealed partial class IconGroupsDetailPage
{
    [ObservableProperty] private IconGroup _group = new();

    public IconGroupsDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // 接收新的 IconGroup 作为 ViewModel
        if (e.Parameter is IconGroup group) Group = group;
    }

    /// <summary>
    /// 点击「导入图标」按键
    /// </summary>
    private async void ImportIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        // 选取图标
        var icons = await StoragePicker.PickFiles(IconGroupService.IconExtensions,
            btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (icons.Length == 0) return;

        // 导入图标
        foreach (var icon in icons) Group.Add(icon);
    }

    /// <summary>
    /// 响应 TokenView 按所选的「筛选类别」的改动
    /// </summary>
    private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        // TODO))
    }

    /// <summary>
    /// 点击右键菜单「删除图标」按钮
    /// </summary>
    private async void DeleteIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: BitmapIcon icon }) return;

        // 二次确认
        var content = new Grid();

        var dialog = new ContentDialog
        {
            Title = $"确定删除\"{icon.FileName}\"吗？",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = new DescriptionDialog {Description = "此操作将无法从回收站恢复"},
            XamlRoot = XamlRoot,
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None) return;

        // 延迟到下一个UI帧移除（让ContextFlyout先关闭），防止 E_FAIL (0x80004005) 崩溃
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Group.Remove(icon));
    }

    /// <summary>
    /// 点击图标后打开
    /// </summary>
    private void Icon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: BitmapIcon icon }) return;
        if (icon.FullPath is not null) Process.Start("explorer.exe", icon.FullPath);
    }
}