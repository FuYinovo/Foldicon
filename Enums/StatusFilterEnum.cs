using System.ComponentModel;

namespace Foldicon.Enums;

public enum StatusFilterEnum
{
    [Description("全部")] All,
    [Description("未修改")] Unmodified,
    [Description("待应用")] Unapplied
}