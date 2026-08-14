using System;
using Foldicon.Messages;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Models;

public class InfoBarNotification(SendAppNotificationMessage msg)
{
    public string Title { get; } = msg.Title;
    public string Message { get; } = msg.Message;
    public InfoBarSeverity Severity { get; } = msg.Severity;
    public TimeSpan Duration { get; } = msg.Duration;
}