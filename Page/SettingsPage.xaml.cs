using Foldicon.Service;

namespace Foldicon.Page;

public sealed partial class SettingsPage
{
    private readonly OptionService _viewModel = OptionService.Instance;

    public SettingsPage()
    {
        InitializeComponent();
    }
}