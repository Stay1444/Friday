using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Attributes;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Modules.ChannelStats.Entities;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.ChannelStats.Commands;

[Group("channelstats"), RequireGuild, Aliases("gcs"), FridayRequirePermission(Permissions.Administrator)]
public class Commands : FridayCommandModule
{
    private ChannelStatsModule _module;
    private FridayConfiguration _fridayConfiguration;
    public Commands(ChannelStatsModule module, FridayConfiguration fridayConfiguration)
    {
        _module = module;
        _fridayConfiguration = fridayConfiguration;
    }

    [GroupCommand, FridayRequirePermission(Permissions.Administrator)]
    public async Task Main(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder();
        var channels = await _module.DatabaseService.GetGuildStatsChannelsAsync(ctx.Guild.Id);
        uiBuilder.Duration = TimeSpan.FromMinutes(5);
        uiBuilder.OnRender(x =>
        {
            x.Embed.Transparent();
            x.Embed.Title = "Guild Channel Stats";
            x.Embed.AddField("Channels", channels.Count + "/" + ChannelStatsModule.MaxChannelsPerGuild, true);
            x.Embed.AddField("Next Update", new HumanTimeSpan(_module.UntilNextUpdate).Humanize(2), true);

            x.AddButton(editButton =>
            {
                editButton.Label = "Edit";
                editButton.Style = ButtonStyle.Primary;
                editButton.OnClick(() =>
                {
                    x.SubPage = "list";
                });
            });
            
            x.AddButton(variables =>
            {
                variables.Label = "Variables";
                variables.Style = ButtonStyle.Secondary;
                variables.OnClick(() =>
                {
                    x.SubPage = "variables";
                });
            });
            
            x.AddSubPageAsync("list", async listPage =>
            {
                var index = x.GetState("index", 0);
                
                if (channels.Count == 0)
                {
                    listPage.Embed.Title = "No channels";
                    listPage.Embed.Description = "There are no channels to display.";
                }
                else
                {
                    listPage.Embed.Title = $"GCS: Channels";
                    listPage.Embed.WithFooter($"{index.Value + 1}/{channels.Count}");
                    listPage.Embed.AddField("Channel", $"<#{channels[index.Value].Id}>", true);
                    var processed = await _module.VariablesService.Process(ctx.Guild, channels[index.Value].Value);
                    if (string.IsNullOrEmpty(processed))
                    {
                        processed = "`Empty`";
                    }
                    listPage.Embed.AddField("Next Name", processed, true);
                    listPage.Embed.AddField("Value", channels[index.Value].Value);
                }
                listPage.AddButton(previous =>
                {
                    previous.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");
                    previous.Style = ButtonStyle.Secondary;
                    previous.Disabled = channels.Count < 2;
                    
                    previous.OnClick(() =>
                    {
                        index.Value--;
                        if (index.Value < 0)
                        {
                            index.Value = channels.Count - 1;
                        }
                    });
                });

                listPage.AddButton(empty =>
                {
                    empty.Emoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Transparent);
                    empty.Disabled = true;
                });
                
                listPage.AddButton(next =>
                {
                    next.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
                    next.Style = ButtonStyle.Secondary;
                    next.Disabled = channels.Count < 2;
                    next.OnClick(() =>
                    {
                        index.Value++;
                        if (index.Value >= channels.Count)
                            index.Value = 0;
                    });
                });
                listPage.NewLine();

                listPage.AddButton(delete =>
                {
                    delete.Style = ButtonStyle.Danger;
                    delete.Emoji = DiscordEmoji.FromName(ctx.Client, ":wastebasket:");
                    delete.Disabled = channels.Count == 0;
                    delete.Label = "Delete";
                    
                    delete.OnClick(async () =>
                    {
                        await _module.DatabaseService.DeleteAsync(channels[index.Value].Id);
                        channels.RemoveAt(index.Value);
                        if (channels.Count != 0)
                        {
                            index.Value = Math.Clamp(index.Value - 1, 0, channels.Count - 1);
                        }
                        else
                        {
                            index.Value = 0;
                        }
                    });
                });

                listPage.AddButton(empty =>
                {
                    empty.Emoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Transparent);
                    empty.Disabled = true;
                });
                
                listPage.AddModal(value =>
                {
                    value.ButtonLabel = "Edit Value";
                    value.ButtonStyle = ButtonStyle.Primary;
                    value.ButtonEmoji = DiscordEmoji.FromName(ctx.Client, ":pencil2:");
                    value.ButtonDisabled = channels.Count == 0;
                    
                    value.Title = "Edit Value";
                    if (!value.ButtonDisabled)
                    {
                        value.AddField("value", field =>
                        {
                            field.Placeholder = "Value";
                            field.Value = channels[index.Value].Value;
                            field.Required = true;
                            field.MinimumLength = 1;
                            field.MaximumLength = 100;
                            field.Style = TextInputStyle.Paragraph;
                        });
                    }

                    value.OnSubmit(async result =>
                    {
                        channels[index.Value].Value = result["value"];
                        await _module.DatabaseService.InsertOrUpdate(ctx.Guild.Id, channels[index.Value].Id, result["value"]);
                        x.ForceRender();
                    });
                });

                listPage.NewLine();

                listPage.AddSelect(channelSelect =>
                {
                    channelSelect.Placeholder = "Channel";
                    channelSelect.Disabled = channels.Count == 0;
                    if (!channelSelect.Disabled)
                    {
                        foreach (var discordChannel in ctx.Guild.Channels.Values.Where(c => c.Type == ChannelType.Voice))
                        {
                            if (channels.Any(c => c.Id == discordChannel.Id && discordChannel.Id != channels[index.Value].Id)) continue;
                            channelSelect.AddOption(option =>
                            {
                                option.Description = discordChannel.Parent?.Name ?? null;
                                option.Emoji = DiscordEmoji.FromName(ctx.Client, ":loud_sound:");
                                option.Label = discordChannel.Name;
                                if (discordChannel.Id == channels[index.Value].Id)
                                {
                                    option.IsDefault = true;
                                }

                                option.Value = discordChannel.Id.ToString();
                            });
                        }
                    }
                    else
                    {
                        channelSelect.AddOption(option =>
                        {
                            option.Label = "No channels";
                            option.Value = "a";
                        });
                    }
                    
                    channelSelect.OnSelect(async result =>
                    {
                        if (result.Length < 1 || result[0] == "a")
                        {
                            return;
                        }
    
                        if (result[0] == channels[index.Value].Id.ToString()) return;
                        
                        if (!ulong.TryParse(result[0], out var channelId))
                        {
                            return;
                        }
                        
                        var channel = ctx.Guild.GetChannel(channelId);
                        if (channel == null)
                        {
                            return;
                        }

                        if (channels.Any(f => f.Id == channel.Id))
                        {
                            return;
                        }

                        try
                        {
                            await _module.DatabaseService.UpdateIdAsync(channels[index.Value].Id, channelId);
                        }catch
                        {
                            x.OnCancelledAsync(async (_, message) =>
                            {
                                await message.ModifyAsync("Channel already used.");
                            });                            
                            
                            x.Stop();
                            return;
                        }
                        channels[index.Value].Id = channel.Id;
                    });
                });

                listPage.NewLine();

                listPage.AddButton(back =>
                {
                    back.Label = "Back";
                    back.OnClick(() =>
                    {
                        x.SubPage = null;
                        index.Value = 0;
                    });
                });

                listPage.AddButton(empty =>
                {
                    empty.Emoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Transparent);
                    empty.Disabled = true;
                });
                
                listPage.AddModal(newChannel =>
                {
                    newChannel.ButtonLabel = "New";
                    newChannel.ButtonStyle = ButtonStyle.Success;
                    newChannel.ButtonDisabled = channels.Count >= ChannelStatsModule.MaxChannelsPerGuild;
                    newChannel.AddField("name", field =>
                    {
                        field.Placeholder = "Name";
                        field.Required = true;
                        field.MinimumLength = 1;
                        field.MaximumLength = 100;
                    });
                    
                    newChannel.OnSubmit(async result =>
                    {
                        var newChannelObject = new GuildStatsChannel()
                        {
                            Id = 0,
                            Value = result["name"]
                        };
                        
                        channels.Add(newChannelObject);
                        
                        await _module.DatabaseService.InsertOrUpdate(ctx.Guild.Id, newChannelObject.Id, newChannelObject.Value);
                        
                        index.Value = channels.Count - 1;
                        
                        x.ForceRender();
                    });
                });
            });
            
