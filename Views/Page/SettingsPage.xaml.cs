
using OptionService = Foldicon.Services.OptionService;

namespace Foldicon.Views.Page;

public sealed partial class SettingsPage
{
    private readonly OptionService _viewModel = OptionService.Instance;

    public SettingsPage()
    {
        InitializeComponent();
    }
}