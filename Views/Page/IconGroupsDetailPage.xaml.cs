using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.ViewModels.Page;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using BitmapIcon = Foldicon.Struct.BitmapIcon;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.Views.Page;

[ObservableObject]
public sealed partial class IconGroupsDetailPage
{
    private IconGroupsDetailPageViewModel ViewModel { get; } = new();

    public IconGroupsDetailPage()
    {
        InitializeComponent();
        Loaded += (_, _) => ViewModel.XamlRoot = XamlRoot;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is IconGroup group)
            ViewModel.Update(group);
    }

    private void Icon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: BitmapIcon icon })
            ViewModel.OpenIconFileCommand.Execute(icon);
    }

    private void DeleteIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: BitmapIcon icon })
            ViewModel.DeleteIconCommand.Execute(icon);
    }
}