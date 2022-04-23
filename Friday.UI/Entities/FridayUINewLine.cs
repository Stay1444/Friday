using DSharpPlus.Entities;

namespace Friday.UI.Entities;

public class FridayUINewLine : FridayUIComponent
{
    public FridayUINewLine(FridayUIPage page) : base(page)
    {
    }

    internal override DiscordComponent? GetDiscordComponent()
    {
        return null;
    }
}