using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Helpers;
using Foldicon.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.ViewModels.Dialog;

public partial class IconGroupInfoDialogViewModel : ObservableObject
{
    public XamlRoot? XamlRoot { get; set; }
    [ObservableProperty] public partial string GroupName { get; set; } = string.Empty;

    [ObservableProperty] public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial BitmapIcon Logo { get; set; } = new()
        { Icon = new BitmapImage(new Uri(UriHelper.GetFilePathFromAssets("LogoFallback.png"))) };

    /// <summary>
    ///     选取一个图片作为图标组 Logo
    /// </summary>
    [RelayCommand]
    private async void PickLogoAsync()
    {
        var path = await StoragePicker.PickFile(IconGroupService.LogoExtensions,
            XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (path is null) return; // 取消操作
        Logo = new BitmapIcon { FullPath = path, Icon = new BitmapImage(new Uri(path)) };
    }
}