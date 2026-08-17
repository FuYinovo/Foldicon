using System;
using Foldicon.Helpers;
using Foldicon.Models;
using Foldicon.Record;
using Foldicon.ViewModels.Dialog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Views.Dialog;

public sealed partial class IconGroupInfoDialog
{
    private IconGroupInfoDialogViewModel ViewModel { get; } =
        App.Services.GetRequiredService<IconGroupInfoDialogViewModel>();

    public static IconGroupInfoDialog Create_EditDialog(IconGroup group)
    {
        return new IconGroupInfoDialog
        {
            ViewModel =
            {
                GroupName = group.Name,
                Description = group.Description,
                Logo = group.Logo
            }
        };
    }

    public static IconGroupInfoDialog Create_CreateDialog(
        string? name = null,
        string? description = null,
        IconGroupLogo? icon = null
    )
    {
        var fallback = UriHelper.GetFilePathFromAssets("LogoFallback.png");
        return new IconGroupInfoDialog
        {
            ViewModel =
            {
                GroupName = name ?? string.Empty,
                Description = description ?? string.Empty,
                Logo = icon ?? new IconGroupLogo
                {
                    Path = fallback,
                    Bitmap = new BitmapImage(new Uri(fallback))
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

    public IconGroupLogo Logo => ViewModel.Logo;
}