namespace Foldicon.Struct;

public record Percentage
{
    public required int Now { get; set; }
    public required int Max { get; set; }
    public double Value => Now / (double)Max;
};