using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using Friday.Common;
using Friday.Common.Attributes;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;

namespace Friday.Modules.Moderation.Commands;

public class BanCommand : FridayCommandModule
{
    private ModerationModuleBase _moderationModuleBase;
    private FridayConfiguration _config;
    private LanguageProvider _languageProvider;
    public BanCommand(ModerationModuleBase moderationModuleBase, FridayConfiguration config, LanguageProvider languageProvider)
    {
        _moderationModuleBase = moderationModuleBase;
        _config = config;
        _languageProvider = languageProvider;
    }

        
    [Command("tempban")]
    [RequireGuild]
    [FridayRequirePermission(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [Priority(15)]
    public async Task cmd_Ban(CommandContext ctx, ulong member, DateTime expiration, [RemainingText] string? reason)
    {
        if (await ctx.Guild.GetMemberAsync(member) is not null)
        {
            // member is in guild
            await cmd_Ban(ctx, await ctx.Guild.GetMemberAsync(member), expiration, reason);
            return;
        }
        
        var banState = await _moderationModuleBase.GetBanState(ctx.Guild.Id, member);
        if (banState is not null)
        {
            await _moderationModuleBase.RemoveBanState(banState);
        }

        if (reason is null)
        {
            reason = await ctx.GetString("moderation.defaults.reason");
        }

        var ackMsgBuilder = await GetAckEmbed(ctx, member, reason, expiration);
        await ctx.RespondAsync(ackMsgBuilder);

        await _moderationModuleBase.BanAsync(ctx.Guild.Id, member, ctx.Member.Id, reason, expiration);
    }
    
    
    [Command("tempban")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [Priority(10)]
    public Task cmd_Ban(CommandContext ctx, ulong member, string expiration, [RemainingText] string? reason)
    {
        try
        {
            TimeSpan duration = Utils.ParseVulgarTimeSpan(expiration);
            return cmd_Ban(ctx, member, DateTime.UtcNow + duration, reason);
        }catch(ArgumentException)
        {
            return cmd_Ban(ctx, member, expiration + " " + reason);
        }
    }
    
    [Command("tempban")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [Priority(5)]
    public async Task cmd_Ban(CommandContext ctx, DiscordMember member, DateTime expiration, [RemainingText] string? reason)
    {
        var botMember = await ctx.GetCurrentMember();
        var banState = await _moderationModuleBase.GetBanState(member);
        if (banState is not null)
        {
            await _moderationModuleBase.RemoveBanState(banState);
        }

        if (!await CanBeBanned(ctx, member, botMember)) return;

        if (reason is null)
        {
            reason = await ctx.GetString("moderation.defaults.reason");
        }
        
        var ackMsgBuilder = await GetAckEmbed(ctx, member, reason, expiration);

        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        var ackMsg = await ctx.RespondAsync(ackMsgBuilder);

        var dmMsgBuilder = await GetDmAckEmbed(ctx, member, reason, null);

        try
        {
            await member.SendMessageAsync(dmMsgBuilder);
        }catch
        {
            // ignored. Can't send DM to user.
        }
        
        await _moderationModuleBase.BanAsync(member, ctx.Member, reason, expiration);
        _ = UnbanButtonHandler(ctx, ackMsgBuilder, ackMsg, member.Id);
    }
    
    [Command("tempban")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [Priority(0)]
    public Task cmd_Ban(CommandContext ctx, DiscordMember member, string expiration, [RemainingText] string? reason)
    {
        try
        {
            TimeSpan duration = Utils.ParseVulgarTimeSpan(expiration);
            return cmd_Ban(ctx, member, DateTime.UtcNow + duration, reason);
        }catch(ArgumentException)
        {
            return cmd_Ban(ctx, member, expiration + " " + reason);
        }
    }
    
    [Command("ban")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [Priority(10)]
    public async Task cmd_Ban(CommandContext ctx, ulong member, [RemainingText] string? reason)
    {
        if (await ctx.Guild.GetMemberAsync(member) is not null)
        {
            await cmd_Ban(ctx, await ctx.Guild.GetMemberAsync(member), reason);
            return;
        }
        var banState = await _moderationModuleBase.GetBanState(ctx.Guild.Id, member);
        if (banState is not null)
        {
            await _moderationModuleBase.RemoveBanState(banState);
        }

        if (reason is null)
        {
            reason = await ctx.GetString("moderation.defaults.reason");
        }
        
        var ackMsgBuilder = await GetAckEmbed(ctx, member, reason, null);
        await ctx.RespondAsync(ackMsgBuilder);

        await ctx.Guild.BanMemberAsync(member, 0, reason);
    }
    
    [Command("ban")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [Priority(3)]
    public async Task cmd_Ban(CommandContext ctx, DiscordMember member, [RemainingText] string? reason)
    {
        var botMember = await ctx.GetCurrentMember();
        var banState = await _moderationModuleBase.GetBanState(member);
        if (banState is not null)
        {
            await _moderationModuleBase.RemoveBanState(banState);
        }

        if (!await CanBeBanned(ctx, member, botMember)) return;
        
        if (reason is null)
        {
            reason = await ctx.GetString("moderation.defaults.reason");
        }
        
        var ackMsgBuilder = await GetAckEmbed(ctx, member, reason, null);

        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        var ackMsg = await ctx.RespondAsync(ackMsgBuilder);
        
        var dmMsgBuilder = await GetDmAckEmbed(ctx, member, reason, null);
        
        _ = Task.Run(async () =>
        {
            try
            {
                await member.SendMessageAsync(dmMsgBuilder);
            }
            catch
            {
                //ignored
            }
        });
        
        try
        {
            await ctx.Guild.BanMemberAsync(member, 0, reason);
        }
        catch (UnauthorizedException)
        {
            var failedEmbedBuilder = new DiscordEmbedBuilder();
            failedEmbedBuilder.WithAuthor(member.Username, null, member.AvatarUrl);
            failedEmbedBuilder.WithColor(new DiscordColor(_config.Discord.Color));
            failedEmbedBuilder.WithDescription("Failed to ban user.");
            failedEmbedBuilder.WithTimestamp(DateTime.UtcNow);
            await ackMsg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(failedEmbedBuilder));
            return;
        }

        _ = UnbanButtonHandler(ctx, ackMsgBuilder, ackMsg, member.Id);
    }

    #region Helpers

    private async Task<DiscordMessageBuilder> GetDmAckEmbed(CommandContext ctx, DiscordMember member, string reason, DateTime? expiration)
    {
        DiscordMessageBuilder dmMsgBuilder = new DiscordMessageBuilder();
        DiscordEmbedBuilder dmEmbedBuilder = new DiscordEmbedBuilder();
        dmEmbedBuilder.WithAuthor(member.Username, null, member.AvatarUrl);
        dmEmbedBuilder.WithColor(new DiscordColor(_config.Discord.Color));
        dmEmbedBuilder.WithTitle(await ctx.GetString("moderation.commands.ban.embeds.dmAck.title"));
        dmEmbedBuilder.WithFooter(ctx.Member.GetName(), ctx.Member.AvatarUrl);
        dmEmbedBuilder.WithDescription(reason);
        dmEmbedBuilder.WithTimestamp(DateTime.UtcNow);
        if (expiration is not null)
        {
            dmEmbedBuilder.AddField(await ctx.GetString("moderation.commands.ban.embeds.ack.field.expiration"), expiration.Value.ToString("dd.MM.yyyy HH:mm:ss"));
        }
        dmMsgBuilder.WithEmbed(dmEmbedBuilder);
        return dmMsgBuilder;
    }

    private async Task<DiscordMessageBuilder> GetAckEmbed(CommandContext ctx, DiscordMember member, string reason, DateTime? expiration)
    {
        DiscordMessageBuilder ackMsgBuilder = new DiscordMessageBuilder();
        DiscordEmbedBuilder ackEmbedBuilder = new DiscordEmbedBuilder();
        ackEmbedBuilder.WithAuthor(member.Username, null, member.AvatarUrl);
        ackEmbedBuilder.WithColor(new DiscordColor(_config.Discord.Color));
        ackEmbedBuilder.WithTitle(await ctx.GetString("moderation.commands.ban.embeds.ack.title"));
        ackEmbedBuilder.WithFooter(ctx.Member.GetName(), ctx.Member.AvatarUrl);
        ackEmbedBuilder.WithDescription(reason);
        ackEmbedBuilder.AddField(await ctx.GetString("moderation.commands.ban.embeds.ack.field.member"), member.Mention);
        ackEmbedBuilder.WithTimestamp(DateTime.UtcNow);
        if (expiration is not null)
        {
            ackEmbedBuilder.AddField(await ctx.GetString("moderation.commands.ban.embeds.ack.field.expiration"), expiration.Value.ToString("dd.MM.yyyy HH:mm:ss"));
        }
        ackMsgBuilder.WithEmbed(ackEmbedBuilder);
        ackMsgBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "unban", await ctx.GetString("moderation.commands.ban.embeds.ack.button.unban")));
        return ackMsgBuilder;
    }
    
    private async Task<DiscordMessageBuilder> GetAckEmbed(CommandContext ctx, ulong member, string reason, DateTime? expiration)
    {
        DiscordMessageBuilder ackMsgBuilder = new DiscordMessageBuilder();
        DiscordEmbedBuilder ackEmbedBuilder = new DiscordEmbedBuilder();
        ackEmbedBuilder.WithColor(new DiscordColor(_config.Discord.Color));
        ackEmbedBuilder.WithTitle(await ctx.GetString("moderation.commands.ban.embeds.ack.title"));
        ackEmbedBuilder.WithFooter(ctx.Member.GetName(), ctx.Member.AvatarUrl);
        ackEmbedBuilder.WithDescription(reason);
        ackEmbedBuilder.AddField(await ctx.GetString("moderation.commands.ban.embeds.ack.field.member"), $"<@{member}>");
        ackEmbedBuilder.WithTimestamp(DateTime.UtcNow);
        if (expiration is not null)
        {
            ackEmbedBuilder.AddField(await ctx.GetString("moderation.commands.ban.embeds.ack.field.expiration"), expiration.Value.ToString("dd.MM.yyyy HH:mm:ss"));
        }
        ackMsgBuilder.WithEmbed(ackEmbedBuilder);
        ackMsgBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "unban", await ctx.GetString("moderation.commands.ban.embeds.ack.button.unban")));
        return ackMsgBuilder;
    }

    private async Task<bool> CanBeBanned(CommandContext ctx, DiscordMember member, DiscordMember bot)
    {
        if (member.Id == bot.Id)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.ban.messages.checks.banBot"));
            return false;
        }
        
        if (member.Id == ctx.Member.Id)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.ban.messages.checks.banSelf"));
            return false;
        }

        if (member.Hierarchy >= bot.Hierarchy)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.ban.messages.checks.cantBan"));
            return false;
        }
        
