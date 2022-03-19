using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using Friday.Common;
using Friday.Common.Services;

namespace Friday.Modules.EmbedCreator;

internal class EmbedCreator
{
    private string _language = "en";
    private DiscordUser _user;
    private DiscordChannel _channel;
    private LanguageProvider _languageProvider;
    private DiscordClient _client;
    private DiscordMessage? _embedMessage;
    private string? _embedTitle = "default";
    private string? _embedDescription = "default";
    private string? _embedThumbnailUrl;
    private string? _embedImageUrl;
    private string? _embedAuthorName = "default";
    private string? _embedAuthorUrl;
    private string? _embedAuthorIconUrl;
    private string? _embedFooterText = "default";
    private string? _embedFooterIconUrl;
    private string? _embedColor;
    private string? _embedTimestamp;
    private List<(string, string, bool)> _embedFields = new();
    private readonly TaskCompletionSource<DiscordEmbedBuilder?> _readyToSend;
    private System.Timers.Timer _timeoutTimer = new(5000);
    private DateTime _lastInteraction;
    public EmbedCreator(string language, DiscordUser controller, DiscordChannel channel, LanguageProvider languageProvider, DiscordClient client)
    {
        this._language = language;
        this._user = controller;
        this._channel = channel;
        _languageProvider = languageProvider;
        _client = client;
        _readyToSend = new TaskCompletionSource<DiscordEmbedBuilder?>();
    }

    private DiscordEmbedBuilder BuildEmbed()
    {
        var deBuilder = new DiscordEmbedBuilder();
        
        if (_embedTitle != null)
            deBuilder.WithTitle(_embedTitle);
        
        if (_embedDescription != null)
            deBuilder.WithDescription(_embedDescription);
        
        if (_embedThumbnailUrl != null)
            deBuilder.WithThumbnail(_embedThumbnailUrl);
        
        if (_embedImageUrl != null)
            deBuilder.WithImageUrl(_embedImageUrl);
        
        if (_embedAuthorName != null)
            deBuilder.WithAuthor(_embedAuthorName, _embedAuthorUrl, _embedAuthorIconUrl);
        
        if (_embedFooterText != null)
            deBuilder.WithFooter(_embedFooterText, _embedFooterIconUrl);
        
        if (_embedColor != null)
            deBuilder.WithColor(new DiscordColor(_embedColor));
        
        if (_embedTimestamp != null)
            deBuilder.WithTimestamp(DateTime.Parse(_embedTimestamp));
        
        if (_embedFields.Count > 0)
        {
            foreach (var (key, value, inline) in _embedFields)
            {
                deBuilder.AddField(key, value, inline);
            }
        }
        
        return deBuilder;
    }

