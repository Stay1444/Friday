using DSharpPlus;
using DSharpPlus.Entities;
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
        var buttons = new List<DiscordButtonComponent>();
        var msgBuilder = new DiscordMessageBuilder();
        
        foreach (var component in page.Components)
        {
            if (component is FridayUINewLine)
            {
                msgBuilder.AddComponents(buttons);
                buttons.Clear();
                continue;
            }
            var componentResult = component.GetDiscordComponent();
            if (componentResult is null)
            {
                continue;
            }

            if (componentResult is DiscordButtonComponent buttonComponent)
            {
                buttons.Add(buttonComponent);
            }
        }

        msgBuilder.WithEmbed(page.Embed);
        msgBuilder.AddComponents(buttons);
        
        return msgBuilder;
    }

    internal FridayUIButtonComponent? GetButtonComponent(string id)
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

    private bool AreEqual(DiscordMessageBuilder? m1, DiscordMessageBuilder? m2)
    {
        if (m1 is null && m2 is null) return true;
        if (m1 is null || m2 is null) return false;
        
        if (m1.Embed.Title != m2.Embed.Title) return false;
        if (m1.Embed.Description != m2.Embed.Description) return false;
        if (m1.Embed.Image?.Url != m2.Embed.Image?.Url) return false;
        if (m1.Embed.Thumbnail?.Url != m2.Embed.Thumbnail?.Url) return false;
        
        if (m1.Embed.Color.HasValue && m2.Embed.Color.HasValue)
        {
            if (m1.Embed.Color.Value.Value != m2.Embed.Color.Value.Value) return false;
        }
        else if (m1.Embed.Color.HasValue || m2.Embed.Color.HasValue)
        {
            return false;
        }
        
        if (m1.Embed.Footer != null && m2.Embed.Footer != null)
        {
            if (m1.Embed.Footer.Text != m2.Embed.Footer.Text) return false;
            if (m1.Embed.Footer.IconUrl != m2.Embed.Footer.IconUrl) return false;
        }
        else if (m1.Embed.Footer != null || m2.Embed.Footer != null)
        {
            return false;
        }
        
        if (m1.Embed.Author != null && m2.Embed.Author != null)
        {
            if (m1.Embed.Author.Name != m2.Embed.Author.Name) return false;
            if (m1.Embed.Author.Url != m2.Embed.Author.Url) return false;
            if (m1.Embed.Author.IconUrl != m2.Embed.Author.IconUrl) return false;
        }
        else if (m1.Embed.Author != null || m2.Embed.Author != null)
        {
            return false;
        }
        
        if (m1.Embed.Timestamp != null && m2.Embed.Timestamp != null)
        {
            if (m1.Embed.Timestamp.Value.ToUnixTimeMilliseconds() != m2.Embed.Timestamp.Value.ToUnixTimeMilliseconds()) return false;
        }
        else if (m1.Embed.Timestamp != null || m2.Embed.Timestamp != null)
        {
            return false;
        }
        
        if (m1.Embed.Fields.Count != m2.Embed.Fields.Count) return false;
        for (var i = 0; i < m1.Embed.Fields.Count; i++)
        {
            if (m1.Embed.Fields[i].Name != m2.Embed.Fields[i].Name) return false;
            if (m1.Embed.Fields[i].Value != m2.Embed.Fields[i].Value) return false;
            if (m1.Embed.Fields[i].Inline != m2.Embed.Fields[i].Inline) return false;
        }
        
        if (m1.Components.Count != m2.Components.Count) return false;
        
        for (var i = 0; i < m1.Components.Count; i++)
        {
            if (m1.Components[i].Components.Count != m2.Components[i].Components.Count) return false;
            
            for (var c = 0; c < m1.Components[i].Components.Count; c++)
            {
                // ReSharper disable once GenericEnumeratorNotDisposed
                var m1Enumerator = m1.Components[i].Components.GetEnumerator();
                // ReSharper disable once GenericEnumeratorNotDisposed
                var m2Enumerator = m2.Components[i].Components.GetEnumerator();

                while (m1Enumerator.MoveNext() && m2Enumerator.MoveNext())
                {
                    var m1Component = m1Enumerator.Current;
                    var m2Component = m2Enumerator.Current;

                    if (m1Component is DiscordButtonComponent buttonComponent1)
                    {
                        if (!(m2Component is DiscordButtonComponent buttonComponent2)) return false;
                        if (buttonComponent1.Label != buttonComponent2.Label) return false;
                        if (buttonComponent1.Style != buttonComponent2.Style) return false;
                        if (buttonComponent1.Emoji != buttonComponent2.Emoji) return false;
                        if (buttonComponent1.Disabled != buttonComponent2.Disabled) return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }

    private Dictionary<string, string> GetComponentsIdMapping(DiscordMessageBuilder old, DiscordMessageBuilder @new)
    {
        var mapping = new Dictionary<string, string>();

        for (int oldI = 0; oldI < old.Components.Count; oldI++)
        {
            var oldRow = old.Components[oldI].Components.GetEnumerator().ToList();
            var newRow = @new.Components[oldI].Components.GetEnumerator().ToList();
            
            for (int componentIndex = 0; componentIndex < oldRow.Count; componentIndex++)
            {
                var oldComponent = oldRow[componentIndex];
                var newComponent = newRow[componentIndex];
                
                mapping.Add(oldComponent.CustomId, newComponent.CustomId);
            }
        }
        
        return mapping;
    }

    internal async Task RenderAsync(DiscordClient client, DiscordChannel channel, DiscordUser user)
    {
        DiscordMessage? message = null;
        DiscordMessageBuilder? lastBuilder = null;
        while (true)
        {
            try
            {
                var msgBuilder = await PrepareRender(client);
                Dictionary<string, string>? idMapping = null;
                if (message is null)
                {
                    message = await channel.SendMessageAsync(msgBuilder);
                    lastBuilder = msgBuilder;
                }
                else
                {
                    if (!AreEqual(lastBuilder, msgBuilder))
                    {
                        message = await message.ModifyAsync(msgBuilder);
                    }
                    else
                    {
                        idMapping = GetComponentsIdMapping(lastBuilder!, msgBuilder);
                    }
                }

                var interactivity = client.GetInteractivity();
                var cancellationTokenSource = _builder.CancellationTokenSource;
                var buttonTask = interactivity.WaitForButtonAsync(message, user, cancellationTokenSource.Token);
                await Task.WhenAny(buttonTask);
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
                else
                {
                    _builder.ResetToken();
                }

                if (buttonTask.IsCompleted)
                {
                    var buttonResult = await buttonTask;

                    var interactionId = buttonResult.Result.Id;
                
                    if (idMapping is not null)
                    {
                        if (idMapping.TryGetValue(interactionId, out var newId))
                        {
                            interactionId = newId;
                        }
                        else
                        {
                            lastBuilder = null;
                            continue;
                        }
                    }
                
                    var buttonComponent = GetButtonComponent(interactionId);
                
                    if (buttonComponent is not null)
                    {
                        await buttonComponent.OnClick(buttonResult.Result.Interaction);
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
                    return;
                }
            }catch(Exception e)
            {
                if (message is not null)
                {
                    await message.ModifyAsync(new DiscordMessageBuilder()
                        .WithEmbed(new DiscordEmbedBuilder()
                            .Transparent()
                            .WithTitle("FridayUI Error")
                            .WithDescription($"An error occured while rendering the page.\n```{e.Message}```\nPlease contact the developer.")
                            .WithColor(DiscordColor.Red)));
                }

                return;
            }
        }
    }
}