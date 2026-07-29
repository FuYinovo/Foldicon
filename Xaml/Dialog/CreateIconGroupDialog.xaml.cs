using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Service;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.Xaml.Dialog;

[ObservableObject]
public sealed partial class CreateIconGroupDialog
{
    public CreateIconGroupDialog()
    {
        InitializeComponent();
    }

    [ObservableProperty] public partial string GroupName { get; set; } = string.Empty;

    [ObservableProperty] public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial BitmapIcon Logo { get; set; } = new()
        { Icon = new BitmapImage(new Uri(UriHelper.GetFilePathFromAssets("LogoFallback.png"))) };

    /// <summary>
    ///     选取一个图片作为图标组 Logo
    /// </summary>
    private async void ChooseLogo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var path = await StoragePicker.PickFile(IconGroupService.LogoExtensions,
            btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (path is null) return; // 取消操作
        Logo = new BitmapIcon { FullPath = path, Icon = new BitmapImage(new Uri(path)) };
    }
}