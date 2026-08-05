using Foldicon.Contracts;
using Foldicon.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Models.Icon;

public class FolderDllIcon(BitmapImage bitmap, string dllPath, int dllIndex) : IFolderIcon
{
    public BitmapImage Bitmap { get; set; } = bitmap;
    public string DisplayName { get; set; } = $"{dllPath} - [{dllIndex}]";
    private readonly string _dllPath = dllPath;
    private readonly int _dllIndex = dllIndex;

    public void ApplyTo(string folder)
    {
        IconHelper.SetFolderIcon(folder, _dllPath, _dllIndex);
    }

    public bool Equals(IFolderIcon icon)
    {
        return icon is FolderDllIcon dllIcon &&
               dllIcon._dllIndex == _dllIndex &&
               dllIcon._dllPath == _dllPath;
    }
}