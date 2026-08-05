using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Contracts;

public interface IFolderIcon
{
    BitmapImage Bitmap { get; set; }
    string DisplayName { get; set; }
    void ApplyTo(string folder);
    bool Equals(IFolderIcon icon);
}