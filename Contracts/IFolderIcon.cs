using Foldicon.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Contracts;

public interface IFolderIcon
{
    byte[] BitmapBytes { get; }
    public BitmapImage Bitmap => IconHelper.GetBitmapTemp(this);

    string DisplayName { get; }
    string ShortDisplayName { get; }
    void ApplyTo(string folder);
    void Save(string folder);
    void Remove(string folder);
    bool Equals(IFolderIcon icon);
}