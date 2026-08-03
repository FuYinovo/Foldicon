using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Contracts;
using Foldicon.Enums;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace Foldicon.ViewModels.Page;

public partial class SettingsPageViewModel(IOptionService optionService, MainWindow window) : ObservableObject
{
    #region ItemsSource

    public List<AppThemeEnum> AppThemes { get; } = GetEnumValues<AppThemeEnum>();
    public List<AppBackdropEnum> AppBackdrops { get; } = GetEnumValues<AppBackdropEnum>();

    #endregion

    #region Options

    [ObservableProperty] public partial AppThemeEnum SelectedAppTheme { get; set; } = optionService.Options.AppTheme;

    [ObservableProperty]
    public partial AppBackdropEnum SelectedAppBackdrop { get; set; } = optionService.Options.AppBackdrop;

    [ObservableProperty] public partial bool IsCaseSensitive { get; set; } = optionService.Options.IsCaseSensitive;
    [ObservableProperty] public partial bool IsRecursive { get; set; } = optionService.Options.IsRecursive;
    [ObservableProperty] public partial bool IsAutoSelectIcon { get; set; } = optionService.Options.IsAutoSelectIcon;
    [ObservableProperty] public partial int MaxRecursive { get; set; } = optionService.Options.MaxRecursive;

    #endregion

    #region OnChanged

    partial void OnSelectedAppThemeChanged(AppThemeEnum value)
    {
        optionService.Options.AppTheme = value;
        window.TrySetTheme(ConvertTheme(value));
        optionService.SaveAll();
    }

    partial void OnSelectedAppBackdropChanged(AppBackdropEnum value)
    {
        optionService.Options.AppBackdrop = value;
        switch (value)
        {
            case AppBackdropEnum.Mica:
                window.TrySetMicaBackdrop(MicaKind.Base);
                break;
            case AppBackdropEnum.MicaAlt:
                window.TrySetMicaBackdrop(MicaKind.BaseAlt);
                break;
            case AppBackdropEnum.Acrylic:
                window.TrySetAcrylicBackdrop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }

        optionService.SaveAll();
    }

    partial void OnIsCaseSensitiveChanged(bool value)
    {
        optionService.Options.IsCaseSensitive = value;
        optionService.SaveAll();
    }

    partial void OnIsRecursiveChanged(bool value)
    {
        optionService.Options.IsRecursive = value;
        optionService.SaveAll();
    }

    partial void OnIsAutoSelectIconChanged(bool value)
    {
        optionService.Options.IsAutoSelectIcon = value;
        optionService.SaveAll();
    }

    partial void OnMaxRecursiveChanged(int value)
    {
        optionService.Options.MaxRecursive = value;
        optionService.SaveAll();
    }
    #endregion

    private static List<TEnum> GetEnumValues<TEnum>() where TEnum : struct, Enum
    {
        return [.. Enum.GetValues<TEnum>()];
    }

    private static ElementTheme ConvertTheme(AppThemeEnum theme)
    {
        return theme switch
        {
            AppThemeEnum.System => ElementTheme.Default,
            AppThemeEnum.Dark => ElementTheme.Dark,
            AppThemeEnum.Light => ElementTheme.Light,
            _ => ElementTheme.Default
        };
    }
}