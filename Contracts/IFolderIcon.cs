using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Contracts;

public interface IFolderIcon
{
    BitmapImage Bitmap { get; }
    string DisplayName { get; }
    void ApplyTo(string folder);
    bool Equals(IFolderIcon icon);
}