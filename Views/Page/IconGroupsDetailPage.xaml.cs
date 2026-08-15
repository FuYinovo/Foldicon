using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Foldicon.Contracts;
using Foldicon.ViewModels.Page;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.Views.Page;

public sealed partial class IconGroupsDetailPage
{
    private IconGroupsDetailPageViewModel ViewModel { get; } =
        App.Services.GetRequiredService<IconGroupsDetailPageViewModel>();

    public IconGroupsDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is IconGroup group)
            ViewModel.Update(group);
    }

    private void Icon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IFolderIcon icon })
            ViewModel.OpenIconFileCommand.Execute(icon);
    }

    private void DeleteIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IFolderIcon icon })
            ViewModel.DeleteIconCommand.Execute(icon);
    }

    private void OpenIconFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IFolderIcon icon })
            ViewModel.OpenIconFolderCommand.Execute(icon);
    }

    private async void DropFileBehavior_OnFileDropped(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: IFolderIcon icon }) return;
        if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

        var folderPaths = (await e.DataView.GetStorageItemsAsync())
            .Where(item => (item.Attributes & FileAttributes.Directory) != 0)
            .Select(item => item.Path)
            .ToArray();
        ViewModel.ApplyToFoldersCommand.Execute((folderPaths, icon));
    }
}