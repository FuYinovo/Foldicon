using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Struct;

public readonly struct FileIcon(string fullPath, BitmapImage icon)
{
    public readonly string FullPath { get; } = fullPath;
    public readonly BitmapImage Icon { get; } = icon;
}