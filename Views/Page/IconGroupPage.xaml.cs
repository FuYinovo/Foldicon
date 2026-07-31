using Foldicon.ViewModels.Page;
using Microsoft.UI.Xaml;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.Views.Page;

public sealed partial class IconGroupPage
{
    public IconGroupPageViewModel ViewModel { get; } = new();

    public IconGroupPage()
    {
        InitializeComponent();
        Loaded += (_, _) => ViewModel.XamlRoot = XamlRoot;
    }

    private void ImportIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.ImportIconAsyncCommand.Execute(group);
    }

    private void EditGroupInfo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.EditGroupInfoAsyncCommand.Execute(group);
    }

    private void RemoveGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.RemoveGroupAsyncCommand.Execute(group);
    }

    private void OpenGroupFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.OpenGroupFolderCommand.Execute(group);
    }

    private void IconGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.NavigateGroupPageCommand.Execute(group);
    }
}