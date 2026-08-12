using System;
using System.Threading.Tasks;
using ABI.Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Contracts;

public interface IDialogService
{
    void Initialize(MainWindow window);
    MainWindow Window { get; }
    WindowId WindowId => Window.AppWindow.Id;

    Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog);

    Task<ContentDialogResult> ShowDialogAsync(
        string title,
        string primaryText = "确定",
        string? closeText = "取消",
        string? secondaryText = null,
        object? content = null,
        ContentDialogButton defaultButton = ContentDialogButton.Primary);

    Task<ContentDialogResult> ShowMessageAsync(string title, string message, bool closeButton = false);
}