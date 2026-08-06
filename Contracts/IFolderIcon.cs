using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Contracts;

public interface IFolderIcon
{
    BitmapImage Bitmap { get; }
    string DisplayName { get; }
    string ShortDisplayName { get; }
    void ApplyTo(string folder);
    void Save(string folder);
    void Remove(string folder);
    bool Equals(IFolderIcon icon);
}