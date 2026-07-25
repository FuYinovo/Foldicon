using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Class;
using Microsoft.UI.Xaml.Navigation;

namespace Foldicon.Page;

[ObservableObject]
public sealed partial class IconGroupsDetailPage
{
    [ObservableProperty] private IconGroup _group = new();

    public IconGroupsDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // 接收新的 IconGroup 作为 ViewModel
        if (e.Parameter is IconGroup group) Group = group;
    }
}