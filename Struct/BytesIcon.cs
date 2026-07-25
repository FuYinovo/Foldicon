namespace Foldicon.Struct;

public record BytesIcon()
{
    public string FullPath { get; set; } = string.Empty;
    public byte[]? Icon { get; set; } = [];
}