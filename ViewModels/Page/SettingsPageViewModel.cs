using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Contracts;
using Foldicon.Enums;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace Foldicon.ViewModels.Page;

public  partial class SettingsPageViewModel(IOptionService optionService, MainWindow window) : ObservableObject
{
    #region ItemsSource

    public List<AppThemeEnum> AppThemes { get; } = GetEnumValues<AppThemeEnum>();
    public List<AppBackdropEnum> AppBackdrops { get; } = GetEnumValues<AppBackdropEnum>();

    #endregion

    #region Options

    [ObservableProperty] public partial AppThemeEnum SelectedAppTheme { get; set; } = optionService.Options.AppTheme;

    [ObservableProperty]
    public partial AppBackdropEnum SelectedAppBackdrop { get; set; } = optionService.Options.AppBackdrop;

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