using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.Record;

public record IconGroupLogo
{
    public string Path { get; init; } = string.Empty;
    public BitmapImage Bitmap { get; init; } = new();
}