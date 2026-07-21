using System;
using System.Text;
using Foldicon.Page;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon;

public sealed partial class MainWindow
{
    private static readonly Type DefaultPage = typeof(IconEditorPage);

    public MainWindow()
    {
        InitializeComponent();
        ContentFrame.Navigate(DefaultPage);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // 设置页面
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        // 其他页面
        if (sender.SelectedItem is not NavigationViewItem item) return;
        if (item.Tag is not string tag) return;
        ContentFrame.Navigate(tag switch
        {
            _ => DefaultPage
        });
    }
}