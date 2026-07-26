using System.IO;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Struct;

public record struct BitmapIcon()
{
    public string? FullPath { get; set; }
    public BitmapImage Icon { get; set; } = new();
    public string? FileName => Path.GetFileNameWithoutExtension(FullPath);
}