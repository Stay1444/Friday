using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Entities;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.AntiRaid.Commands;

public partial class Commands
{
    [Command("antiraid")]
    [Description("AntiRaid settings")]
    public async Task AntiRaidSettingsCommand(CommandContext ctx)
    {
        var guildAntiRaid = await _module.GetAntiRaid(ctx.Guild);
        
        var uiBuilder = new FridayUIBuilder().OnRenderAsync(async x =>
        {
            x.OnCancelledAsync(async (_, message) =>
            {
                await message.ModifyAsync(new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder().Transparent()
                        .WithDescription("AntiRaid settings saved!")));
            });
            
            x.Embed.Title = await ctx.GetString("AntiRaid");
            x.Embed.Description = await ctx.GetString("Manage AntiRaid Settings");
            x.Embed.WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl);
            
            await x.AddButton(async button =>
            {
                button.Label = guildAntiRaid.Settings!.Enabled ? await ctx.GetString("common.enabled") : await ctx.GetString("common.disabled");
                button.Style = guildAntiRaid.Settings!.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;
                
                button.OnClick(async () =>
                {
                    guildAntiRaid.Settings!.Enabled = !guildAntiRaid.Settings!.Enabled;
                    await guildAntiRaid.SaveSettingsAsync();
                });
            });
            
            await x.AddSubPageAsync("permissions", async permissionsPage =>
            {
                permissionsPage.Embed.Title = await ctx.GetString("AntiRaid - Permissions");
                permissionsPage.Embed.Description = await ctx.GetString("Manage AntiRaid Permissions settings");
                
                if (!guildAntiRaid.Settings!.AdminsCanBypass)
                {
                    permissionsPage.Embed.AddField(await ctx.GetString("Require Owner"), await ctx.GetString("Only the server owner can use the AntiRaid commands.\nBypasses AntiRaid restrictions."));
                }
                else
                {
                    permissionsPage.Embed.AddField(await ctx.GetString("Require Admin"), await ctx.GetString("Any member with Administrator permission can use the AntiRaid commands.\nAdmins bypass AntiRaid restrictions."));
                }

                await permissionsPage.AddButton(async button =>
                {
                    button.Label = await ctx.GetString("common.back");
                    button.Style = ButtonStyle.Secondary;
                    
                    button.OnClick(() =>
                    {
                        x.SubPage = null;
                    });
                });
                
                await permissionsPage.AddButton(async button =>
                {
                    button.Label = guildAntiRaid.Settings!.AdminsCanBypass ? await ctx.GetString("Require Admin") : await ctx.GetString("Require Owner");
                    button.Style = ButtonStyle.Primary;
                    button.OnClick(async () =>
                    {
                        guildAntiRaid.Settings!.AdminsCanBypass = !guildAntiRaid.Settings!.AdminsCanBypass;
                        await guildAntiRaid.SaveSettingsAsync();
                    });
                });
                
                await x.AddButton(async button =>
                {
                    button.Label = await ctx.GetString("common.permissions");
                    button.Style = ButtonStyle.Primary;
                    button.Disabled = !ctx.IsCallerOwner();
                    button.OnClick(() =>
                    {
                        x.SubPage = "permissions";
                    });
                });
            });

            await x.AddSubPageAsync("events", async eventsPage =>
            {
                eventsPage.Embed.Title = await ctx.GetString("AntiRaid - Event Settings");
                eventsPage.Embed.Description = await ctx.GetString("Manage AntiRaid Events");

                x.AddButton(button =>
                {
                    button.Label = "Events";
                    button.Style = ButtonStyle.Primary;
                    button.OnClick(() =>
                    {
                        x.SubPage = "events";
                    });
                });

                await eventsPage.AddButton(async button =>
                {
                    button.Label = await ctx.GetString("common.back");
                    button.Style = ButtonStyle.Secondary;
                    button.OnClick(() =>
                    {
                        x.SubPage = null;
                    });
                });

                await eventsPage.AddSubPageAsync("channel", async channelEventsPage =>
                {
                    channelEventsPage.Embed.Title = await ctx.GetString("AntiRaid - Channels");
                    channelEventsPage.Embed.Description = await ctx.GetString("Manage channel deletion events");
                    channelEventsPage.Embed.Color = guildAntiRaid.Settings!.Channels.Enabled ? DiscordColor.SpringGreen : DiscordColor.IndianRed;
                    channelEventsPage.Embed.AddField("Threshold", "The number of channels that can be deleted before the member is " +
                                                                  (guildAntiRaid.Settings!.Channels.Ban ? "banned" : "kicked") + (guildAntiRaid.Settings!.Channels.Restore ? " and the channels are restored." : ".") + 
                                                                      "\n```" + guildAntiRaid.Settings!.Channels.Count + "```");
                    channelEventsPage.Embed.AddField("Punishment", guildAntiRaid.Settings!.Channels.Ban ? "```Ban```" : "```Kick```");
                    channelEventsPage.Embed.AddField("Restore", guildAntiRaid.Settings!.Channels.Restore ? "```Yes```" : "```No```");
                    channelEventsPage.Embed.AddField("Time Span", "If threshold is reached in the given time span, the member will be " +
                                                                  (guildAntiRaid.Settings!.Channels.Ban ? "banned" : "kicked") + (guildAntiRaid.Settings!.Channels.Restore ? " and the channels are restored." : ".") + 
                                                                      "\n```" + new HumanTimeSpan(guildAntiRaid.Settings!.Channels.Time).Humanize() + "```");
                    eventsPage.AddButton(button =>
                    {
                        button.Label = "Channels";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = "channel";
                        });
                    });
                    
                    await channelEventsPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = null;
                        });
                    });

                    await channelEventsPage.AddButton(async button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Channels.Enabled ? await ctx.GetString("common.enabled") : await ctx.GetString("common.disabled");
                        button.Style = guildAntiRaid.Settings!.Channels.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;
                
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Channels.Enabled = !guildAntiRaid.Settings!.Channels.Enabled;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });
                    
                    channelEventsPage.NewLine();
                    
                    channelEventsPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Threshold";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "Channel Deletion Threshold";
                        
                        modal.AddField("threshold", field =>
                        {
                            field.Value = guildAntiRaid.Settings!.Channels.Count.ToString();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Threshold";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async (result) =>
                        {
                            if (int.TryParse(result["threshold"], out var threshold))
                            {
                                guildAntiRaid.Settings!.Channels.Count = threshold;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });

                    channelEventsPage.AddButton(button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Channels.Ban ? "Ban" : "Kick";
                        button.Style = guildAntiRaid.Settings!.Channels.Ban ? ButtonStyle.Danger : ButtonStyle.Primary;
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Channels.Ban = !guildAntiRaid.Settings!.Channels.Ban;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });

                    channelEventsPage.AddButton(button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Channels.Restore ? "Restore" : "Don't Restore";
                        button.Style = guildAntiRaid.Settings!.Channels.Restore
                            ? ButtonStyle.Success
                            : ButtonStyle.Danger;
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Channels.Restore = !guildAntiRaid.Settings!.Channels.Restore;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });
                    
                    channelEventsPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Time Span";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "CHannel Deletion Time Span";
                        
                        modal.AddField("example", field =>
                        {
                            field.Value = new HumanTimeSpan(guildAntiRaid.Settings!.Channels.Time).ToHumanTime();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Example";
                            field.Disabled = true;
                        });
                        
                        modal.AddField("timeSpan", field =>
                        {
                            field.Value = new HumanTimeSpan(guildAntiRaid.Settings!.Channels.Time).ToHumanTime();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Time Span";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async result =>
                        {
                            if (HumanTimeSpan.TryParse(result["timeSpan"], out var timeSpan))
                            {
                                guildAntiRaid.Settings!.Channels.Time = timeSpan.Value;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });
                });

            });

        });
        uiBuilder.Duration = TimeSpan.FromSeconds(60);
        await ctx.SendUIAsync(uiBuilder);
    }
}