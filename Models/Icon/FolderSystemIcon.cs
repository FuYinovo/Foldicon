using System.IO;
using Foldicon.Contracts;

namespace Foldicon.Models.Icon;

public class FolderSystemIcon(byte[] bitmapBytes) : IFolderIcon
{
    public byte[] BitmapBytes { get; } = bitmapBytes;
    public string DisplayName => "系统默认";
    public string ShortDisplayName => "系统默认";

    public void ApplyTo(string folder)
    {
        var desktopIni = Path.Combine(folder, "desktop.ini");
        if (File.Exists(desktopIni)) File.Delete(desktopIni);
    }

    public void Save(string folder)
    {
    }

    public void Remove(string folder)
    {
    }

    public bool Equals(IFolderIcon icon)
    {
        return icon is FolderSystemIcon;
    }
}