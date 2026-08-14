using System;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Messages;
using Foldicon.Models;
using Foldicon.ViewModels.UserControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Views.UserControl;

public sealed partial class NotificationBarControl
{
    public NotificationBarControl()
    {
        InitializeComponent();
    }

    private readonly NotificationBarControlViewModel _viewModel =
        App.Services.GetRequiredService<NotificationBarControlViewModel>();

    private void InfoBar_OnClosed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        if (sender is not { DataContext: InfoBarNotification notification }) return;
        _viewModel.Notifications.Remove(notification);
    }

    private void AddTest(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new SendAppNotificationMessage
        {
            Title = "test",
            Message = "sth",
            Severity = InfoBarSeverity.Informational,
            Duration = new TimeSpan(0,0,5)
        });
    }

    private void DebugTest(object sender, RoutedEventArgs e)
    {
        var a = _viewModel.Notifications;
        var b = true;
    }
}