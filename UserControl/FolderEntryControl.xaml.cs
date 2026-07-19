using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Foldicon.Class;
using Foldicon.Struct;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using FileIcon = Foldicon.Struct.FileIcon;

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
        var icon = IconHelper.GetIcon(fullPath, false);
        if (icon is null) return;
        FolderEntry.OptionalIcons.Add(new FileIcon(fullPath, icon));
    }
}