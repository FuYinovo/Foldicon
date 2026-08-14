using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Messages;
using Foldicon.Models;
using Microsoft.UI.Dispatching;

namespace Foldicon.ViewModels.UserControl;

public partial class NotificationBarControlViewModel : ObservableObject
{
    public ObservableCollection<InfoBarNotification> Notifications { get; } = [];

    public NotificationBarControlViewModel()
    {
        WeakReferenceMessenger.Default.Register<SendAppNotificationMessage>(this, (_, message) =>
        {
            var notification = new InfoBarNotification(message);
            Notifications.Add(notification);

            // Auto close
            var uiThread = DispatcherQueue.GetForCurrentThread();
            Task.Run(async () =>
            {
                await Task.Delay(notification.Duration);
                uiThread.TryEnqueue(() => Notifications.Remove(notification));
            });
        });
    }

    [RelayCommand]
    private void Clear() => Notifications.Clear();
}