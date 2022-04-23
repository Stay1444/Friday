using DSharpPlus;

namespace Friday.UI.Entities;

public class ModalComponentField
{
    public string Title { get; set; }
    public string? Value { get; set; }
    public string? Placeholder { get; set; }
    public bool Required { get; set; }
    public TextInputStyle Style { get; set; } = TextInputStyle.Short;
    public int MaximumLength { get; set; } = 4000;
    public int MinimumLength { get; set; }
    public bool Disabled { get; set; }
    internal ModalComponentField()
    {
        this.Title = "Field";   
    }
}