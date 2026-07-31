using Foldicon.Helpers;
using Foldicon.ViewModels.Page;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Views.Page;

public sealed partial class IconEditorPage
{
    public IconEditorPageViewModel ViewModel { get; } = new();

    public IconEditorPage()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     「选择文件夹」按钮
    /// </summary>
    private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.IsEnabled = false;

        var fullPath = await StoragePicker.PickFolder(btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (fullPath is not null)
            await ViewModel.LoadFolderAsync(fullPath);

        btn.IsEnabled = true;
    }
}
