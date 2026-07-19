using Foldicon.Class;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.UserControl;

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
    private async void PickOtherIconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.IsEnabled = false;
        var fullPath =
            await StoragePicker.PickFile([".exe", ".ico"], btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        btn.IsEnabled = true;

        if (fullPath is null) return;
        FolderEntry.AddCustomIcon(fullPath);
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
}