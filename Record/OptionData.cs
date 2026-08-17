using Foldicon.Enums;

namespace Foldicon.Record;

public record OptionData
{
    public AppThemeEnum AppTheme { get; set; } = AppThemeEnum.System;
    public AppBackdropEnum AppBackdrop { get; set; } = AppBackdropEnum.Mica;
    public bool IsCaseSensitive { get; set; } = false;
    public bool IsRecursive { get; set; } = true;
    public bool IsAutoSelectIcon { get; set; } = true;
    public int MaxRecursive { get; set; } = 2;
}