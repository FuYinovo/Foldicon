using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Contracts;
using Foldicon.Enums;
using Foldicon.Messages;
using Foldicon.ViewModels.Page;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using IconEditorPage = Foldicon.Views.Page.IconEditorPage;
using IconGroup = Foldicon.Models.IconGroup;
using IconGroupPage = Foldicon.Views.Page.IconGroupPage;
using IconGroupsDetailPage = Foldicon.Views.Page.IconGroupsDetailPage;
using SettingsPage = Foldicon.Views.Page.SettingsPage;

namespace Foldicon;

public sealed partial class MainWindow
{
    private IIconGroupService IconGroupService { get; }
    private INavigationService NavigationService { get; }
    private IDialogService DialogService { get; }
    private static readonly Type DefaultPage = typeof(IconEditorPage);

    public MainWindow()
    {
        InitializeComponent();
        RegisterMessages();
        Activated += OnActivated;
        IconGroupService = App.Services.GetRequiredService<IIconGroupService>();
        NavigationService = App.Services.GetRequiredService<INavigationService>();
        DialogService = App.Services.GetRequiredService<IDialogService>();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        NavigationService.Initialize(NavigationFrame);
        NavigationService.Navigate(DefaultPage);

        DialogService.Initialize(this);

        Activated -= OnActivated; // 防止每次从最小化恢复都触发
    }

    private void RegisterMessages()
    {
        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, AppThemeChangedMessageHandler);
        WeakReferenceMessenger.Default.Register<AppBackdropChangedMessage>(this, AppBackdropChangedMessageHandler);
    }


    /// <summary>
    ///     响应 NavigationView 的跳转点击
    /// </summary>
    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // 设置页面
        if (args.IsSettingsSelected)
        {
            NavigationService.Navigate(typeof(SettingsPage));
            return;
        }

        // 动态页面
        if (args.SelectedItem is IconGroup group)
        {
            NavigationService.Navigate(typeof(IconGroupsDetailPage), group);
            return;
        }

        // 静态页面
        if (sender.SelectedItem is not NavigationViewItem { Tag: string tag }) return;
        var target = tag switch
        {
            "IconEditor" => typeof(IconEditorPage),
            "IconGroup" => typeof(IconGroupPage),
            _ => null
        };
        if (target is not null) NavigationService.Navigate(target);
    }

    #region Message Handler

    private void AppThemeChangedMessageHandler(object _, AppThemeChangedMessage msg)
    {
        var theme = msg.NewTheme switch
        {
            AppThemeEnum.System => ElementTheme.Default,
            AppThemeEnum.Dark => ElementTheme.Dark,
            AppThemeEnum.Light => ElementTheme.Light,
            _ => ElementTheme.Default
        };
        TrySetTheme(theme);
    }

    private void AppBackdropChangedMessageHandler(object _, AppBackdropChangedMessage msg)
    {
        var backdrop = msg.NewBackdrop;
        switch (backdrop)
        {
            case AppBackdropEnum.Mica:
                TrySetMicaBackdrop(MicaKind.Base);
                break;
            case AppBackdropEnum.MicaAlt:
                TrySetMicaBackdrop(MicaKind.BaseAlt);
                break;
            case AppBackdropEnum.Acrylic:
                TrySetAcrylicBackdrop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backdrop), backdrop, null);
        }
    }

    #endregion

    #region UI

    private bool CountToBool(int count) => count > 0;

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

    #endregion
}