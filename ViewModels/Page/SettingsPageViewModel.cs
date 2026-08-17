using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Contracts;
using Foldicon.Enums;
using Foldicon.Helpers;
using Foldicon.Messages;
using Foldicon.Record;

namespace Foldicon.ViewModels.Page;

public partial class SettingsPageViewModel(IOptionService optionService) : ObservableObject
{
    #region ItemsSource

    public List<LocalizationItem<AppThemeEnum>> AppThemes { get; } = LocalizationHelper.GetEnums<AppThemeEnum>();

    public List<LocalizationItem<AppBackdropEnum>> AppBackdrops { get; } =
        LocalizationHelper.GetEnums<AppBackdropEnum>();

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
        WeakReferenceMessenger.Default.Send(new AppThemeChangedMessage { NewTheme = value });
        optionService.SaveAll();
    }

    partial void OnSelectedAppBackdropChanged(AppBackdropEnum value)
    {
        optionService.Options.AppBackdrop = value;
        WeakReferenceMessenger.Default.Send(new AppBackdropChangedMessage { NewBackdrop = value });
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
}