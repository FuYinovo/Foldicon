using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Service;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.Xaml.Dialog;

[ObservableObject]
public sealed partial class CreateIconGroupDialog
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private BitmapIcon _logo = new() { Icon = new BitmapImage() };

    public CreateIconGroupDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 选取一个图片作为图标组 Logo
    /// </summary>
    private async void ChooseLogo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var path = await StoragePicker.PickFile(IconGroupService.SupportedLogoExtensions,
            btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (path is null) return; // 取消操作
        Logo = new BitmapIcon { FullPath = path, Icon = new BitmapImage(new Uri(path)) };
    }
}