            x.AddSubPage("variables", variablesPage =>
            {
                variablesPage.Embed.Transparent();
                variablesPage.Embed.Title = "GCS: Variables";
                variablesPage.Embed.WithFooter($"Use {ctx.Prefix}gcs vars <variable> to get more information about a variable.");
                foreach (var variable in _module.VariablesService.GetVariables())
                {
                    var argsString = $"{{{variable.Value.Name}";
                    if (variable.Value.Parameters.Any())
                    {
                        argsString += $"[{string.Join(", ", variable.Value.Parameters)}]}}";
                    }else
                    {
                        argsString += "}";
                    }
                    variablesPage.Embed.AddField(variable.Value.Name, variable.Value.Description + "\n```css\n" + argsString + "\n```");
                }
                
                variablesPage.AddButton(back =>
                {
                    back.Label = "Back";
                    back.OnClick(() =>
                    {
                        x.SubPage = null;
                    });
                });
            });
        });
        await ctx.SendUIAsync(uiBuilder);
    }
    
    [Command("vars")]
    [Description("Get information about a variable.")]
    [Aliases("variable")]
    [RequireGuild, FridayRequirePermission(Permissions.Administrator)]
    public async Task GetVariableAsync(CommandContext ctx, [RemainingText] [Description("The variable to get information about.")] string variableName)
    {
        var variables = _module.VariablesService.GetVariables();
        if (!variables.ContainsKey(variableName))
        {
            if (variableName.Contains("{") || variableName.Contains("}") || variableName.Contains("[") || variableName.Contains("]"))
            {
                await ctx.RespondAsync($"Variable `{variableName}` not found. Did you mean `{variableName.Split("[")[0].Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "")}`?");
            }else
            {
                await ctx.RespondAsync($"Variable `{variableName} not found.");
            }
            
            return;
        }

        var embedBuilder = new DiscordEmbedBuilder();
        embedBuilder.Transparent();
        embedBuilder.Title = $"GCS: Variable {variableName}";
        embedBuilder.AddField("Description", variables[variableName].Description);
        var argsString = $"{{{variables[variableName].Name}";
        if (variables[variableName].Parameters.Any())
        {
            argsString += $"[{string.Join(", ", variables[variableName].Parameters)}]}}";
        }else
        {
            argsString += "}";
        }
        embedBuilder.AddField("Usage", "```\n" + argsString + "\n```");
        embedBuilder.AddField("Example", await variables[variableName].Example(_module, ctx.Guild));
        
        await ctx.RespondAsync(embed: embedBuilder);
    }
}