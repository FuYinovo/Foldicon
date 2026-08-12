
using Foldicon.Enums;

namespace Foldicon.Messages;

public record AppThemeChangedMessage
{
    public required AppThemeEnum NewTheme { get; init; }
}