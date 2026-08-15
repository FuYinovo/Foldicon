using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;

namespace Foldicon.Contracts;

public interface INotificationService
{
    /// <summary>
    ///     向应用发送一条通知
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">信息</param>
    /// <param name="severity">级别</param>
    /// <param name="durationSeconds">展示时长(秒)</param>
    /// <param name="isPermanent">是否永久展示(禁用<see cref="durationSeconds"/>)</param>
    void SendToApp(string title, string message, InfoBarSeverity severity, int durationSeconds = 5, bool isPermanent = false);

    /// <summary>
    ///     向系统发送一条通知
    /// </summary>
    void SendToSystem(AppNotification notification);
}