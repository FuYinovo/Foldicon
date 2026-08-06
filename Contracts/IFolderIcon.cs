using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Contracts;

public interface IFolderIcon
{
    BitmapImage Bitmap { get; }
    string DisplayName { get; }
    string ShortDisplayName { get; }
    void ApplyTo(string folder);
    void SaveAt(string folder);
    void RemoveAt(string folder);
    bool Equals(IFolderIcon icon);
}