using System;
using System.Threading.Tasks;
using Foldicon.Contracts;
using Foldicon.Views.Dialog;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Services;

public class DialogService(MainWindow? window) : IDialogService
{
    public MainWindow Window { get; private set; } = window!;  // Fresh() 兜底

    public WindowId WindowId
    {
        get
        {
            Refresh();
            return Window.AppWindow.Id;
        }
    }

    public async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
    {
        Refresh();
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
        Refresh();
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
        Refresh();
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

    /// <summary>
    /// 修复 Service 注册顺序导致的 null
    /// </summary>
    private void Refresh() => Window = App.MainWindow;
}