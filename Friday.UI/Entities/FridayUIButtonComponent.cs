using DSharpPlus;
using DSharpPlus.Entities;

namespace Friday.UI.Entities;

public abstract class FridayUIButtonComponent : FridayUIComponent
{
    internal abstract Task<bool> OnClick(DiscordInteraction interaction);

    protected FridayUIButtonComponent(FridayUIPage page) : base(page)
    {
        
    }
}