namespace Foldicon.Xaml.Dialog;

public sealed partial class DescriptionDialog
{
    public readonly string Description;

    public DescriptionDialog(string description)
    {
        Description = description;
        InitializeComponent();
    }
}