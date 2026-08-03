using System.Linq;
using Foldicon.Contracts;
using Foldicon.Models;
using Foldicon.ViewModels.UserControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Views.UserControl;

public sealed partial class FolderEntryControl
{
    public FolderEntryControl()
    {
        InitializeComponent();
        Loaded += PickIconFromGroup_Flyout_GenerateItems;
    }

    private static readonly DependencyProperty FolderEntryProperty = DependencyProperty.Register(
        nameof(FolderEntry),
        typeof(FolderEntry),
        typeof(FolderEntryControl),
        new PropertyMetadata(null)
    );

    public FolderEntry FolderEntry
    {
        get => (FolderEntry)GetValue(FolderEntryProperty);
        set
        {
            SetValue(FolderEntryProperty, value);
            ViewModel.FolderEntry = value;
        }
    }

    public readonly FolderEntryControlViewModel ViewModel =
        App.Services.GetRequiredService<FolderEntryControlViewModel>();

    /// <summary>
    ///     从图标组选择其他自定义图标
    /// </summary>
    private void PickOtherIconFromGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: IconGroup group }) return;
        ViewModel.PickIconFromGroupCommand.Execute(group);
    }

    /// <summary>
    ///     图标选择器在 OneWay 的基础上手动实现 TwoWay
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void IconSelector_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        var index = comboBox.SelectedIndex;
        if (index == -1) return; // 防止虚拟化回收时覆写 -1 污染数据
        ViewModel.FolderEntry?.SelectedIndex = index;
    }

    /// <summary>
    ///     加载「浏览→从图标库」的子菜单
    /// </summary>
    private void PickIconFromGroup_Flyout_GenerateItems(object sender, RoutedEventArgs e)
    {
        foreach (var group in App.Services.GetRequiredService<IIconGroupService>().Groups)
        {
            if (IsItemExists(group)) continue;
            var item = new MenuFlyoutItem
            {
                Text = group.Name,
                Tag = group
            };
            item.Click += PickOtherIconFromGroup_Click;
            PickIconFromGroupFlyout.Items.Add(item);
        }

        return;

        bool IsItemExists(IconGroup group)
        {
            return PickIconFromGroupFlyout.Items.Any(item => ReferenceEquals(group, item.Tag));
        }
    }
}