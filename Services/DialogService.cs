using System;
using System.Threading.Tasks;
using Foldicon.Contracts;
using Foldicon.Views.Dialog;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Services;

public class DialogService : IDialogService
{
    private MainWindow? _window;
    public MainWindow Window => _window ?? throw new Exception("未调用 IDialogService.Initialize() 进行初始化");
    public void Initialize(MainWindow window) => _window = window;

    public async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
    {
        dialog.RequestedTheme = Window.GetRequestedTheme();
        dialog.XamlRoot = Window.Content.XamlRoot;
        return await dialog.ShowAsync();
    }

    public async Task<ContentDialogResult> ShowDialogAsync(
        string title,
        string primaryText = "确定",
        string? closeText = "取消",
        string? secondaryText = null,
        object? content = null,
        ContentDialogButton defaultButton = ContentDialogButton.Primary)
    {
        return await new ContentDialog
        {
            RequestedTheme = Window.GetRequestedTheme(),
            Title = title,
            PrimaryButtonText = primaryText,
            CloseButtonText = closeText,
            SecondaryButtonText = secondaryText,
            Content = content,
            XamlRoot = Window.Content.XamlRoot,
            DefaultButton = defaultButton
        }.ShowAsync();
    }

    public async Task<ContentDialogResult> ShowMessageAsync(string title, string message, bool closeButton = false)
    {
        return await new ContentDialog
        {
            RequestedTheme = Window.GetRequestedTheme(),
            Title = title,
            Content = new DescriptionDialog(message),
            PrimaryButtonText = "确认",
            CloseButtonText = closeButton ? "取消" : null,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = Window.Content.XamlRoot
        }.ShowAsync();
    }
}