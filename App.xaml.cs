using System.Diagnostics;
using System.Text;
using Foldicon.Contracts;
using Foldicon.Helpers;
using Foldicon.Services;
using Foldicon.ViewModels.Dialog;
using Foldicon.ViewModels.Page;
using Foldicon.ViewModels.UserControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Foldicon;

public partial class App
{
    public static ServiceProvider Services { get; } = ConfigureServices();
    public static MainWindow MainWindow { get; } = new();

    public App()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        Debug.WriteLine($"[ERROR] {args.Message}");
        args.Handled = true;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Activate();
    }

    private static ServiceProvider ConfigureServices()
    {
        var builder = new ServiceCollection();

        // MainWindow
        builder.AddSingleton<MainWindow>(_ => MainWindow);

        // ViewModel
        builder.AddTransient<IconEditorPageViewModel>();
        builder.AddTransient<IconGroupPageViewModel>();
        builder.AddTransient<IconGroupsDetailPageViewModel>();
        builder.AddTransient<IconGroupInfoDialogViewModel>();
        builder.AddTransient<FolderEntryControlViewModel>();
        builder.AddTransient<SettingsPageViewModel>();

        // Service
        builder.AddSingleton<IIconGroupService, IconGroupService>();
        builder.AddSingleton<IDialogService, DialogService>();
        builder.AddSingleton<INavigationService, NavigationService>();
        builder.AddSingleton<INotificationService, NotificationService>();
        builder.AddSingleton<IUserDataService, UserDataService>();
        builder.AddSingleton<IOptionService, JsonOptionService>();

        return builder.BuildServiceProvider();
    }
}