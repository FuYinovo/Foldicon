using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;

namespace Foldicon.Contracts;

public interface INotificationService
{
    void SendToApp(string title, string message, InfoBarSeverity severity, int durationSeconds);
    void SendToSystem(AppNotification notification);
}