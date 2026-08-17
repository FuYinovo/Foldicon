using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Contracts;
using Foldicon.Helpers;
using Foldicon.Record;
using Foldicon.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.ViewModels.Dialog;

public partial class IconGroupInfoDialogViewModel(IDialogService dialogService) : ObservableObject
{
    [ObservableProperty] public partial string GroupName { get; set; } = string.Empty;
    [ObservableProperty] public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconGroupLogo Logo { get; set; }

    /// <summary>
    ///     选取一个图片作为图标组 Logo
    /// </summary>
    [RelayCommand]
    private async Task PickLogoAsync()
    {
        var path = await StoragePicker.PickFile(IconGroupService.LogoExtensions, dialogService.WindowId);
        if (path is null) return; // 取消操作
        Logo = new IconGroupLogo { Path = path, Bitmap = new BitmapImage(new Uri(path)) };
    }
}