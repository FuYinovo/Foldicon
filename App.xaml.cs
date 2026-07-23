using Foldicon.Service;
using Microsoft.UI.Xaml;


namespace Foldicon;

public partial class App
{
    public static MainWindow MainWindow { get; } = MainWindow = new MainWindow();
    public static OptionService OptionService { get; private set; } = OptionService.Instance; // 在 App 启动前使设置选项就绪


    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Activate();
    }
}