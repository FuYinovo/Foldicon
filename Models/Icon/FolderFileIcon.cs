using Foldicon.Contracts;
using Foldicon.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Models.Icon;

public class FolderFileIcon(BitmapImage bitmap, string filePath) : IFolderIcon
{
    public BitmapImage Bitmap { get; set; } = bitmap;
    public string DisplayName { get; set; } = filePath;
    private readonly string _filePath = filePath;

    public void ApplyTo(string folder)
    {
        IconHelper.SetFolderIcon(folder, _filePath);
    }

    public bool Equals(IFolderIcon icon)
    {
        return icon is FolderFileIcon fileIcon && fileIcon._filePath == _filePath;
    }
}