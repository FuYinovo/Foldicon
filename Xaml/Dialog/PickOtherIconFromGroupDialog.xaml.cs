using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Class;
using Foldicon.Struct;

namespace Foldicon.Xaml.Dialog;

[ObservableObject]
public partial class PickOtherIconFromGroupDialog
{
    private readonly IconGroup _group;

    public PickOtherIconFromGroupDialog(IconGroup group)
    {
        _group = group;
        FilteredIcons = [.. group.Icons];
        InitializeComponent();
    }

    [ObservableProperty] public partial int SelectedIndex { get; set; } = -1;
    [ObservableProperty] public partial string IconNameFilter { get; set; } = string.Empty;
    [ObservableProperty] public partial ObservableCollection<BitmapIcon> FilteredIcons { get; set; }

    public string? GetSelectedIconPath()
    {
        if (SelectedIndex > 0 && SelectedIndex < FilteredIcons.Count) return FilteredIcons[SelectedIndex].FullPath;

        return null;
    }

    partial void OnIconNameFilterChanged(string value)
    {
        ApplyFilter();
    }

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