using DSharpPlus;
using DSharpPlus.Entities;
using Friday.UI.Entities;

namespace Friday.UI.Components;

public class ButtonComponent : FridayUIButtonComponent
{
    private Func<Task>? _asyncClickAction;
    private Action? _clickAction;
    
    public string? Label { get; set; }
    public ButtonStyle Style { get; set; } = ButtonStyle.Secondary;
    public bool Disabled { get; set; } 
    public DiscordEmoji? Emoji { get; set; }


    public void OnClick(Func<Task> action)
    {
        this._asyncClickAction = action;
    }
    
    public void OnClick(Action action)
    {
        this._clickAction = action;
    }
    
    internal override async Task OnClick(DiscordInteraction interaction)
    {
        await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

        if (this._asyncClickAction is not null)
        {
            await this._asyncClickAction.Invoke();
        }else if (this._clickAction is not null)
        {
            this._clickAction.Invoke();
        }
    }

    internal override DiscordComponent? GetDiscordComponent()
    {
        return new DiscordButtonComponent(Style, Id, Label, Disabled);
    }

    public ButtonComponent(FridayUIPage page) : base(page)
    {
    }
}