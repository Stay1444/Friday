using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Entities;
using Friday.Modules.AntiRaid.Entities;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.AntiRaid.Commands;

public partial class Commands
{
    [Description("AntiRaid settings")]
    [GroupCommand]
    public async Task AntiRaidSettingsCommand(CommandContext ctx)
    {
        var guildAntiRaid = await _module.GetAntiRaid(ctx.Guild);

        AntiRaidSettings.AntiRaidSettingsChannels? selectedChannelSettings = null;
        string? selectedChannelSettingsName = null;

        AntiRaidSettings.AntiRaidSettingsRoles? selectedRolesSettings = null;
        string? selectedRolesSettingsName = null;

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
            x.Embed.Color = guildAntiRaid.Settings!.Enabled ? DiscordColor.SpringGreen : DiscordColor.IndianRed;

            await x.AddButton(async button =>
            {
                button.Label = guildAntiRaid.Settings!.Enabled
                    ? await ctx.GetString("common.enabled")
                    : await ctx.GetString("common.disabled");
                button.Style = guildAntiRaid.Settings!.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;

                button.OnClick(async () =>
                {
                    guildAntiRaid.Settings!.Enabled = !guildAntiRaid.Settings!.Enabled;
                    await guildAntiRaid.SaveSettingsAsync();
                });
            });

            x.AddSubPageAsync("permissions", async permissionsPage =>
            {
                permissionsPage.Embed.Title = await ctx.GetString("AntiRaid - Permissions");
                permissionsPage.Embed.Description = await ctx.GetString("Manage AntiRaid Permissions settings");

                if (!guildAntiRaid.Settings!.AdminsCanBypass)
                    permissionsPage.Embed.AddField(await ctx.GetString("Require Owner"),
                        await ctx.GetString(
                            "Only the server owner can use the AntiRaid commands.\nBypasses AntiRaid restrictions."));
                else
                    permissionsPage.Embed.AddField(await ctx.GetString("Require Admin"),
                        await ctx.GetString(
                            "Any member with Administrator permission can use the AntiRaid commands.\nAdmins bypass AntiRaid restrictions."));

                await permissionsPage.AddButton(async button =>
                {
                    button.Label = await ctx.GetString("common.back");
                    button.Style = ButtonStyle.Secondary;

                    button.OnClick(() => { x.SubPage = null; });
                });

                await permissionsPage.AddButton(async button =>
                {
                    button.Label = guildAntiRaid.Settings!.AdminsCanBypass
                        ? await ctx.GetString("Require Admin")
                        : await ctx.GetString("Require Owner");
                    button.Style = ButtonStyle.Primary;
                    button.OnClick(async () =>
                    {
                        guildAntiRaid.Settings!.AdminsCanBypass = !guildAntiRaid.Settings!.AdminsCanBypass;
                        await guildAntiRaid.SaveSettingsAsync();
                    });
                });
            });

            await x.AddButton(async button =>
            {
                button.Label = await ctx.GetString("common.permissions");
                button.Style = ButtonStyle.Primary;
                button.Disabled = !ctx.IsCallerOwner();
                button.OnClick(() => { x.SubPage = "permissions"; });
            });

            x.AddSubPageAsync("events", async eventsPage =>
            {
                eventsPage.Embed.Title = await ctx.GetString("AntiRaid - Event Settings");
                eventsPage.Embed.Description = await ctx.GetString("Manage AntiRaid Events");

                await eventsPage.AddButton(async button =>
                {
                    button.Label = await ctx.GetString("common.back");
                    button.Style = ButtonStyle.Secondary;
                    button.OnClick(() => { x.SubPage = null; });
                });

                eventsPage.AddButton(button =>
                {
                    button.Label = "Channels";
                    button.Style = ButtonStyle.Primary;
                    button.OnClick(() => { eventsPage.SubPage = "channel"; });
                });

                eventsPage.AddSubPageAsync("channel", async channelEventsPage =>
                {
                    channelEventsPage.Embed.Title = "AntiRaid - Channels";
                    channelEventsPage.Embed.Description = "Manage channel Events";

                    await channelEventsPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() => { eventsPage.SubPage = null; });
                    });
                    channelEventsPage.NewLine();

                    channelEventsPage.AddButton(button =>
                    {
                        button.Label = "On Delete";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedChannelSettings = guildAntiRaid.Settings!.DeleteChannels;
                            selectedChannelSettingsName = "delete";
                            channelEventsPage.SubPage = "settings";
                        });
                    });

