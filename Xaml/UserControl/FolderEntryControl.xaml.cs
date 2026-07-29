using System;
using Foldicon.Class;
using Foldicon.Service;
using Foldicon.Tool;
using Foldicon.Xaml.Dialog;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Xaml.UserControl;

public sealed partial class FolderEntryControl
{
    public FolderEntry FolderEntry
    {
        get => (FolderEntry)GetValue(FolderEntryProperty);
        set => SetValue(FolderEntryProperty, value);
    }


    private static readonly DependencyProperty FolderEntryProperty = DependencyProperty.Register(
        nameof(FolderEntry),
        typeof(FolderEntry),
        typeof(FolderEntryControl),
        new PropertyMetadata(null)
    );

    public FolderEntryControl()
    {
        InitializeComponent();
        Loaded += PickIconFromGroup_Flyout_ItemsGenerate;
    }

    /// <summary>
    /// 应用选中的自定义图标
    /// </summary>
    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        FolderEntry.Apply();
    }

    /// <summary>
    /// 使用 FilePicker 选择其他自定义图标
    /// </summary>
    private async void PickIconFromFile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;
        var fullPath =
            await StoragePicker.PickFile([".exe", ".ico"], item.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (fullPath is null) return;
        FolderEntry.AddCustomIcon(fullPath);
    }

    /// <summary>
    /// 从图标组选择其他自定义图标
    /// </summary>
    private async void PickOtherIconFromGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: IconGroup group }) return;
        // 弹出选择图标对话框
        var content = new PickOtherIconFromGroupDialog(group);
        var dialog = new ContentDialog
        {
            RequestedTheme = App.MainWindow.GetRequestedTheme(),
            Title = $"从\"{group.Name}\" 选择一个图标",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            Content = content
        };
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.None) return; // 取消操作

        var selectedPath = content.GetSelectedIconPath();
        if (selectedPath is null)
        {
            // 未选择提示
            _ = await new ContentDialog
            {
                RequestedTheme = App.MainWindow.GetRequestedTheme(),
                Title = "添加失败",
                CloseButtonText = "确定",
                XamlRoot = XamlRoot,
                Content = new DescriptionDialog { Description = "未选中任何图标" }
            }.ShowAsync();
            return;
        }

        // 正常选择 -> 添加图标
        FolderEntry.AddCustomIcon(selectedPath);
    }

    /// <summary>
    /// 图标选择器在 OneWay 的基础上手动实现 TwoWay
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void IconSelector_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        var index = comboBox.SelectedIndex;
        if (index == -1) return; // 防止虚拟化回收时覆写 -1 污染数据
        FolderEntry.SelectedIndex = index;
    }

    /// <summary>
    /// 加载「浏览→从图标库」的子菜单
    /// </summary>
    private void PickIconFromGroup_Flyout_ItemsGenerate(object sender, RoutedEventArgs e)
    {
        foreach (var group in IconGroupService.Instance.Groups)
        {
            var item = new MenuFlyoutItem
            {
                Text = group.Name,
                Tag = group,
            };
            item.Click += PickOtherIconFromGroup_Click;
            PickIconFromGroupFlyout.Items.Add(item);
        }
    }
}