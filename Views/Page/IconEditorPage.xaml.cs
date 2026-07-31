using Foldicon.ViewModels.Page;

namespace Foldicon.Views.Page;

public sealed partial class IconEditorPage
{
    public IconEditorPageViewModel ViewModel { get; } = new();

    public IconEditorPage()
    {
        InitializeComponent();
        Loaded += (_, _) => ViewModel.XamlRoot = XamlRoot;
    }
}