                    channelEventsPage.AddButton(button =>
                    {
                        button.Label = "On Create";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedChannelSettings = guildAntiRaid.Settings!.CreateChannels;
                            selectedChannelSettingsName = "create";
                            channelEventsPage.SubPage = "settings";
                        });
                    });

                    channelEventsPage.AddButton(button =>
                    {
                        button.Label = "On Update";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedChannelSettings = guildAntiRaid.Settings!.UpdateChannels;
                            selectedChannelSettingsName = "update";
                            channelEventsPage.SubPage = "settings";
                        });
                    });
                    if (selectedChannelSettings is not null)
                        channelEventsPage.AddSubPageAsync("settings", async settingsChannelsPage =>
                        {
                            settingsChannelsPage.Embed.Title =
                                "AntiRaid - Channels " + selectedChannelSettingsName + " Events";
                            settingsChannelsPage.Embed.Description =
                                $"Manage channel {selectedChannelSettingsName} events";
                            settingsChannelsPage.Embed.Color = selectedChannelSettings!.Enabled
                                ? DiscordColor.SpringGreen
                                : DiscordColor.IndianRed;
                            settingsChannelsPage.Embed.AddField("Threshold",
                                $"The number of channels that can be {selectedChannelSettingsName} before the member is " +
                                (selectedChannelSettings.Ban ? "banned" : "kicked") +
                                (selectedChannelSettings.Restore ? " and the channels are restored." : ".") +
                                "\n```" + selectedChannelSettings.Count + "```");
                            settingsChannelsPage.Embed.AddField("Punishment",
                                selectedChannelSettings.Ban ? "```Ban```" : "```Kick```");
                            settingsChannelsPage.Embed.AddField("Restore",
                                selectedChannelSettings.Restore ? "```Yes```" : "```No```");
                            settingsChannelsPage.Embed.AddField("Time Span",
                                "If threshold is reached in the given time span, the member will be " +
                                (selectedChannelSettings.Ban ? "banned" : "kicked") +
                                (selectedChannelSettings.Restore ? " and the channels are restored." : ".") +
                                "\n```" + new HumanTimeSpan(selectedChannelSettings.Time).Humanize() + "```");

                            await settingsChannelsPage.AddButton(async button =>
                            {
                                button.Label = await ctx.GetString("common.back");
                                button.Style = ButtonStyle.Secondary;
                                button.OnClick(() => { channelEventsPage.SubPage = null; });
                            });

                            await settingsChannelsPage.AddButton(async button =>
                            {
                                button.Label = selectedChannelSettings.Enabled
                                    ? await ctx.GetString("common.enabled")
                                    : await ctx.GetString("common.disabled");
                                button.Style = selectedChannelSettings.Enabled
                                    ? ButtonStyle.Success
                                    : ButtonStyle.Danger;

                                button.OnClick(async () =>
                                {
                                    selectedChannelSettings.Enabled = !selectedChannelSettings.Enabled;
                                    await guildAntiRaid.SaveSettingsAsync();
                                });
                            });

                            settingsChannelsPage.NewLine();

                            settingsChannelsPage.AddModal(modal =>
                            {
                                modal.ButtonLabel = "Threshold";
                                modal.ButtonStyle = ButtonStyle.Primary;

                                modal.Title = $"Channel {selectedChannelSettingsName} Threshold";

                                modal.AddField("threshold", field =>
                                {
                                    field.Value = selectedChannelSettings.Count.ToString();
                                    field.Style = TextInputStyle.Short;
                                    field.Title = "Threshold";
                                    field.Required = true;
                                });

                                modal.OnSubmit(async result =>
                                {
                                    if (int.TryParse(result["threshold"], out var threshold))
                                    {
                                        selectedChannelSettings.Count = threshold;
                                        await guildAntiRaid.SaveSettingsAsync();
                                        x.ForceRender();
                                    }
                                });
                            });

                            settingsChannelsPage.AddButton(button =>
                            {
                                button.Label = selectedChannelSettings.Ban ? "Ban" : "Kick";
                                button.Style = selectedChannelSettings.Ban
                                    ? ButtonStyle.Danger
                                    : ButtonStyle.Primary;
                                button.OnClick(async () =>
                                {
                                    selectedChannelSettings.Ban = !selectedChannelSettings.Ban;
                                    await guildAntiRaid.SaveSettingsAsync();
                                });
                            });

                            settingsChannelsPage.AddButton(button =>
                            {
                                button.Label = selectedChannelSettings.Restore ? "Restore" : "Don't Restore";
                                button.Style = selectedChannelSettings.Restore
                                    ? ButtonStyle.Success
                                    : ButtonStyle.Danger;
                                button.OnClick(async () =>
                                {
                                    selectedChannelSettings.Restore = !selectedChannelSettings.Restore;
                                    await guildAntiRaid.SaveSettingsAsync();
                                });
                            });

                            settingsChannelsPage.AddModal(modal =>
                            {
                                modal.ButtonLabel = "Time Span";
                                modal.ButtonStyle = ButtonStyle.Primary;

                                modal.Title = $"Channel {selectedChannelSettingsName} Time Span";

                                modal.AddField("timeSpan", field =>
                                {
                                    field.Value = new HumanTimeSpan(selectedChannelSettings.Time).ToHumanTime();
                                    field.Style = TextInputStyle.Short;
                                    field.Title = "Time Span";
                                    field.Required = true;
                                });

                                modal.OnSubmit(async result =>
                                {
                                    if (HumanTimeSpan.TryParse(result["timeSpan"], out var timeSpan))
                                    {
                                        selectedChannelSettings.Time = timeSpan.Value;
                                        await guildAntiRaid.SaveSettingsAsync();
                                        x.ForceRender();
                                    }
                                });
                            });
                        });
                });

                eventsPage.AddButton(button =>
                {
                    button.Label = "Roles";
                    button.Style = ButtonStyle.Primary;
                    button.OnClick(() => { eventsPage.SubPage = "role"; });
                });

                eventsPage.AddSubPageAsync("role", async roleEventsPage =>
                {
                    roleEventsPage.Embed.Title = "AntiRaid - Roles";
                    roleEventsPage.Embed.Description = "Manage Role Events";


                    await roleEventsPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() => { eventsPage.SubPage = null; });
                    });

                    roleEventsPage.NewLine();

                    roleEventsPage.AddButton(button =>
                    {
                        button.Label = "On Delete";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedRolesSettings = guildAntiRaid.Settings!.DeleteRoles;
                            selectedRolesSettingsName = "delete";
                            roleEventsPage.SubPage = "settings";
                        });
                    });

                    roleEventsPage.AddButton(button =>
                    {
                        button.Label = "On Create";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedRolesSettings = guildAntiRaid.Settings!.CreateRoles;
                            selectedRolesSettingsName = "create";
                            roleEventsPage.SubPage = "settings";
                        });
                    });

                    roleEventsPage.AddButton(button =>
                    {
                        button.Label = "On Update";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedRolesSettings = guildAntiRaid.Settings!.UpdateRoles;
                            selectedRolesSettingsName = "update";
                            roleEventsPage.SubPage = "settings";
                        });
                    });

                    roleEventsPage.AddButton(button =>
                    {
                        button.Label = "On Grant";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedRolesSettings = guildAntiRaid.Settings!.GrantRoles;
                            selectedRolesSettingsName = "grant";
                            roleEventsPage.SubPage = "settings";
                        });
                    });

                    roleEventsPage.AddButton(button =>
                    {
                        button.Label = "On Revoke";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() =>
                        {
                            selectedRolesSettings = guildAntiRaid.Settings!.RevokeRoles;
                            selectedRolesSettingsName = "revoke";
                            roleEventsPage.SubPage = "settings";
                        });
                    });

                    if (selectedRolesSettings is not null)
                        roleEventsPage.AddSubPageAsync("settings", async settingsChannelsPage =>
                        {
                            settingsChannelsPage.Embed.Title =
                                "AntiRaid - Roles " + selectedRolesSettingsName + " Events";
                            settingsChannelsPage.Embed.Description = $"Manage role {selectedRolesSettingsName} events";
                            settingsChannelsPage.Embed.Color = selectedRolesSettings!.Enabled
                                ? DiscordColor.SpringGreen
                                : DiscordColor.IndianRed;
                            settingsChannelsPage.Embed.AddField("Threshold",
                                $"The number of roles that can be {selectedRolesSettingsName} before the member is " +
                                (selectedRolesSettings.Ban ? "banned" : "kicked") +
                                (selectedRolesSettings.Restore ? " and the roles are restored." : ".") +
                                "\n```" + selectedRolesSettings.Count + "```");
                            settingsChannelsPage.Embed.AddField("Punishment",
                                selectedRolesSettings.Ban ? "```Ban```" : "```Kick```");
                            settingsChannelsPage.Embed.AddField("Restore",
                                selectedRolesSettings.Restore ? "```Yes```" : "```No```");
                            settingsChannelsPage.Embed.AddField("Time Span",
                                "If threshold is reached in the given time span, the member will be " +
                                (selectedRolesSettings.Ban ? "banned" : "kicked") +
                                (selectedRolesSettings.Restore ? " and the roles are restored." : ".") +
                                "\n```" + new HumanTimeSpan(selectedRolesSettings.Time).Humanize() + "```");

                            await settingsChannelsPage.AddButton(async button =>
                            {
                                button.Label = await ctx.GetString("common.back");
                                button.Style = ButtonStyle.Secondary;
                                button.OnClick(() => { roleEventsPage.SubPage = null; });
                            });

                            await settingsChannelsPage.AddButton(async button =>
                            {
                                button.Label = selectedRolesSettings.Enabled
                                    ? await ctx.GetString("common.enabled")
                                    : await ctx.GetString("common.disabled");
                                button.Style = selectedRolesSettings.Enabled
                                    ? ButtonStyle.Success
                                    : ButtonStyle.Danger;

                                button.OnClick(async () =>
                                {
                                    selectedRolesSettings.Enabled = !selectedRolesSettings.Enabled;
                                    await guildAntiRaid.SaveSettingsAsync();
                                });
                            });

                            settingsChannelsPage.NewLine();

                            settingsChannelsPage.AddModal(modal =>
                            {
                                modal.ButtonLabel = "Threshold";
                                modal.ButtonStyle = ButtonStyle.Primary;

                                modal.Title = $"Rhannel {selectedRolesSettingsName} Threshold";

                                modal.AddField("threshold", field =>
                                {
                                    field.Value = selectedRolesSettings.Count.ToString();
                                    field.Style = TextInputStyle.Short;
                                    field.Title = "Threshold";
                                    field.Required = true;
                                });

                                modal.OnSubmit(async result =>
                                {
                                    if (int.TryParse(result["threshold"], out var threshold))
                                    {
                                        selectedRolesSettings.Count = threshold;
                                        await guildAntiRaid.SaveSettingsAsync();
                                        x.ForceRender();
                                    }
                                });
                            });

                            settingsChannelsPage.AddButton(button =>
                            {
                                button.Label = selectedRolesSettings.Ban ? "Ban" : "Kick";
                                button.Style = selectedRolesSettings.Ban
                                    ? ButtonStyle.Danger
                                    : ButtonStyle.Primary;
                                button.OnClick(async () =>
                                {
                                    selectedRolesSettings.Ban = !selectedRolesSettings.Ban;
                                    await guildAntiRaid.SaveSettingsAsync();
                                });
                            });

                            settingsChannelsPage.AddButton(button =>
                            {
                                button.Label = selectedRolesSettings.Restore ? "Restore" : "Don't Restore";
                                button.Style = selectedRolesSettings.Restore
                                    ? ButtonStyle.Success
                                    : ButtonStyle.Danger;
                                button.OnClick(async () =>
                                {
                                    selectedRolesSettings.Restore = !selectedRolesSettings.Restore;
                                    await guildAntiRaid.SaveSettingsAsync();
                                });
                            });

                            settingsChannelsPage.AddModal(modal =>
                            {
                                modal.ButtonLabel = "Time Span";
                                modal.ButtonStyle = ButtonStyle.Primary;

                                modal.Title = $"Role {selectedRolesSettingsName} Time Span";

                                modal.AddField("timeSpan", field =>
                                {
                                    field.Value = new HumanTimeSpan(selectedRolesSettings.Time).ToHumanTime();
                                    field.Style = TextInputStyle.Short;
                                    field.Title = "Time Span";
                                    field.Required = true;
                                });

                                modal.OnSubmit(async result =>
                                {
                                    if (HumanTimeSpan.TryParse(result["timeSpan"], out var timeSpan))
                                    {
                                        selectedRolesSettings.Time = timeSpan.Value;
                                        await guildAntiRaid.SaveSettingsAsync();
                                        x.ForceRender();
                                    }
                                });
                            });
                        });
                });

                eventsPage.AddButton(button =>
                {
                    button.Label = "Ban";
                    button.Style = ButtonStyle.Primary;
                    button.OnClick(() => { eventsPage.SubPage = "ban"; });
                });

                eventsPage.AddSubPageAsync("ban", async banEventPage =>
                {
                    banEventPage.Embed.Title = await ctx.GetString("AntiRaid - Ban");
                    banEventPage.Embed.Description = await ctx.GetString("Manage ban events");
                    banEventPage.Embed.Color = guildAntiRaid.Settings!.Bans.Enabled
                        ? DiscordColor.SpringGreen
                        : DiscordColor.IndianRed;
                    banEventPage.Embed.AddField("Threshold",
                        "The number of bans that can be issued before the member is " +
                        (guildAntiRaid.Settings!.Bans.Ban ? "banned" : "kicked") +
                        "\n```" + guildAntiRaid.Settings!.Bans.Count + "```");
                    banEventPage.Embed.AddField("Punishment",
                        guildAntiRaid.Settings!.Bans.Ban ? "```Ban```" : "```Kick```");
                    banEventPage.Embed.AddField("Time Span",
                        "If threshold is reached in the given time span, the member will be " +
                        (guildAntiRaid.Settings!.Bans.Ban ? "banned" : "kicked") +
                        "\n```" + new HumanTimeSpan(guildAntiRaid.Settings!.Bans.Time).Humanize() + "```");


                    await banEventPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() => { eventsPage.SubPage = null; });
                    });

                    await banEventPage.AddButton(async button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Bans.Enabled
                            ? await ctx.GetString("common.enabled")
                            : await ctx.GetString("common.disabled");
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

                        modal.OnSubmit(async result =>
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

                eventsPage.AddSubPageAsync("kick", async kickEventPage =>
                {
                    kickEventPage.Embed.Title = await ctx.GetString("AntiRaid - Kick");
                    kickEventPage.Embed.Description = await ctx.GetString("Manage kick events");
                    kickEventPage.Embed.Color = guildAntiRaid.Settings!.Kicks.Enabled
                        ? DiscordColor.SpringGreen
                        : DiscordColor.IndianRed;
                    kickEventPage.Embed.AddField("Threshold",
                        "The number of kicks that can be issued before the member is " +
                        (guildAntiRaid.Settings!.Kicks.Ban ? "banned" : "kicked") +
                        "\n```" + guildAntiRaid.Settings!.Kicks.Count + "```");
                    kickEventPage.Embed.AddField("Punishment",
                        guildAntiRaid.Settings!.Kicks.Ban ? "```Ban```" : "```Kick```");
                    kickEventPage.Embed.AddField("Time Span",
                        "If threshold is reached in the given time span, the member will be " +
                        (guildAntiRaid.Settings!.Kicks.Ban ? "banned" : "kicked") +
                        "\n```" + new HumanTimeSpan(guildAntiRaid.Settings!.Kicks.Time).Humanize() + "```");
                    eventsPage.AddButton(button =>
                    {
                        button.Label = "Kick";
                        button.Style = ButtonStyle.Primary;
                        button.OnClick(() => { eventsPage.SubPage = "kick"; });
                    });

                    await kickEventPage.AddButton(async button =>
                    {
                        button.Label = await ctx.GetString("common.back");
                        button.Style = ButtonStyle.Secondary;
                        button.OnClick(() => { eventsPage.SubPage = null; });
                    });

                    await kickEventPage.AddButton(async button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Kicks.Enabled
                            ? await ctx.GetString("common.enabled")
                            : await ctx.GetString("common.disabled");
                        button.Style = guildAntiRaid.Settings!.Kicks.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;

                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Kicks.Enabled = !guildAntiRaid.Settings!.Kicks.Enabled;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });

                    kickEventPage.NewLine();

                    kickEventPage.AddModal(modal =>
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

                        modal.OnSubmit(async result =>
                        {
                            if (int.TryParse(result["threshold"], out var threshold))
                            {
                                guildAntiRaid.Settings!.Kicks.Count = threshold;
                                await guildAntiRaid.SaveSettingsAsync();
                                x.ForceRender();
                            }
                        });
                    });

                    kickEventPage.AddButton(button =>
                    {
                        button.Label = guildAntiRaid.Settings!.Kicks.Ban ? "Ban" : "Kick";
                        button.Style = guildAntiRaid.Settings!.Kicks.Ban ? ButtonStyle.Danger : ButtonStyle.Primary;
                        button.OnClick(async () =>
                        {
                            guildAntiRaid.Settings!.Kicks.Ban = !guildAntiRaid.Settings!.Kicks.Ban;
                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });

                    kickEventPage.AddModal(modal =>
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

            x.AddButton(button =>
            {
                button.Label = "Events";
                button.Style = ButtonStyle.Primary;
                button.OnClick(() => { x.SubPage = "events"; });
            });
            
            x.AddSubPageAsync("account-age", async agePage =>
            {
                agePage.Embed.Transparent();
                agePage.Embed.Title = "AntiRaid - Account Restrictions";
                agePage.Embed.Color = guildAntiRaid.Settings.MinimumAge.Enabled ? DiscordColor.SpringGreen : DiscordColor.IndianRed;
                agePage.Embed.AddField("Minimum Age",
                    $"```\n{guildAntiRaid.Settings.MinimumAge.MinimumAge.ToHumanTimeSpan().Humanize()}\n```");

                agePage.AddButton(back =>
                {
                    back.Label = "Back";
                    back.OnClick(() => x.SubPage = null);
                });
                
                await agePage.AddButton(async button =>
                {
                    button.Label = guildAntiRaid.Settings!.MinimumAge.Enabled
                        ? await ctx.GetString("common.enabled")
                        : await ctx.GetString("common.disabled");
                    button.Style = guildAntiRaid.Settings!.MinimumAge.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;

                    button.OnClick(async () =>
                    {
                        guildAntiRaid.Settings!.MinimumAge.Enabled = !guildAntiRaid.Settings!.MinimumAge.Enabled;
                        await guildAntiRaid.SaveSettingsAsync();
                    });
                });
                
                agePage.AddModal(minimumModal =>
                {
                    minimumModal.ButtonStyle = ButtonStyle.Primary;
                    minimumModal.ButtonLabel = "Minimum Age";
                    minimumModal.Title = "Minimum Age";
                    minimumModal.AddField("0", field =>
                    {
                        field.Title = "Minimum Age";
                        field.Value = guildAntiRaid.Settings.MinimumAge.MinimumAge.ToHumanTimeSpan().ToHumanTime();
                        field.Placeholder = "7d";
                    });
                    
                    minimumModal.OnSubmit(async result =>
                    {
                        if (HumanTimeSpan.TryParse(result["0"], out var timeSpan))
                        {
                            guildAntiRaid.Settings.MinimumAge.MinimumAge = timeSpan;

                            await guildAntiRaid.SaveSettingsAsync();
                        }
                    });
                });
            });

            x.AddButton(button =>
            {
                button.Label = "Account Restrictions";
                button.Style = ButtonStyle.Primary;

                button.OnClick(() => x.SubPage = "account-age");
            });
            
            x.AddSubPageAsync("logs", async logsPage =>
            {
                logsPage.Embed.Title = "AntiRaid - Logs";
                logsPage.Embed.Description = "Log Settings";

                if (ctx.Guild.Channels.Any(xr => xr.Key == guildAntiRaid.Settings!.Logs.ChannelId))
                    logsPage.Embed.AddField("Logs Channel", "<#" + guildAntiRaid.Settings!.Logs.ChannelId + ">");
                else
                    logsPage.Embed.AddField("Logs Channel", "None");

                logsPage.Embed.Color = guildAntiRaid.Settings!.Logs.Enabled
                    ? DiscordColor.SpringGreen
                    : DiscordColor.IndianRed;


                logsPage.AddButton(button =>
                {
                    button.Label = "Back";
                    button.Style = ButtonStyle.Secondary;
                    button.OnClick(() => { x.SubPage = null; });
                });


                await logsPage.AddButton(async button =>
                {
                    button.Label = guildAntiRaid.Settings!.Logs.Enabled
                        ? await ctx.GetString("common.enabled")
                        : await ctx.GetString("common.disabled");
                    button.Style = guildAntiRaid.Settings!.Logs.Enabled ? ButtonStyle.Success : ButtonStyle.Danger;

                    button.OnClick(async () =>
                    {
                        guildAntiRaid.Settings!.Logs.Enabled = !guildAntiRaid.Settings!.Logs.Enabled;
                        await guildAntiRaid.SaveSettingsAsync();
                    });
                });

                if (guildAntiRaid.Settings!.Logs.Enabled)
                {
                    logsPage.NewLine();

                    logsPage.AddSelect(select =>
                    {
                        select.Placeholder = "Select Log Channel";

                        foreach (var textChannel in ctx.Guild.Channels.Where(xr => xr.Value.Type == ChannelType.Text))
                            select.AddOption(option =>
                            {
                                option.Label = "# " + textChannel.Value.Name;
                                option.Value = textChannel.Value.Id.ToString();
                                option.Description = textChannel.Value.Parent != null
                                    ? textChannel.Value.Parent.Name
                                    : null;
                                option.IsDefault = guildAntiRaid.Settings!.Logs.ChannelId == textChannel.Key;
                            });

                        select.OnSelect(async result =>
                        {
                            if (!result.Any()) return;

                            if (!ulong.TryParse(result.First(), out var channelId)) return;

                            guildAntiRaid.Settings!.Logs.ChannelId = channelId;

                            await guildAntiRaid.SaveSettingsAsync();
                        });
                    });
                }
            });

            x.AddButton(button =>
            {
                button.Label = "Logs";
                button.Style = ButtonStyle.Primary;

                button.OnClick(() => { x.SubPage = "logs"; });
            });
        });
        uiBuilder.Duration = TimeSpan.FromSeconds(60);
        await ctx.SendUIAsync(uiBuilder);
    }
}