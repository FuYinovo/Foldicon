using Foldicon.ViewModels.Page;
using Microsoft.Extensions.DependencyInjection;

namespace Foldicon.Views.Page;

public sealed partial class SettingsPage
{
    public readonly SettingsPageViewModel ViewModel = App.Services.GetRequiredService<SettingsPageViewModel>();

    public SettingsPage()
    {
        InitializeComponent();
    }
}