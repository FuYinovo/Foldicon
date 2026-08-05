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

    ///<summary>手动实现 TwoWay</summary>
    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox box) return;
        // 防止 TwoWay 在虚拟化控件回收时覆写 null 污染数据 -->
        if (box.SelectedItem is IFolderIcon item) ViewModel.FolderEntry?.SelectedIcon = item;
    }
}