    private DiscordMessageBuilder BuildMsgBuilder()
    {
        var discordMessageBuilder = new DiscordMessageBuilder();
        discordMessageBuilder.WithEmbed(BuildEmbed());

        discordMessageBuilder.AddComponents(
            new DiscordButtonComponent(ButtonStyle.Primary, "editBasic", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":pencil:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "editFields", "Fields", _fieldsEditMessage is not null, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":books:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "editImg", "Image", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":camera:")))
        );
        
        discordMessageBuilder.AddComponents(
            new DiscordButtonComponent(ButtonStyle.Secondary, "editFooter", "Footer", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":athletic_shoe:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "editAuthor", "Author", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":pushpin:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "editDate", "Date", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":calendar:")))
        );
        
        discordMessageBuilder.AddComponents(
            new DiscordButtonComponent(ButtonStyle.Danger, "reset", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":arrows_counterclockwise:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "void", "", true, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(_client, 952937632600576111))),
            new DiscordButtonComponent(ButtonStyle.Success, "done", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":envelope_with_arrow:")))
        );
        
        return discordMessageBuilder;
    }
    
    public async Task<DiscordEmbedBuilder?> RunAsync()
    {
        _lastInteraction = DateTime.Now;
        _timeoutTimer.Start();
        _timeoutTimer.AutoReset = true;
        _timeoutTimer.Elapsed += TimeoutTimerOnElapsed;
        _client.ComponentInteractionCreated += OnComponentInteractionCreated;
        _client.MessageDeleted += OnMessageDeleted;
        
        _embedMessage = await _channel.SendMessageAsync(BuildMsgBuilder());
        
        return await _readyToSend.Task;
    }

    private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        if (e.Message.Id == _embedMessage?.Id)
        {
            return Stop(failed: true);
        }
        
        return Task.CompletedTask;
    }

    private void TimeoutTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_lastInteraction.AddMinutes(5) < DateTime.Now)
        {
            _ = Stop(failed: true);
        }
    }

    private Task OnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (e.Message.Id != _embedMessage?.Id)
            return Task.CompletedTask;
        
        if (e.User.Id != _user.Id)
            return Task.CompletedTask;

        if (e.Interaction.Data.CustomId == "editBasic")
        {
            _ = HandleBasicEdit(e);
        }else if (e.Interaction.Data.CustomId == "editFields")
        {
            _ = HandleFieldsEdit(e);
        }else if (e.Interaction.Data.CustomId == "editImg")
        {
            _ = HandleImgEdit(e);
        }else if (e.Interaction.Data.CustomId == "editFooter")
        {
            _ = HandleFooterEdit(e);
        }else if (e.Interaction.Data.CustomId == "editAuthor")
        {
            _ = HandleAuthorEdit(e);
        }else if (e.Interaction.Data.CustomId == "editDate")
        {
            _ = HandleDateEdit(e);
        }else if (e.Interaction.Data.CustomId == "reset")
        {
            _ = HandleReset(e);
        }else if (e.Interaction.Data.CustomId == "done")
        {
            _ = HandleDone(e);
        }
        
        return Task.CompletedTask; 
    }

    private async Task HandleBasicEdit(ComponentInteractionCreateEventArgs e)
    {
        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.WithTitle("Embed Edit - Basic");
        responseBuilder.WithCustomId("embedCreator_editBasic");
        responseBuilder.AddComponents(
            new TextInputComponent("Title", "title", "Embed Title", _embedTitle ?? "", false, TextInputStyle.Short, 0, 256)
        );
        
        responseBuilder.AddComponents(
            new TextInputComponent("Description", "description", "Embed Description", _embedDescription ?? "", false, TextInputStyle.Paragraph, 0, 4000)
        );
        
        responseBuilder.AddComponents(
            new TextInputComponent("Color", "color", "Embed Color", _embedColor ?? "#34BAEB", false, TextInputStyle.Short, 0, 7)
        );

        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
        var result = await _client.GetInteractivity()
            .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id
            && x.Interaction.Data.CustomId == "embedCreator_editBasic", TimeSpan.FromMinutes(5));

        if (result.TimedOut)
        {
            return;
        }

        await result.Ack();
        string? title = result.Result.Values.GetValueOrDefault("title");
        string? description = result.Result.Values.GetValueOrDefault("description");
        string? color = result.Result.Values.GetValueOrDefault("color");
        
        _embedColor = color;
        _embedDescription = description;
        _embedTitle = title;

        await UpdateMessage();
    }

    
    private DiscordMessage? _fieldsEditMessage;
    private async Task HandleFieldsEdit(ComponentInteractionCreateEventArgs e)
    {
        if (_fieldsEditMessage is not null) return;
        await e.Ack();
        var discordMessageBuilder = new DiscordMessageBuilder();
        var discordEmbedBuilder = new DiscordEmbedBuilder();
        discordEmbedBuilder.WithDescription("Field Editor").Transparent();
        discordMessageBuilder.WithEmbed(discordEmbedBuilder);

        discordMessageBuilder.AddComponents(
            new DiscordButtonComponent(ButtonStyle.Secondary, "fieldEditor_cancel", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":x:"))),
            new DiscordButtonComponent(ButtonStyle.Danger, "fieldEditor_delete", "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":no_entry_sign:"))),
            new DiscordButtonComponent(ButtonStyle.Primary, "fieldEditor_add", "Add", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":heavy_plus_sign:"))),
            new DiscordButtonComponent(ButtonStyle.Primary, "fieldEditor_edit", "Edit", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":pencil:")))
        );
        discordMessageBuilder.WithReply(_embedMessage?.Id, false, false);
        
        _fieldsEditMessage = await _channel.SendMessageAsync(discordMessageBuilder);

        while (true)
        {
            var result = await _client.GetInteractivity().WaitForButtonAsync(_fieldsEditMessage, _user, TimeSpan.FromMinutes(2));
            
            if (result.TimedOut)
            {
                await _fieldsEditMessage.DeleteAsync();
                _fieldsEditMessage = null;
                return;
            }
            
            if (result.Result.Interaction.Data.CustomId == "fieldEditor_cancel")
            {
                await _fieldsEditMessage.DeleteAsync();
                _fieldsEditMessage = null;
                return;
            }
            
            if (result.Result.Interaction.Data.CustomId == "fieldEditor_delete")
            {
                var responseBuilder = new DiscordMessageBuilder();
                var responseBuilderEmbed = new DiscordEmbedBuilder();
                responseBuilderEmbed.Transparent();
                responseBuilderEmbed.WithDescription("Select a field to delete");
                responseBuilder.WithEmbed(responseBuilderEmbed);

                responseBuilder.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Secondary, "fieldEditor_rm_cancel", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":x:")))
                );

                var options = new List<DiscordSelectComponentOption>();
                
                foreach (var field in _embedFields)
                {
                    options.Add(new DiscordSelectComponentOption(field.Item1, field.Item2.Substring(0, 100)));
                }
                
                responseBuilder.AddComponents(
                    new DiscordSelectComponent("fieldEditor_rm_select", "Select a filed to delete", options)
                );
                
                var responseMessage = await _channel.SendMessageAsync(responseBuilder);

                _ = Task.Run(async () =>
                {
                    var buttonResult = await _client.GetInteractivity()
                        .WaitForButtonAsync(responseMessage, _user, TimeSpan.FromMinutes(5));
                    await responseMessage.DeleteAsync();
                });

                _ = Task.Run(async () =>
                {
                    var selectResult = await _client.GetInteractivity()
                        .WaitForSelectAsync(responseMessage, _user, "fieldEditor_rm_select", TimeSpan.FromMinutes(5));
                    
                    if (selectResult.TimedOut)
                    {
                        await responseMessage.DeleteAsync();
                        return;
                    }
                    
                    _embedFields.RemoveAll(x => x.Item1 == selectResult.Result.Values.First());

                    await UpdateMessage();

                });

            }

            if (result.Result.Interaction.Data.CustomId == "fieldEditor_add")
            {
                var responseBuilder = new DiscordInteractionResponseBuilder();
                responseBuilder.WithTitle("Add Field").WithCustomId("fieldEditor_m_add");
                responseBuilder.AddComponents(
                    new TextInputComponent("Name", "name", "Field Name", "", true, TextInputStyle.Short, 0, 256)
                );
                responseBuilder.AddComponents(
                    new TextInputComponent("Value", "value", "Field Value", "", true, TextInputStyle.Paragraph, 0, 1024)
                );
                responseBuilder.AddComponents(
                    new TextInputComponent("Inline", "inline", "Inline (false/true)", "false", true, TextInputStyle.Short, 0, 5)
                );
                await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
                _ = Task.Run(async () =>
                {
                    var mresult = await _client.GetInteractivity()
                        .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id
                         && x.Interaction.Data.CustomId == "fieldEditor_m_add", TimeSpan.FromMinutes(5));
                    
                    if (mresult.TimedOut) return;
                    
                    string? name = mresult.Result.Values.GetValueOrDefault("name");
                    string? value = mresult.Result.Values.GetValueOrDefault("value");
                    string? inline = mresult.Result.Values.GetValueOrDefault("inline");
                    
                    if (name is null || value is null || inline is null)
                    {
                        return;
                    }
                    
                    if (!bool.TryParse(inline, out var inlineBool))
                    {
                        inlineBool = false;
                    }

                    _embedFields.Add((name,value,inlineBool));

                    await UpdateMessage();
                });
            }

            if (result.Result.Interaction.Data.CustomId == "fieldEditor_edit")
            {
                var responseBuilder = new DiscordMessageBuilder();
                var responseBuilderEmbed = new DiscordEmbedBuilder();
                responseBuilderEmbed.Transparent();
                responseBuilderEmbed.WithDescription("Select a field to edit");
                responseBuilder.WithEmbed(responseBuilderEmbed);

                responseBuilder.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Secondary, "fieldEditor_edit_cancel", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_client, ":x:")))
                );

                var options = new List<DiscordSelectComponentOption>();
                
                foreach (var field in _embedFields)
                {
                    options.Add(new DiscordSelectComponentOption(field.Item1, field.Item2.Substring(0, 100)));
                }
                
                responseBuilder.AddComponents(
                    new DiscordSelectComponent("fieldEditor_edit_select", "Select a field to edit", options)
                );
                
                var responseMessage = await _channel.SendMessageAsync(responseBuilder);

                _ = Task.Run(async () =>
                {

                    var selectResult = await _client.GetInteractivity().WaitForSelectAsync(responseMessage, _user, "fieldEditor_edit_select", TimeSpan.FromMinutes(5));

                    if (selectResult.TimedOut)
                    {
                        await responseMessage.DeleteAsync();
                        return;
                    }

                    var selectedOption = selectResult.Result.Values.First();
                    var selectedField = _embedFields.First(x => x.Item1 == selectedOption);
                    var modalBuilder = new DiscordInteractionResponseBuilder();
                    modalBuilder.WithTitle("Edit Field").WithCustomId("fieldEditor_edit_m");
                    modalBuilder.AddComponents(
                        new TextInputComponent("Name", "name", "Field Name", selectedField.Item1, true, TextInputStyle.Short, 0, 256)
                    );
                    modalBuilder.AddComponents(
                        new TextInputComponent("Value", "value", "Field Value", selectedField.Item2, true, TextInputStyle.Paragraph, 0, 1024)
                    );
                    modalBuilder.AddComponents(
                        new TextInputComponent("Inline", "inline", "Inline (true/false)", selectedField.Item3.ToString(), true, TextInputStyle.Short, 0, 6)
                    );

                    await selectResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal,
                        modalBuilder);
                    
                    var modalResult = await _client.GetInteractivity().WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == _user.Id && x.Interaction.Data.CustomId == "embedEditor_edit_m");
                    
                    if (modalResult.TimedOut)
                    {
                        await responseMessage.DeleteAsync();
                        return;
                    }
                    
                    var name = modalResult.Result.Values.GetValueOrDefault("name");
                    var value = modalResult.Result.Values.GetValueOrDefault("value");
                    var inline = modalResult.Result.Values.GetValueOrDefault("inline");
                    
                    if (name == null || value == null || inline == null)
                    {
                        await responseMessage.DeleteAsync();
                        return;
                    }
                    
                    if (!bool.TryParse(inline, out var inlineBool))
                    {
                        await responseMessage.DeleteAsync();
                        return;
                    }
                    

                    int index = _embedFields.FindIndex(x => x.Item1 == selectedOption);
                    _embedFields[index] = (name, value, inlineBool);
                    
                    await responseMessage.DeleteAsync();
                    await UpdateMessage();
                });
            }
            
        }
    }
    
    private async Task HandleImgEdit(ComponentInteractionCreateEventArgs e)
    {
        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.WithTitle("Embed Edit - Image");
        responseBuilder.WithCustomId("embedCreator_editImg");
        
        responseBuilder.AddComponents(
            new TextInputComponent("Image URL", "url", "Embed Image URL", _embedImageUrl ?? "", false, TextInputStyle.Short, 0, 2048)
        );
        responseBuilder.AddComponents(
            new TextInputComponent("Thumbnail URL", "t_url", "Embed Thumnail URL", _embedThumbnailUrl ?? "", false, TextInputStyle.Short, 0, 2048)
        );
        
        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
        var result = await _client.GetInteractivity()
            .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id
            && x.Interaction.Data.CustomId == "embedCreator_editImg", TimeSpan.FromMinutes(5));
        
        if (result.TimedOut)
        {
            return;
        }
        await result.Ack();

        string? url = result.Result.Values.GetValueOrDefault("url");
        string? t_url = result.Result.Values.GetValueOrDefault("t_url");
        
        _embedImageUrl = url;

        _embedThumbnailUrl = t_url;

        await UpdateMessage();
    }

    private async Task HandleFooterEdit(ComponentInteractionCreateEventArgs e)
    {
        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.WithTitle("Embed Edit - Footer");
        responseBuilder.WithCustomId("embedCreator_editFooter");
        
        responseBuilder.AddComponents(
            new TextInputComponent("Footer Text", "text", "Embed Footer Text", _embedFooterText ?? "", false, TextInputStyle.Short, 0, 2048)
        );
        
        responseBuilder.AddComponents(
            new TextInputComponent("Footer Icon URL", "icon", "Embed Footer Icon URL", _embedFooterIconUrl ?? "", false, TextInputStyle.Short, 0, 2048)
        );
        
        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
        var result = await _client.GetInteractivity()
            .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id
            && x.Interaction.Data.CustomId == "embedCreator_editFooter", TimeSpan.FromMinutes(5));
        
        if (result.TimedOut)
        {
            return;
        }
        await result.Ack();

        string? text = result.Result.Values.GetValueOrDefault("text");
        string? icon = result.Result.Values.GetValueOrDefault("icon");
        _embedFooterText = text;
        _embedFooterIconUrl = icon;
        
        await UpdateMessage();
    }

    private async Task HandleAuthorEdit(ComponentInteractionCreateEventArgs e)
    {
        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.WithTitle("Embed Edit - Author");
        responseBuilder.WithCustomId("embedCreator_editAuthor");
        
        responseBuilder.AddComponents(
            new TextInputComponent("Author Name", "name", "Embed Author Name", _embedAuthorName ?? "", false, TextInputStyle.Short, 0, 256)
        );
        
        responseBuilder.AddComponents(
            new TextInputComponent("Author URL", "url", "Embed Author URL", _embedAuthorUrl ?? "", false, TextInputStyle.Short, 0, 2048)
        );
        
        responseBuilder.AddComponents(
            new TextInputComponent("Author Icon URL", "icon", "Embed Author Icon URL", _embedAuthorIconUrl ?? "", false, TextInputStyle.Short, 0, 2048)
        );
        
        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
        var result = await _client.GetInteractivity()
            .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id
            && x.Interaction.Data.CustomId == "embedCreator_editAuthor", TimeSpan.FromMinutes(5));
        
        if (result.TimedOut)
        {
            return;
        }
        await result.Ack();

        string? name = result.Result.Values.GetValueOrDefault("name");
        string? url = result.Result.Values.GetValueOrDefault("url");
        string? icon = result.Result.Values.GetValueOrDefault("icon");
        _embedAuthorName = name;
        _embedAuthorUrl = url;
        _embedAuthorIconUrl = icon;
        
        await UpdateMessage();
    }

    private async Task HandleDateEdit(ComponentInteractionCreateEventArgs e)
    {
        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.WithTitle("Embed Edit - Date");
        responseBuilder.WithCustomId("embedCreator_editDate");
        responseBuilder.AddComponents(
            new TextInputComponent("Date", "date", "yyyy-MM-dd HH:mm:ss", _embedTimestamp ?? "", false, TextInputStyle.Short, 0, 256)
        );
        
        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
        var result = await _client.GetInteractivity()
            .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id
            && x.Interaction.Data.CustomId == "embedCreator_editDate", TimeSpan.FromMinutes(5));
        
        if (result.TimedOut)
        {
            return;
        }
        await result.Ack();

        string? date = result.Result.Values.GetValueOrDefault("date");
        
        if (date != null)
        {
            if (DateTime.TryParse(date, out var parsedDate))
            {
                _embedTimestamp = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }else
        {
            _embedTimestamp = null;
        }
        
        await UpdateMessage();
    }

    private async Task HandleReset(ComponentInteractionCreateEventArgs e)
    {
        _embedTitle = "default";
        _embedDescription = "default";
        _embedColor = null;
        _embedFooterText = "default";
        _embedFooterIconUrl = null;
        _embedAuthorName = "default";
        _embedAuthorUrl = null;
        _embedAuthorIconUrl = null;
        _embedTimestamp = null;
        await e.Ack();
        await UpdateMessage();
    }

    private async Task HandleDone(ComponentInteractionCreateEventArgs _)
    {
        await Stop(false);
    }
    
    private async Task UpdateMessage()
    {
        if (_embedMessage == null)
            return;
        
        await _embedMessage.ModifyAsync(BuildMsgBuilder());
    }
    
    private async Task Stop(bool failed)
    {
        _timeoutTimer.Stop();
        _client.ComponentInteractionCreated -= OnComponentInteractionCreated;
        _client.MessageDeleted -= OnMessageDeleted;

        if (failed)
        {
            _readyToSend.SetResult(null);
        }else
        {
            _readyToSend.SetResult(BuildEmbed());
        }

        if (_embedMessage is not null)
        {
            try
            {
                await _embedMessage.DeleteAsync();
            }catch
            {
                // ignored
            }
        }
    }
}