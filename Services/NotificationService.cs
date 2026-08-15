using System;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Contracts;
using Foldicon.Messages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;

namespace Foldicon.Services;

public class NotificationService : INotificationService
{
    public void SendToApp(string title, string message, InfoBarSeverity severity, int durationSeconds = 5,
        bool isPermanent = false)
    {
        WeakReferenceMessenger.Default.Send(new NotificationMessage
        {
            Title = title,
            Message = message,
            Severity = severity,
            Duration = isPermanent ? TimeSpan.FromDays(365) : TimeSpan.FromSeconds(durationSeconds)
        });
    }

    public void SendToSystem(AppNotification notification)
    {
        AppNotificationManager.Default.Show(notification);
    }
}