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

    public async Task<ContentDialogResult> ShowAsync(string title, string primaryText, string? closeText,
        object content,
        ContentDialogButton defaultButton = ContentDialogButton.Primary)
    {
        return await new ContentDialog
        {
            RequestedTheme = Window.GetRequestedTheme(),
            Title = title,
            PrimaryButtonText = primaryText,
            CloseButtonText = closeText,
            Content = content,
            XamlRoot = Window.Content.XamlRoot,
            DefaultButton = defaultButton
        }.ShowAsync();
    }
}