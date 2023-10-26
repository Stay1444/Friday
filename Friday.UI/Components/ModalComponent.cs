using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using Friday.UI.Entities;
using Serilog;

namespace Friday.UI.Components;

public class ModalComponent : FridayUIButtonComponent
{
    public string? ButtonLabel { get; set; }
    public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Primary;
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
    
    internal override Task<bool> OnClick(DiscordInteraction interaction)
    {
        if (_fields.Count > 5)
        {
            throw new Exception("Modal can only have up to 5 fields");
        }
        
        _ = Task.Run(async () =>
        {
            try
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
                else
                {
                    this._onSubmit?.Invoke(result.Result.Values);
                }
            }
            catch (BadRequestException exception)
            {
                Log.Error(exception.Message + exception.JsonMessage + exception.Errors + " Bad request exception");
            }
            catch (Exception exception)
            {
                Log.Error("Error creating modal {exception}", exception);
            }
        });
        
        return Task.FromResult(true);
    }

    internal override DiscordComponent? GetDiscordComponent()
    {
        return new DiscordButtonComponent(ButtonStyle, Id, ButtonLabel ?? "", ButtonDisabled, ButtonEmoji is null ? null : new DiscordComponentEmoji(ButtonEmoji));
    }

    internal ModalComponent(FridayUIPage page) : base(page)
    {
        
    }
}