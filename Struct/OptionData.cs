using Foldicon.Enums;

namespace Foldicon.Struct;

public record OptionData
{
    public AppThemeEnum AppTheme { get; set; } = AppThemeEnum.System;
    public AppBackdropEnum AppBackdrop { get; set; } = AppBackdropEnum.Mica;
}