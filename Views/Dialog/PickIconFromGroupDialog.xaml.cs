using Foldicon.Contracts;
using Foldicon.ViewModels.Dialog;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.Views.Dialog;

public partial class PickIconFromGroupDialog
{
    private PickIconFromGroupDialogViewModel ViewModel { get; }

    public PickIconFromGroupDialog(IconGroup group)
    {
        ViewModel = new PickIconFromGroupDialogViewModel(group);
        InitializeComponent();
    }

    public IFolderIcon? GetSelectedIcon()
    {
        var index = ViewModel.SelectedIndex;
        var count = ViewModel.FilteredIcons.Count;
        var icons = ViewModel.FilteredIcons;

        if (index >= 0 && index < count) return icons[index];
        return null;
    }
}