        if (member.Id == ctx.Guild.Owner.Id)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.ban.messages.checks.banOwner"));
            return false;
        }
        
        if (member.Hierarchy >= ctx.Member.Hierarchy)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.ban.messages.checks.banMember"));
            return false;
        }
        
        return true;
    }

    private async Task UnbanButtonHandler(CommandContext ctx, DiscordMessageBuilder ackBuilder, DiscordMessage message, ulong unban)
    {
        while (true)
        {
            var unbanButton = await ctx.Client.GetInteractivity()
                .WaitForButtonAsync(message, TimeSpan.FromSeconds(300));
            
            if (unbanButton.TimedOut) break;
            var clickMember = await ctx.Guild.GetMemberAsync(unbanButton.Result.User.Id);
            if (clickMember is null) continue;
            if (!clickMember.Permissions.HasPermission(Permissions.BanMembers))
            {
                try
                {
                    await clickMember.SendMessageAsync(await _languageProvider.GetString(clickMember, "general.msg.notEnoughPermissions"));
                    await unbanButton.Ack();
                }catch
                {
                    // ignored. Can't send DM to user.
                }
                continue;   
            }
            await unbanButton.Ack();            
            await ctx.Guild.UnbanMemberAsync(unban);
            await _moderationModuleBase.RemoveBanState(unban, ctx.Guild.Id);
            ackBuilder.ClearComponents();
            ackBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "unban", await ctx.GetString("moderation.commands.ban.embeds.ack.button.unbanned"), true));
            await message.ModifyAsync(ackBuilder);
            return;
        }
        ackBuilder.ClearComponents();
        await message.ModifyAsync(ackBuilder);
    }
    
    #endregion
    
    [Command("unban")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [Priority(1)]
    public async Task cmd_Unban(CommandContext ctx, DiscordUser member)
    {
        await _moderationModuleBase.RemoveBanState(member.Id, ctx.Guild.Id);
        try
        {
            await ctx.Guild.UnbanMemberAsync(member.Id);
        }catch
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.unban.messages.failed", "<@" + member.Id + ">"));
            return;
        }

        await ctx.RespondAsync(await ctx.GetString("moderation.commands.unban.messages.success", "<@" + member.Id + ">"));
    }

    [Command("unban")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    public async Task cmd_Unban(CommandContext ctx, ulong memberId)
    {
        await _moderationModuleBase.RemoveBanState(memberId, ctx.Guild.Id);
        try
        {
            await ctx.Guild.UnbanMemberAsync(memberId);
        }catch
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.unban.messages.failed", "<@" + memberId + ">"));
            return;
        }
        
        await ctx.RespondAsync(await ctx.GetString("moderation.commands.unban.messages.success", "<@" + memberId + ">"));
    }
    
}