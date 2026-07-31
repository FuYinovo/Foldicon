using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Helpers;
using Foldicon.Models;
using Foldicon.Services;
using Foldicon.ViewModels.Dialog;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using BitmapIcon = Foldicon.Struct.BitmapIcon;

namespace Foldicon.Views.Dialog;

public sealed partial class IconGroupInfoDialog
{
    private IconGroupInfoDialogViewModel ViewModel { get; } = new();

    public static IconGroupInfoDialog GetEditDialog(IconGroup group)
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
            },
        };
    }

    public static IconGroupInfoDialog GetCreateDialog(
        string? name = null,
        string? description = null,
        BitmapIcon? icon = null
    )
    {
        return new IconGroupInfoDialog
        {
            ViewModel =
            {
                GroupName = name ?? string.Empty,
                Description = description ?? string.Empty,
                Logo = icon ?? new BitmapIcon()
            }
        };
    }

    private IconGroupInfoDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => ViewModel.XamlRoot = XamlRoot;
    }

    public string GroupName => ViewModel.GroupName;

    public string Description => ViewModel.Description;

    public BitmapIcon Logo => ViewModel.Logo;
}