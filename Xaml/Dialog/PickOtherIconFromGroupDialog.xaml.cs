using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Class;
using Foldicon.Struct;

namespace Foldicon.Xaml.Dialog;

[ObservableObject]
public sealed partial class PickOtherIconFromGroupDialog
{
    private readonly IconGroup _group;
    [ObservableProperty] private int _selectedIndex = -1;
    [ObservableProperty] private string _iconNameFilter = string.Empty;
    [ObservableProperty] private ObservableCollection<BitmapIcon> _filteredIcons;

    public PickOtherIconFromGroupDialog(IconGroup group)
    {
        _group = group;
        _filteredIcons = [.. group.Icons];
        InitializeComponent();
    }

    public string? GetSelectedIconPath()
    {
        if (SelectedIndex > 0 && SelectedIndex < FilteredIcons.Count)
        {
            return FilteredIcons[SelectedIndex].FullPath;
        }

        return null;
    }

    partial void OnIconNameFilterChanged(string value) => ApplyFilter();
    private void ApplyFilter()
    {
        FilteredIcons.Clear();
        foreach (var icon in _group.Icons)
        {
            if (icon.FileName is not null && icon.FileName.Contains(IconNameFilter))
            {
                FilteredIcons.Add(icon);
            }
        }
    }
}