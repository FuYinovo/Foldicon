using Foldicon.ViewModels.Page;
using Microsoft.Extensions.DependencyInjection;

namespace Foldicon.Views.Page;

public sealed partial class IconEditorPage
{
    public IconEditorPageViewModel ViewModel { get; } = App.Services.GetRequiredService<IconEditorPageViewModel>();

    public IconEditorPage()
    {
        InitializeComponent();
    }
}
