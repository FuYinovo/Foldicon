using System;
using System.IO;
using Foldicon.Contracts;
using Foldicon.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Models.Icon;

public class FolderFileIcon : IFolderIcon
{
    public byte[]? BitmapBytes { get; }

    public BitmapImage Bitmap
    {
        get
        {
            if (field != null) return field;
            field = IconHelper.CreateBitmap(BitmapBytes ?? throw new Exception("byte[] BitmapBytes is null"));
            return field;
        }
        set;
    }

    public string DisplayName { get; }
    public string ShortDisplayName { get; }
    public string FilePath { get; }

    public FolderFileIcon(byte[] bitmapBytes, string filePath)
    {
        BitmapBytes = bitmapBytes;
        DisplayName = filePath;
        ShortDisplayName = Path.GetFileName(filePath);
        FilePath = filePath;
    }

    public FolderFileIcon(string filePath)
    {
        Bitmap = new BitmapImage(new Uri(filePath));
        DisplayName = filePath;
        ShortDisplayName = Path.GetFileName(filePath);
        FilePath = filePath;
    }

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