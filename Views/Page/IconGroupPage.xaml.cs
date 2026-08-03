using Foldicon.ViewModels.Page;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using IconGroup = Foldicon.Models.IconGroup;

namespace Foldicon.Views.Page;

public sealed partial class IconGroupPage
{
    public IconGroupPageViewModel ViewModel { get; } =  App.Services.GetRequiredService<IconGroupPageViewModel>();

    public IconGroupPage()
    {
        InitializeComponent();
    }

    private void ImportIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.ImportIconCommand.Execute(group);
    }

    private void EditGroupInfo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.EditGroupInfoCommand.Execute(group);
    }

    private void RemoveGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IconGroup group })
            ViewModel.RemoveGroupCommand.Execute(group);
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