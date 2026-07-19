using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Struct;

public readonly record struct FileIcon(string FullPath, BitmapImage Icon)
{
    public string FullPath { get; } = FullPath;
    public BitmapImage Icon { get; } = Icon;
}