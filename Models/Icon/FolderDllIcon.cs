using System.IO;
using Foldicon.Contracts;
using Foldicon.Helpers;

namespace Foldicon.Models.Icon;

public class FolderDllIcon(byte[] bitmapBytes, string dllPath, int dllIndex) : IFolderIcon
{
    public byte[] BitmapBytes { get; } = bitmapBytes;
    public string DisplayName { get; } = $"{dllPath} - [{dllIndex}]";
    public string ShortDisplayName { get; } = $"{Path.GetFileName(dllPath)} - [{dllIndex}]";
    private readonly string _dllPath = dllPath;
    private readonly int _dllIndex = dllIndex;

    public void ApplyTo(string folder)
    {
        IconHelper.SetFolderIcon(folder, _dllPath, _dllIndex);
    }

    public void Save(string folder)
    {
    }

    public void Remove(string folder)
    {
    }


    public bool Equals(IFolderIcon icon)
    {
        return icon is FolderDllIcon dllIcon &&
               dllIcon._dllIndex == _dllIndex &&
               dllIcon._dllPath == _dllPath;
    }
}