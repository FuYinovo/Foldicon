using System.Threading.Tasks;
using Foldicon.Views.Dialog;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Contracts;

public interface IDialogService
{
    const string DefaultPrimaryText = "确定";
    const string DefaultCloseText = "取消";
    void Initialize(MainWindow window);
    MainWindow Window { get; }
    WindowId WindowId => Window.AppWindow.Id;

    Task<ContentDialogResult> ShowAsync(
        string title,
        string primaryText,
        string? closeText,
        object content,
        ContentDialogButton defaultButton = ContentDialogButton.Primary);

    Task<ContentDialogResult> ShowDialogAsync(string title, object content)
    {
        return ShowAsync(title, DefaultPrimaryText, DefaultCloseText, content);
    }

    Task<ContentDialogResult> ShowDialogAsync(string title, string message, bool showCloseButton = false)
    {
        return ShowAsync(title, DefaultPrimaryText, showCloseButton ? DefaultCloseText : null,
            new DescriptionDialog(message));
    }
}