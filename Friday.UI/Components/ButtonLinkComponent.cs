using DSharpPlus;
using DSharpPlus.Entities;
using Friday.UI.Entities;

namespace Friday.UI.Components;

public class ButtonLinkComponent : FridayUIComponent
{
    public string Url { get; set; }
    public string? Label { get; set; }
    public bool Disabled { get; set; }
    public DiscordEmoji? Emoji { get; set; }
    
    internal ButtonLinkComponent(FridayUIPage page) : base(page)
    {
        this.Url = "www.example.com";
        this.Label = "Example";
    }

    internal override DiscordComponent? GetDiscordComponent()
    {
        return new DiscordLinkButtonComponent(Url, Label, Disabled,
            Emoji == null ? null : new DiscordComponentEmoji(Emoji));
    }
}