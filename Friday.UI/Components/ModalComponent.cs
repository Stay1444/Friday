using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Friday.UI.Entities;

namespace Friday.UI.Components;

public class ModalComponent : FridayUIButtonComponent
{
    public string? ButtonLabel { get; set; }
    public ButtonStyle ButtonStyle { get; set; }
    public bool ButtonDisabled { get; set; } 
    public DiscordEmoji? ButtonEmoji { get; set; }
    
    public string Title { get; set; } = "Friday UI Modal";

    private Dictionary<string, ModalComponentField> _fields = new ();
    
    public void AddField(string id, Action<ModalComponentField> modify)
    {
        var field = new ModalComponentField();
        modify(field);
        this._fields.Add(id, field);
    }
    
    private Func<IReadOnlyDictionary<string,string>, Task>? _onSubmitAsync;
    private Action<IReadOnlyDictionary<string,string>>? _onSubmit;


    public void OnSubmit(Action<IReadOnlyDictionary<string, string>> onSubmit)
    {
        this._onSubmit = onSubmit;
    }

    public void OnSubmit(Func<IReadOnlyDictionary<string,string>, Task> onSubmit)
    {
        this._onSubmitAsync = onSubmit;
    }
    
    internal override Task OnClick(DiscordInteraction interaction)
    {
        _ = Task.Run(async () =>
        {
            var builder = new DiscordInteractionResponseBuilder();
            builder.WithCustomId(Id);
            builder.WithTitle(Title);

            foreach (var field in _fields)
            {
                builder.AddComponents(new TextInputComponent(field.Value.Title, field.Key, field.Value.Placeholder,
                    field.Value.Value, field.Value.Required, field.Value.Style, field.Value.MinimumLength,
                    field.Value.MaximumLength));
            }

            await interaction.CreateResponseAsync(InteractionResponseType.Modal, builder);

            var result = await Page.Client.GetInteractivity().WaitForModalAsync(Id, TimeSpan.FromSeconds(60));

            if (result.TimedOut) return;
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            if (this._onSubmitAsync is not null)
            {
                await this._onSubmitAsync(result.Result.Values);
            }
            else if (this._onSubmit != null)
            {
                this._onSubmit(result.Result.Values);
            }
        });
        
        return Task.CompletedTask;
    }

    internal override DiscordComponent? GetDiscordComponent()
    {
        return new DiscordButtonComponent(ButtonStyle, Id, ButtonLabel, ButtonDisabled);
    }

    public ModalComponent(FridayUIPage page) : base(page)
    {
        
    }
}