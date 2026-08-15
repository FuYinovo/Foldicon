using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Messages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Foldicon.Views.UserControl;

public sealed partial class NotificationBarControl
{
    private int NotificationCount => NotificationPanel.Children.Count;
    private static readonly TimeSpan NotificationDuration = TimeSpan.FromSeconds(0.3);
    private static readonly TimeSpan ContainerDuration = TimeSpan.FromSeconds(0.4);
    private const int NotificationCornerRadius = 8;
    private const string ClosingTag = "Closing";

    public NotificationBarControl()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<NotificationMessage>(this, NotificationMessageHandler);
    }

    private async void NotificationMessageHandler(object recipient, NotificationMessage message)
    {
        var notification = CreateInfoBar(message);
        await InsertNotificationAsync(notification); // 插入
        await Task.Delay(message.Duration); // 生命周期
        await DismissNotificationAsync(notification); // 移除
    }

    private InfoBar CreateInfoBar(NotificationMessage msg)
    {
        var infoBar = new InfoBar
        {
            CornerRadius = new CornerRadius(NotificationCornerRadius),
            IsOpen = true,
            Severity = msg.Severity,
            Title = msg.Title,
            Message = msg.Message,
        };
        infoBar.Closing += async (_, args) =>
        {
            args.Cancel = true; // 接管 Close 逻辑
            await DismissNotificationAsync(infoBar);
        };
        return infoBar;
    }

    #region Animation

    /// <summary>
    ///     播放容器动画
    /// </summary>
    /// <param name="element">容器元素</param>
    /// <param name="isEntrance">是否为入场动画</param>
    private static async Task PlayContainerAnimationAsync(FrameworkElement element, bool isEntrance)
    {
        // Init
        element.RenderTransform = new ScaleTransform
        {
            CenterX = element.ActualWidth == 0 ? element.MinWidth : element.ActualWidth,
            CenterY = element.ActualHeight == 0 ? element.MinHeight : element.ActualHeight,
        };
        var easingFunc = new CubicEase { EasingMode = isEntrance ? EasingMode.EaseOut : EasingMode.EaseIn };

        // Scale
        var scaleFrom = isEntrance ? 0 : 1;
        var scaleTo = isEntrance ? 1 : 0;
        var scaleXAnim = new DoubleAnimation
        {
            From = scaleFrom,
            To = scaleTo,
            Duration = ContainerDuration,
            EasingFunction = easingFunc
        };
        var scaleYAnim = new DoubleAnimation
        {
            From = scaleFrom,
            To = scaleTo,
            Duration = ContainerDuration,
            EasingFunction = easingFunc
        };
        Storyboard.SetTarget(scaleXAnim, element.RenderTransform);
        Storyboard.SetTarget(scaleYAnim, element.RenderTransform);
        Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");
        Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");

        // Opacity
        var opacityAnim = new DoubleAnimation
        {
            From = isEntrance ? 0 : 1,
            To = isEntrance ? 1 : 0,
            Duration = ContainerDuration,
            EasingFunction = easingFunc
        };
        Storyboard.SetTarget(opacityAnim, element);
        Storyboard.SetTargetProperty(opacityAnim, "Opacity");

        // Begin
        var sb = new Storyboard();
        sb.Children.Add(scaleXAnim);
        sb.Children.Add(scaleYAnim);
        sb.Children.Add(opacityAnim);
        sb.Begin();
        await Task.Delay(ContainerDuration);
    }

    /// <summary>
    ///     播放通知动画
    /// </summary>
    /// <param name="infoBar">通知控件</param>
    /// <param name="isEntrance">是否为入场动画</param>
    private static async Task PlayNotificationAnimationAsync(InfoBar infoBar, bool isEntrance)
    {
        // Init
        infoBar.RenderTransform = new TranslateTransform();
        var easingFunc = new CircleEase { EasingMode = isEntrance ? EasingMode.EaseOut : EasingMode.EaseIn };

        // OffsetX
        var offsetXAnim = new DoubleAnimation
        {
            From = isEntrance ? 300 : 0,
            To = isEntrance ? 0 : 300,
            Duration = NotificationDuration,
            EasingFunction = easingFunc
        };
        Storyboard.SetTarget(offsetXAnim, infoBar.RenderTransform);
        Storyboard.SetTargetProperty(offsetXAnim, "X");

        // Opacity
        var opacityAnim = new DoubleAnimation
        {
            From = isEntrance ? 0 : 1,
            To = isEntrance ? 1 : 0,
            Duration = NotificationDuration,
            EasingFunction = easingFunc
        };
        Storyboard.SetTarget(opacityAnim, infoBar);
        Storyboard.SetTargetProperty(opacityAnim, "Opacity");

        // Begin
        var storyboard = new Storyboard();
        storyboard.Children.Add(opacityAnim);
        storyboard.Children.Add(offsetXAnim);
        storyboard.Begin();
        await Task.Delay(NotificationDuration);
    }

    /// <summary>
    ///     根据当前通知数量，更新容器的<see cref="Visibility"/>并播放动画
    /// </summary>
    /// <param name="element">容器元素</param>
    /// <param name="notificationCount">当前通知数量</param>
    private static async Task UpdateContainerVisualAsync(FrameworkElement element, int notificationCount)
    {
        switch (notificationCount)
        {
            case > 0 when element.Visibility == Visibility.Collapsed:
                SetVisibility(Visibility.Visible);
                await PlayContainerAnimationAsync(element, true);
                break;
            case 0 when element.Visibility == Visibility.Visible:
                await PlayContainerAnimationAsync(element, false);
                SetVisibility(Visibility.Collapsed);
                break;
        }

        return;

        void SetVisibility(Visibility visibility) =>
            DispatcherQueue.GetForCurrentThread().TryEnqueue(() => element.Visibility = visibility);
    }

    #endregion

    #region Add, Remove, Claer

    /// <summary>
    ///     插入一个通知并播放动画
    /// </summary>
    private async Task InsertNotificationAsync(InfoBar infoBar)
    {
        _ = UpdateContainerVisualAsync(Container, NotificationCount + 1);
        NotificationPanel.Children.Insert(0, infoBar);
        await PlayNotificationAnimationAsync(infoBar, true); // 入场动画
    }

    /// <summary>
    ///     移除一个通知并播放动画
    /// </summary>
    private async Task DismissNotificationAsync(InfoBar infoBar)
    {
        if (infoBar.Tag is ClosingTag) return;
        infoBar.Tag = ClosingTag;

        _ = UpdateContainerVisualAsync(Container, NotificationCount - 1);
        await PlayNotificationAnimationAsync(infoBar, false); // 离场动画
        NotificationPanel.Children.Remove(infoBar);
    }

    /// <summary>
    ///     清空通知并播放动画
    /// </summary>
    private void Clear()
    {
        foreach (var element in NotificationPanel.Children)
        {
            if (element is not InfoBar infoBar) continue;
            _ = DismissNotificationAsync(infoBar);
        }

        _ = UpdateContainerVisualAsync(Container, 0);
    }

    #endregion

    #region Button Click Handler

    private void Clear_ButtonClick(object sender, RoutedEventArgs e) => Clear();

    #endregion
}