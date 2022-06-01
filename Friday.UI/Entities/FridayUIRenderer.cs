using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Friday.Common;

namespace Friday.UI.Entities;

internal class FridayUIRenderer
{
    private readonly FridayUIBuilder _builder;
    internal FridayUIRenderer(FridayUIBuilder builder)
    {
        this._builder = builder;
    }

    private FridayUIPage GetActivePage(FridayUIPage page)
    {
        if (page.SubPage is null) return page;
        
        return GetActivePage(page.SubPages[page.SubPage]);
    }

    private async Task<DiscordMessageBuilder> PrepareRender(DiscordClient client)
    {
        await _builder.Render(client);
        var page = GetActivePage(_builder.Page!);
        var dComponents = new List<DiscordComponent>();
        var msgBuilder = page.UsedMessageBuilder ? page.Message : new DiscordMessageBuilder();
        
        foreach (var component in page.Components)
        {
            if (component is FridayUINewLine)
            {
                if (dComponents.Count > 0)
                {
                    msgBuilder.AddComponents(dComponents);
                }

                dComponents.Clear();
                continue;
            }
            var componentResult = component.GetDiscordComponent();
            if (componentResult is null)
            {
                continue;
            }

            if (componentResult is DiscordButtonComponent buttonComponent)
            {
                dComponents.Add(buttonComponent);
            }

            if (componentResult is DiscordSelectComponent selectComponent)
            {
                dComponents.Add(selectComponent);
            }
        }

        if (!page.UsedMessageBuilder)
        {
            msgBuilder.AddEmbed(page.Embed);
        }

        if (dComponents.Count > 0)
        {
            msgBuilder.AddComponents(dComponents);
        }

        return msgBuilder;
    }

    private FridayUIButtonComponent? GetButtonComponent(string id)
    {
        var page = GetActivePage(_builder.Page!);
        foreach (var component in page.Components)
        {
            if (component is FridayUIButtonComponent buttonComponent)
            {
                if (buttonComponent.Id == id)
                {
                    return buttonComponent;
                }
            }
        }
        
        return null;
    }

    private FridayUISelectComponent? GetSelectComponent(string id)
    {
        var page = GetActivePage(_builder.Page!);
        foreach (var component in page.Components)
        {
            if (component is FridayUISelectComponent selectComponent)
            {
                if (selectComponent.Id == id)
                {
                    return selectComponent;
                }
            }
        }
        
        return null;
    }
    
