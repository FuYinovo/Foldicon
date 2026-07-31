using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Foldicon.Struct;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.ViewModels.Dialog;

public partial class PickIconFromGroupDialogViewModel : ObservableObject
{
    private readonly IconGroup _group;

    public PickIconFromGroupDialogViewModel(IconGroup group)
    {
        _group = group;
        FilteredIcons = [.. group.Icons];
    }

    [ObservableProperty] public partial int SelectedIndex { get; set; } = -1;
    [ObservableProperty] public partial string IconNameFilter { get; set; } = string.Empty;
    [ObservableProperty] public partial ObservableCollection<BitmapIcon> FilteredIcons { get; set; }

    partial void OnIconNameFilterChanged(string value) => ApplyFilter();
    private void ApplyFilter()
    {
        FilteredIcons.Clear();
        foreach (var icon in _group.Icons)
        {
            if (icon.FileName is not null && icon.FileName.Contains(IconNameFilter))
                FilteredIcons.Add(icon);
        }
    }
}