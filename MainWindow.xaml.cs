using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Foldicon.Class;
using Foldicon.Service;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using IconEditorPage = Foldicon.Xaml.Page.IconEditorPage;
using IconGroupPage = Foldicon.Xaml.Page.IconGroupPage;
using IconGroupsCategorizePage = Foldicon.Xaml.Page.IconGroupsCategorizePage;
using IconGroupsDetailPage = Foldicon.Xaml.Page.IconGroupsDetailPage;
using SettingsPage = Foldicon.Xaml.Page.SettingsPage;

namespace Foldicon;

public sealed partial class MainWindow
{
    private static readonly Type DefaultPage = typeof(IconEditorPage);

    public MainWindow()
    {
        InitializeComponent();
        NavigateTo(DefaultPage);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static ObservableCollection<IconGroup> IconGroups => IconGroupService.Instance.Groups;

    /// <summary>
    ///     尝试设置窗口背景为亚克力
    /// </summary>
    /// <returns>是否成功</returns>
    public bool TrySetAcrylicBackdrop()
    {
        if (!DesktopAcrylicController.IsSupported()) return false;
        var backdrop = new DesktopAcrylicBackdrop();
        SystemBackdrop = backdrop;
        return true;
    }

    /// <summary>
    ///     尝试设置窗口背景为云母
    /// </summary>
    /// <param name="style">云母样式</param>
    /// <returns>是否成功</returns>
    public bool TrySetMicaBackdrop(MicaKind style)
    {
        if (!MicaController.IsSupported()) return false;
        var backdrop = new MicaBackdrop
        {
            Kind = style
        };
        SystemBackdrop = backdrop;
        return true;
    }

    /// <summary>
    ///     尝试设置窗口颜色主题
    /// </summary>
    /// <param name="theme">主题样式</param>
    /// <returns>是否成功</returns>
    public bool TrySetTheme(ElementTheme theme)
    {
        try
        {
            ((FrameworkElement)Content).RequestedTheme = theme;
            AppWindow.TitleBar.PreferredTheme = theme switch
            {
                ElementTheme.Default => TitleBarTheme.UseDefaultAppMode,
                ElementTheme.Light => TitleBarTheme.Light,
                ElementTheme.Dark => TitleBarTheme.Dark,
                _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
            };

            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    /// <summary>
    ///     获取当前颜色主题
    /// </summary>
    public ElementTheme GetRequestedTheme()
    {
        try
        {
            return ((FrameworkElement)Content).RequestedTheme;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return ElementTheme.Default;
        }
    }

    /// <summary>
    ///     让 NavigationView 跳转到某个页面
    /// </summary>
    /// <param name="targetPage">页面类型</param>
    /// <param name="parm">传参（null即留空）</param>
    public void NavigateTo(Type targetPage, object? parm = null)
    {
        if (parm is null) ContentFrame.Navigate(targetPage);
        else ContentFrame.Navigate(targetPage, parm);
    }

    /// <summary>
    ///     响应 NavigationView 的跳转点击
    /// </summary>
    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // 设置页面
        if (args.IsSettingsSelected)
        {
            NavigateTo(typeof(SettingsPage));
            return;
        }

        // 动态页面
        if (args.SelectedItem is IconGroup group)
        {
            NavigateTo(typeof(IconGroupsDetailPage), group);
            return;
        }

        // 静态页面
        if (sender.SelectedItem is not NavigationViewItem item) return;
        if (item.Tag is not string tag) return;
        NavigateTo(tag switch
        {
            "IconEditor" => typeof(IconEditorPage),
            "IconGroup" => typeof(IconGroupPage),
            "IconGroupCategorize" => typeof(IconGroupsCategorizePage),
            _ => DefaultPage
        });
    }
}