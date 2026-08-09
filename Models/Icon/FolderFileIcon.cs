using System.IO;
using Foldicon.Contracts;
using Foldicon.Helpers;

namespace Foldicon.Models.Icon;

public class FolderFileIcon(byte[] bitmapBytes, string filePath) : IFolderIcon
{
    public byte[] BitmapBytes { get; } = bitmapBytes;
    public string DisplayName { get; } = filePath;
    public string ShortDisplayName { get; } = Path.GetFileName(filePath);
    public string FilePath { get; } = filePath;

    public void ApplyTo(string folder)
    {
        IconHelper.SetFolderIcon(folder, FilePath);
    }

    public void Save(string folder)
    {
        var target = Path.Combine(folder, Path.GetFileName(FilePath));
        if (!File.Exists(FilePath)) return;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        if (File.Exists(target)) File.Delete(target);
        File.Copy(FilePath, target);
    }

    public void Remove(string folder)
    {
        var fileName = Path.Combine(folder, Path.GetFileName(FilePath));
        if (File.Exists(fileName)) File.Delete(fileName);
    }

    public bool Equals(IFolderIcon icon)
    {
        return icon is FolderFileIcon fileIcon && fileIcon.FilePath == FilePath;
    }
}