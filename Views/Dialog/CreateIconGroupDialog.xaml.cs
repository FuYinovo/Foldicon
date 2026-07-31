using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Helpers;
using Foldicon.Models;
using Foldicon.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.Views.Dialog;

[ObservableObject]
public sealed partial class CreateIconGroupDialog
{
    public static CreateIconGroupDialog GetEditDialog(IconGroup group)
    {
        var logo = (BitmapImage)group.Logo;
        return new CreateIconGroupDialog
        {
            GroupName = group.Name,
            Description = group.Description,
            Logo = new BitmapIcon
            {
                FullPath = logo.UriSource.AbsolutePath,
                Icon = logo
            }
        };
    }

    public static CreateIconGroupDialog GetCreateDialog(string? name = null)
    {
        return new CreateIconGroupDialog { Name = name ?? string.Empty };
    }

    private CreateIconGroupDialog()
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