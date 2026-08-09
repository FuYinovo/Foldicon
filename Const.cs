using Vanara.PInvoke;

namespace Foldicon;

public abstract record Const
{
    /// <summary> 文件夹图标尺寸 </summary>
    public const Shell32.SHIL FolderIconSize = Shell32.SHIL.SHIL_LARGE;
    /// <summary> 图标组图标尺寸 </summary>
    public const Shell32.SHIL GroupIconSize = Shell32.SHIL.SHIL_EXTRALARGE;

};