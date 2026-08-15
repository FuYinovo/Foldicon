using System;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Messages;

public record NotificationMessage
{
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required InfoBarSeverity Severity { get; init; }
    public required TimeSpan Duration { get; init; }
};