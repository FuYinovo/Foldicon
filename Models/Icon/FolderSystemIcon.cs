using System.IO;
using Foldicon.Contracts;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Models.Icon;

public class FolderSystemIcon(BitmapImage bitmap) : IFolderIcon
{
    public BitmapImage Bitmap { get; } = bitmap;
    public string DisplayName { get; } = "系统默认";
    public string ShortDisplayName { get; } = "系统默认";

    public void ApplyTo(string folder)
    {
        var desktopIni = Path.Combine(folder, "desktop.ini");
        if (File.Exists(desktopIni)) File.Delete(desktopIni);
    }

    public void SaveAt(string folder)
    {
    }

    public void RemoveAt(string folder)
    {
    }

    public bool Equals(IFolderIcon icon)
    {
        return icon is FolderSystemIcon;
    }
}