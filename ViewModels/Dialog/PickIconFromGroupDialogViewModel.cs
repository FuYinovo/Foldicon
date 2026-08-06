using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Foldicon.Contracts;
using Microsoft.Extensions.DependencyInjection;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.ViewModels.Dialog;

public partial class PickIconFromGroupDialogViewModel : ObservableObject
{
    private readonly IconGroup _group;
    private readonly IOptionService _optionService;

    public PickIconFromGroupDialogViewModel(IconGroup group)
    {
        _group = group;
        _optionService = App.Services.GetService<IOptionService>()!;
        FilteredIcons = [.. group.Icons];
    }

    [ObservableProperty] public partial int SelectedIndex { get; set; } = -1;
    [ObservableProperty] public partial string IconNameFilter { get; set; } = string.Empty;
    [ObservableProperty] public partial ObservableCollection<IFolderIcon> FilteredIcons { get; set; }

    partial void OnIconNameFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredIcons.Clear();
        foreach (var icon in _group.Icons)
        {
            var comparison = _optionService.Options.IsCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            if (icon.DisplayName.Contains(IconNameFilter, comparison))
                FilteredIcons.Add(icon);
        }
    }
}