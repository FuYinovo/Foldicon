using System.Diagnostics;
using Foldicon.Service;
using Microsoft.UI.Xaml;


namespace Foldicon;

public partial class App
{
    public static MainWindow MainWindow { get; } = MainWindow = new MainWindow();

    // 所有 Service 随 App 启动
    public static OptionService OptionService { get; private set; } = OptionService.Instance;
    public static IconGroupService IconGroupService { get; private set; } = IconGroupService.Instance;

    public App()
    {
        InitializeComponent();
        UnhandledException += (sender, args) =>
        {
            Debug.WriteLine($"[ERROR] {args.Message}");
            throw args.Exception;
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Activate();
    }
}