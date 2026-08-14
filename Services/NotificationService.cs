using System;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Contracts;
using Foldicon.Messages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;

namespace Foldicon.Services;

public class NotificationService : INotificationService
{
    public void SendToApp(string title, string message, InfoBarSeverity severity, int durationSeconds = 5)
    {
        WeakReferenceMessenger.Default.Send(new SendAppNotificationMessage
            { Title = title, Message = message, Severity = severity, Duration = new TimeSpan(0, 0, durationSeconds) });
    }

    public void SendToSystem(AppNotification notification)
    {
        AppNotificationManager.Default.Show(notification);
    }
}