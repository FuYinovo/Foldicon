using Foldicon.Enums;

namespace Foldicon.Messages;

public record AppBackdropChangedMessage
{
    public required AppBackdropEnum NewBackdrop { get; init; }
}