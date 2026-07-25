using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Class;
using Foldicon.Service;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Foldicon.Page;

[ObservableObject]
public sealed partial class IconGroupsDetailPage
{
    [ObservableProperty] private IconGroup _group = new();

    public IconGroupsDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // 接收新的 IconGroup 作为 ViewModel
        if (e.Parameter is IconGroup group) Group = group;
    }

    /// <summary>
    /// 点击「导入图标」按键
    /// </summary>
    private async void ImportIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        // 选取图标
        var icons = await StoragePicker.PickFiles(IconGroupService.SupportedIconExtensions,
            btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        if (icons.Length == 0) return;

        // 导入图标
        foreach (var icon in icons) Group.Add(icon);
    }
}