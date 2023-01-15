using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.InviteTracker.Commands;

public partial class Commands
{
    [GroupCommand]
    public async Task cmd_SettingsCommand(CommandContext ctx)
    {
        var guildSettings = await _module.GetConfiguration(ctx.Guild);
        var uiBuilder = new FridayUIBuilder
        {
            Duration = TimeSpan.FromSeconds(60)
        };
        uiBuilder.OnRenderAsync(async x =>
        {
            x.OnCancelledAsync(async (_, message) =>
            {
                await message.DeleteAsync();  
            });
            
            x.Embed.Title = "Invite Tracker Settings";
            x.Embed.Color = guildSettings.Enabled ? DiscordColor.SpringGreen : DiscordColor.IndianRed;
            await x.AddButton(async button =>
            {
                button.Label = guildSettings.Enabled ? await ctx.GetString("common.enabled") : await ctx.GetString("common.disabled");
                button.Style = guildSettings.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;
                
                button.OnClick(async () =>
                {
                    guildSettings.Enabled = !guildSettings.Enabled;
                    await _module.SetConfiguration(ctx.Guild, guildSettings);
                });
            });

            x.AddModal(modal =>
                    {
                        modal.Title = "Channel Id";
                        modal.ButtonLabel = "Select by Channel Id";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        modal.ButtonEmoji = DiscordEmoji.FromName(ctx.Client, ":pencil2:");
                        
                        modal.AddField("id", field =>
                        {
                            field.Required = true;
                            field.MinimumLength = 1;
                            field.MaximumLength = 16;
                            field.Style = TextInputStyle.Short;
                            field.Title = "Id";
                        });

                        modal.OnSubmit(fields =>
                        {
                            if (fields.IsEmpty()) return;
                            
                            var idString = fields["id"];

                            if (!ulong.TryParse(idString, out var id))
                            {
                                return;
                            }

                            if (ctx.Guild.Channels.ContainsKey(id)) {
                                guildSettings.JoinLogChannel = id;
                                await _module.SetConfiguration(ctx.Guild, guildSettings);
                            }

                            x.ForceRender();
                        });
                    });

            x.NewLine();
            await x.AddSelect(async select =>
            {
                select.Disabled = !guildSettings.Enabled;
                select.Placeholder = await ctx.GetString("it.cfg.select.placeholder");

                foreach (var channel in ctx.Guild.Channels.Values)
                {
                    if (channel.Type != ChannelType.Text) continue;

                    select.AddOption(option =>
                    {
                        option.Label = "# " + channel.Name;
                        option.Description = channel.Id.ToString();
                        option.Value = channel.Id.ToString();
                        option.IsDefault = guildSettings.JoinLogChannel == channel.Id;
                    });

                    if (select.Options.Count > 24) {
                        break;
                    }
                }

                select.OnSelect(async result =>
                {
                    if (!result.Any()) return;
                    
                    if (!ulong.TryParse(result.First(), out var channelId)) return;
                    
                    guildSettings.JoinLogChannel = channelId;
                    await _module.SetConfiguration(ctx.Guild, guildSettings);
                });

            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }
}
