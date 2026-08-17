namespace Foldicon.Record;

public record LocalizationItem<TObj>
{
    public required string Localization { get; set; }
    public required TObj Item { get; set; }
}