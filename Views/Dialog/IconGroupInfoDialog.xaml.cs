using System;
using Foldicon.Helpers;
using Foldicon.Models;
using Foldicon.ViewModels.Dialog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.Views.Dialog;

public sealed partial class IconGroupInfoDialog
{
    private IconGroupInfoDialogViewModel ViewModel { get; } =
        App.Services.GetRequiredService<IconGroupInfoDialogViewModel>();

    public static IconGroupInfoDialog Create_EditDialog(IconGroup group)
    {
        var logo = (BitmapImage)group.Logo;
        return new IconGroupInfoDialog
        {
            ViewModel =
            {
                GroupName = group.Name,
                Description = group.Description,
                Logo = new BitmapIcon
                {
                    FullPath = logo.UriSource.AbsolutePath,
                    Icon = logo
                }
            }
        };
    }

    public static IconGroupInfoDialog Create_CreateDialog(
        string? name = null,
        string? description = null,
        BitmapIcon? icon = null
    )
    {
        var fallback = UriHelper.GetFilePathFromAssets("LogoFallback.png");
        return new IconGroupInfoDialog
        {
            ViewModel =
            {
                GroupName = name ?? string.Empty,
                Description = description ?? string.Empty,
                Logo = icon ?? new BitmapIcon
                {
                    FullPath = fallback,
                    Icon = new BitmapImage(new Uri(fallback))
                }
            }
        };
    }

    private IconGroupInfoDialog()
    {
        InitializeComponent();
    }

    public string GroupName => ViewModel.GroupName;

    public string Description => ViewModel.Description;

    public BitmapIcon Logo => ViewModel.Logo;
}