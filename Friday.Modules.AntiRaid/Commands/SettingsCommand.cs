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
                        
                        modal.Title = "Channel Deletion Time Span";
                        
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

                await eventsPage.AddSubPageAsync("role", async roleEventsPage =>
                {
                    roleEventsPage.Embed.Title = await ctx.GetString("AntiRaid - Roles");
                    roleEventsPage.Embed.Description = await ctx.GetString("Manage role deletion events");
                    roleEventsPage.Embed.Color = guildAntiRaid.Settings!.Roles.Enabled ? DiscordColor.SpringGreen : DiscordColor.IndianRed;
                    roleEventsPage.Embed.AddField("Threshold", "The number of roles that can be deleted before the member is " +
                                                                  (guildAntiRaid.Settings!.Roles.Ban ? "banned" : "kicked") + (guildAntiRaid.Settings!.Roles.Restore ? " and the roles are restored." : ".") + 
                                                                      "\n```" + guildAntiRaid.Settings!.Roles.Count + "```");
                    roleEventsPage.Embed.AddField("Punishment", guildAntiRaid.Settings!.Roles.Ban ? "```Ban```" : "```Kick```");
                    roleEventsPage.Embed.AddField("Restore", guildAntiRaid.Settings!.Roles.Restore ? "```Yes```" : "```No```");
                    roleEventsPage.Embed.AddField("Time Span", "If threshold is reached in the given time span, the member will be " +
                                                                  (guildAntiRaid.Settings!.Roles.Ban ? "banned" : "kicked") + (guildAntiRaid.Settings!.Roles.Restore ? " and the roles will be restored." : ".") + 
                                                                      "\n```" + new HumanTimeSpan(guildAntiRaid.Settings!.Roles.Time).Humanize() + "```");
                    eventsPage.AddButton(button =>
                    {
                        button.Label = "Roles";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = "role";
                        });
                    });
                    
                    await roleEventsPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = null;
                        });
                    });

                    await roleEventsPage.AddButton(async button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Roles.Enabled ? await ctx.GetString("common.enabled") : await ctx.GetString("common.disabled");
                        button.Style = guildAntiRaid.Settings!.Roles.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;
                
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Roles.Enabled = !guildAntiRaid.Settings!.Roles.Enabled;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });
                    
                    roleEventsPage.NewLine();
                    
                    roleEventsPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Threshold";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "Role Deletion Threshold";
                        
                        modal.AddField("threshold", field =>
                        {
                            field.Value = guildAntiRaid.Settings!.Roles.Count.ToString();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Threshold";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async (result) =>
                        {
                            if (int.TryParse(result["threshold"], out var threshold))
                            {
                                guildAntiRaid.Settings!.Roles.Count = threshold;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });

                    roleEventsPage.AddButton(button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Roles.Ban ? "Ban" : "Kick";
                        button.Style = guildAntiRaid.Settings!.Roles.Ban ? ButtonStyle.Danger : ButtonStyle.Primary;
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Roles.Ban = !guildAntiRaid.Settings!.Roles.Ban;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });

                    roleEventsPage.AddButton(button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Roles.Restore ? "Restore" : "Don't Restore";
                        button.Style = guildAntiRaid.Settings!.Roles.Restore
                            ? ButtonStyle.Success
                            : ButtonStyle.Danger;
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Roles.Restore = !guildAntiRaid.Settings!.Roles.Restore;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });
                    
                    roleEventsPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Time Span";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "Role Deletion Time Span";

                        modal.AddField("timeSpan", field =>
                        {
                            field.Value = new HumanTimeSpan(guildAntiRaid.Settings!.Roles.Time).ToHumanTime();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Time Span";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async result =>
                        {
                            if (HumanTimeSpan.TryParse(result["timeSpan"], out var timeSpan))
                            {
                                guildAntiRaid.Settings!.Roles.Time = timeSpan.Value;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });
                });

                await eventsPage.AddSubPageAsync("ban", async banEventPage =>
                {
                    banEventPage.Embed.Title = await ctx.GetString("AntiRaid - Ban");
                    banEventPage.Embed.Description = await ctx.GetString("Manage ban events");
                    banEventPage.Embed.Color = guildAntiRaid.Settings!.Bans.Enabled ? DiscordColor.SpringGreen : DiscordColor.IndianRed;
                    banEventPage.Embed.AddField("Threshold", "The number of bans that can be issued before the member is " +
                                                                  (guildAntiRaid.Settings!.Bans.Ban ? "banned" : "kicked") + 
                                                                      "\n```" + guildAntiRaid.Settings!.Bans.Count + "```");
                    banEventPage.Embed.AddField("Punishment", guildAntiRaid.Settings!.Bans.Ban ? "```Ban```" : "```Kick```");
                    banEventPage.Embed.AddField("Time Span", "If threshold is reached in the given time span, the member will be " +
                                                                  (guildAntiRaid.Settings!.Bans.Ban ? "banned" : "kicked") + 
                                                                      "\n```" + new HumanTimeSpan(guildAntiRaid.Settings!.Bans.Time).Humanize() + "```");
                    eventsPage.AddButton(button =>
                    {
                        button.Label = "Ban";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = "ban";
                        });
                    });
                    
                    await banEventPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = null;
                        });
                    });

                    await banEventPage.AddButton(async button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Bans.Enabled ? await ctx.GetString("common.enabled") : await ctx.GetString("common.disabled");
                        button.Style = guildAntiRaid.Settings!.Bans.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;
                
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Bans.Enabled = !guildAntiRaid.Settings!.Bans.Enabled;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });
                    
                    banEventPage.NewLine();
                    
                    banEventPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Threshold";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "Bans Threshold";
                        
                        modal.AddField("threshold", field =>
                        {
                            field.Value = guildAntiRaid.Settings!.Bans.Count.ToString();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Threshold";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async (result) =>
                        {
                            if (int.TryParse(result["threshold"], out var threshold))
                            {
                                guildAntiRaid.Settings!.Bans.Count = threshold;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });

                    banEventPage.AddButton(button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Bans.Ban ? "Ban" : "Kick";
                        button.Style = guildAntiRaid.Settings!.Bans.Ban ? ButtonStyle.Danger : ButtonStyle.Primary;
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Bans.Ban = !guildAntiRaid.Settings!.Bans.Ban;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });

                    banEventPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Time Span";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "Bans Time Span";

                        modal.AddField("timeSpan", field =>
                        {
                            field.Value = new HumanTimeSpan(guildAntiRaid.Settings!.Bans.Time).ToHumanTime();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Time Span";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async result =>
                        {
                            if (HumanTimeSpan.TryParse(result["timeSpan"], out var timeSpan))
                            {
                                guildAntiRaid.Settings!.Bans.Time = timeSpan.Value;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });
                });
                
                await eventsPage.AddSubPageAsync("kick", async banEventPage =>
                {
                    banEventPage.Embed.Title = await ctx.GetString("AntiRaid - Kick");
                    banEventPage.Embed.Description = await ctx.GetString("Manage kick events");
                    banEventPage.Embed.Color = guildAntiRaid.Settings!.Kicks.Enabled ? DiscordColor.SpringGreen : DiscordColor.IndianRed;
                    banEventPage.Embed.AddField("Threshold", "The number of kicks that can be issued before the member is " +
                                                                  (guildAntiRaid.Settings!.Kicks.Ban ? "banned" : "kicked") + 
                                                                      "\n```" + guildAntiRaid.Settings!.Kicks.Count + "```");
                    banEventPage.Embed.AddField("Punishment", guildAntiRaid.Settings!.Kicks.Ban ? "```Ban```" : "```Kick```");
                    banEventPage.Embed.AddField("Time Span", "If threshold is reached in the given time span, the member will be " +
                                                                  (guildAntiRaid.Settings!.Kicks.Ban ? "banned" : "kicked") + 
                                                                      "\n```" + new HumanTimeSpan(guildAntiRaid.Settings!.Kicks.Time).Humanize() + "```");
                    eventsPage.AddButton(button =>
                    {
                        button.Label = "Kick";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = "kick";
                        });
                    });
                    
                    await banEventPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() =>
                        {
                            eventsPage.SubPage = null;
                        });
                    });

                    await banEventPage.AddButton(async button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Kicks.Enabled ? await ctx.GetString("common.enabled") : await ctx.GetString("common.disabled");
                        button.Style = guildAntiRaid.Settings!.Kicks.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;
                
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Kicks.Enabled = !guildAntiRaid.Settings!.Kicks.Enabled;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });
                    
                    banEventPage.NewLine();
                    
                    banEventPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Threshold";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "Kicks Threshold";
                        
                        modal.AddField("threshold", field =>
                        {
                            field.Value = guildAntiRaid.Settings!.Kicks.Count.ToString();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Threshold";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async (result) =>
                        {
                            if (int.TryParse(result["threshold"], out var threshold))
                            {
                                guildAntiRaid.Settings!.Kicks.Count = threshold;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });

                    banEventPage.AddButton(button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Kicks.Ban ? "Ban" : "Kick";
                        button.Style = guildAntiRaid.Settings!.Kicks.Ban ? ButtonStyle.Danger : ButtonStyle.Primary;
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Kicks.Ban = !guildAntiRaid.Settings!.Kicks.Ban;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });

                    banEventPage.AddModal(modal =>
                    {
                        modal.ButtonLabel = "Time Span";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        
                        modal.Title = "Kicks Time Span";

                        modal.AddField("timeSpan", field =>
                        {
                            field.Value = new HumanTimeSpan(guildAntiRaid.Settings!.Kicks.Time).ToHumanTime();
                            field.Style = TextInputStyle.Short;
                            field.Title = "Time Span";
                            field.Required = true;
                        });
                        
                        modal.OnSubmit(async result =>
                        {
                            if (HumanTimeSpan.TryParse(result["timeSpan"], out var timeSpan))
                            {
                                guildAntiRaid.Settings!.Kicks.Time = timeSpan.Value;
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