    private bool HasButtons(DiscordMessageBuilder builder)
    {
        foreach (var discordActionRowComponent in builder.Components)
        {
            foreach (var discordComponent in discordActionRowComponent.Components)
            {
                if (discordComponent is DiscordButtonComponent)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private bool HasSelects(DiscordMessageBuilder builder)
    {
        foreach (var discordActionRowComponent in builder.Components)
        {
            foreach (var discordComponent in discordActionRowComponent.Components)
            {
                if (discordComponent is DiscordSelectComponent)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    internal async Task RenderAsync(DiscordClient client, DiscordChannel channel, DiscordUser user)
    {
        DiscordMessage? message = null;
        DiscordInteraction? interaction = null;
        while (true)
        {
            try
            {
                var msgBuilder = await PrepareRender(client);
                if (message is null)
                {
                    message = await channel.SendMessageAsync(msgBuilder);
                }
                else
                {
                    if (interaction is null)
                    {
                        message = await message.ModifyAsync(msgBuilder);
                    }
                    else
                    {
                        try
                        {
                            await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                                msgBuilder.ToInteractionResponseBuilder());
                        
                            message = await interaction.GetOriginalResponseAsync();
                            interaction = null;
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException)
                        {
                            message = await message.ModifyAsync(msgBuilder);
                        }
                    }
                }

                var interactivity = client.GetInteractivity();
                var cancellationTokenSource = _builder.CancellationTokenSource;
                Task<InteractivityResult<ComponentInteractionCreateEventArgs>>? buttonTask = null;
                Task<InteractivityResult<ComponentInteractionCreateEventArgs>>? selectTask = null;
                if (HasButtons(msgBuilder))
                {
                    buttonTask = interactivity.WaitForButtonAsync(message, user, cancellationTokenSource.Token);
                }
                
                if (HasSelects(msgBuilder))
                {
                    selectTask = interactivity.WaitForSelectAsync(message, x => x.User.Id == user.Id, cancellationTokenSource.Token);
                }
                
                if (buttonTask is null && selectTask is null)
                {
                    break;
                }

                if (buttonTask is not null && selectTask is not null)
                {
                    await Task.WhenAny(buttonTask, selectTask);
                }else if (buttonTask is not null)
                {
                    await buttonTask;
                }else if (selectTask is not null)
                {
                    await selectTask;
                }
                
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    if (_builder.RenderRequested)
                    {
                        _builder.ResetToken();
                        continue;
                    }
                
                    if (_builder.Page!.EventOnCancelled is not null)
                    {
                        _builder.Page!.EventOnCancelled.Invoke(client, message);
                    }else if (_builder.Page!.EventOnCancelledAsync is not null)
                    {
                        await _builder.Page!.EventOnCancelledAsync.Invoke(client, message);
                    }
                    return;
                }
                
                _builder.ResetToken();

                if (buttonTask is not null && buttonTask.IsCompleted)
                {
                    var buttonResult = await buttonTask;

                    var interactionId = buttonResult.Result.Id;
                
                    var buttonComponent = GetButtonComponent(interactionId);
                
                    if (buttonComponent is not null)
                    {
                        var handled = await buttonComponent.OnClick(buttonResult.Result.Interaction);
                        if (!handled) interaction = buttonResult.Result.Interaction;
                    }
                }

                if (selectTask is not null && selectTask.IsCompleted)
                {
                    var selectResult = await selectTask;
                    
                    var interactionId = selectResult.Result.Id;
                
                    var selectComponent = GetSelectComponent(interactionId);
                    
                    if (selectComponent is not null)
                    {
                        var handled = await selectComponent.OnSelect(selectResult.Result.Interaction, selectResult.Result.Values);
                        if (!handled) interaction = selectResult.Result.Interaction;
                    }
                }

                if (_builder.CancellationTokenSource.IsCancellationRequested)
                {
                    if (_builder.Page!.EventOnCancelled is not null)
                    {
                        _builder.Page!.EventOnCancelled.Invoke(client, message);
                    }else if (_builder.Page!.EventOnCancelledAsync is not null)
                    {
                        await _builder.Page!.EventOnCancelledAsync.Invoke(client, message);
                    }
                    else
                    {
                        await message.DeleteAsync();
                    }
                    return;
                }
            }catch(Exception e)
            {
                if (message is not null)
                {
                    if (e is BadRequestException badRequestException)
                    {
                        await message.ModifyAsync(new DiscordMessageBuilder()
                            .WithEmbed(new DiscordEmbedBuilder()
                                .Transparent()
                                .WithTitle("FridayUI Error")
                                .WithDescription($"An error occured while rendering the page.\n```json\n{badRequestException.Errors.MaxLength(1024)}```\nPlease contact the developer.")
                                .WithColor(DiscordColor.Red)));
                    }else
                    {
                        await message.ModifyAsync(new DiscordMessageBuilder()
                            .WithEmbed(new DiscordEmbedBuilder()
                                .Transparent()
                                .WithTitle("FridayUI Error")
                                .WithDescription($"An error occured while rendering the page.\n```{e.ToString().MaxLength(1024)}```\nPlease contact the developer.")
                                .WithColor(DiscordColor.Red)));
                    }
                }else
                {
                    if (e is BadRequestException badRequestException)
                    {
                        await channel.SendMessageAsync(new DiscordMessageBuilder()
                            .WithEmbed(new DiscordEmbedBuilder()
                                .Transparent()
                                .WithTitle("FridayUI Error")
                                .WithDescription($"An error occured rendering the page.\n```json\n{badRequestException.Errors.MaxLength(1024)}```\nPlease contact the developer.")
                                .WithColor(DiscordColor.Red)));
                    }else
                    {
                        await channel.SendMessageAsync(new DiscordMessageBuilder()
                            .WithEmbed(new DiscordEmbedBuilder()
                                .Transparent()
                                .WithTitle("FridayUI Error")
                                .WithDescription($"An error occured rendering the page.\n```{e.ToString().MaxLength(1024)}```\nPlease contact the developer.")
                                .WithColor(DiscordColor.Red)));
                    }
                }

                return;
            }
        }
    }
}