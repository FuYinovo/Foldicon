using System.IO;
using Foldicon.Contracts;
using Foldicon.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Models.Icon;

public class FolderDllIcon(BitmapImage bitmap, string dllPath, int dllIndex) : IFolderIcon
{
    public BitmapImage Bitmap { get; } = bitmap;
    public string DisplayName { get; } = $"{dllPath} - [{dllIndex}]";
    public string ShortDisplayName { get; } = $"{Path.GetFileName(dllPath)} - [{dllIndex}]";
    private readonly string _dllPath = dllPath;
    private readonly int _dllIndex = dllIndex;

    public void ApplyTo(string folder)
    {
        IconHelper.SetFolderIcon(folder, _dllPath, _dllIndex);
    }

    public void SaveAt(string folder)
    {
    }

    public void RemoveAt(string folder)
    {
    }

    public bool Equals(IFolderIcon icon)
    {
        return icon is FolderDllIcon dllIcon &&
               dllIcon._dllIndex == _dllIndex &&
               dllIcon._dllPath == _dllPath;
    }
}