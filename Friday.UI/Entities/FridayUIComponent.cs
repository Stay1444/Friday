using DSharpPlus.Entities;

namespace Friday.UI.Entities;

public abstract class FridayUIComponent
{
    internal FridayUIPage Page { get; }
    internal FridayUIComponent(FridayUIPage page)
    {
        this.Page = page;
    }
    
    internal string Id { get; } = Guid.NewGuid().ToString();

    internal abstract DiscordComponent? GetDiscordComponent();
    internal virtual FridayUIBuilder? GetPage() => null;
}