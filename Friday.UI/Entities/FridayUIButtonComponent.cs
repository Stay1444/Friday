using DSharpPlus;
using DSharpPlus.Entities;

namespace Friday.UI.Entities;

public abstract class FridayUIButtonComponent : FridayUIComponent
{
    internal abstract Task OnClick(DiscordInteraction interaction);

    protected FridayUIButtonComponent(FridayUIPage page) : base(page)
    {
        
    }
}