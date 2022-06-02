using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Attributes;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;

namespace Friday.Modules.Moderation.Commands;

public class KickCommand : FridayCommandModule
{
    private ModerationModuleBase _moderationModuleBase;
    private FridayConfiguration _config;
    private LanguageProvider _languageProvider;
    
    public KickCommand(ModerationModuleBase moderationModuleBase, FridayConfiguration config, LanguageProvider languageProvider)
    {
        _moderationModuleBase = moderationModuleBase;
        _config = config;
        _languageProvider = languageProvider;
    }


    [Command("kick")]
    [RequireGuild]
    [FridayRequirePermission(Permissions.KickMembers)]
    [RequireBotPermissions(Permissions.KickMembers)]
    public async Task Kick(CommandContext ctx, DiscordMember member, [RemainingText] string? reason = null)
    {
        if (string.IsNullOrEmpty(reason))
        {
            reason = "No reason provided";
            
        }
        
        var ackMsgBuilder = await GetAckEmbed(ctx, member.Id, reason);
        var botMember = await ctx.GetCurrentMember();
        if(!await CanbeKicked(ctx, member, botMember)) return;
        _ = Task.Run(async () =>
        {
            try
            {
                await member.SendMessageAsync(new DiscordEmbedBuilder()
                    .WithTitle($"You have been kicked from {ctx.Guild.Name}")
                    .WithDescription(reason)
                    .Transparent()
                    .AddField("Kicked By", ctx.Member!.Mention));
            }
            catch
            {
                // ignored
            }
        });
        await member.RemoveAsync(reason);
        await ctx.RespondAsync(ackMsgBuilder);
    }

    private async Task<bool> CanbeKicked(CommandContext ctx, DiscordMember member, DiscordMember bot)
    {
        if (member.Id == bot.Id)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.kick.messages.checks.kickBot"));
            return false;
        }
        
        if (member.Id == ctx.Member!.Id)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.kick.messages.checks.kickSelf"));
            return false;
        }

        if (member.Hierarchy >= bot.Hierarchy)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.kick.messages.checks.cantKick"));
            return false;
        }
        
        if (member.Id == ctx.Guild.Owner.Id)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.kick.messages.checks.kickOwner"));
            return false;
        }
        
        if (member.Hierarchy >= ctx.Member.Hierarchy)
        {
            await ctx.RespondAsync(await ctx.GetString("moderation.commands.kick.messages.checks.kickMember"));
            return false;
        }
        
        return true;
    }
    private async Task<DiscordMessageBuilder> GetAckEmbed(CommandContext ctx, ulong member, string reason)
    {
        DiscordMessageBuilder ackMsgBuilder = new DiscordMessageBuilder();
        DiscordEmbedBuilder ackEmbedBuilder = new DiscordEmbedBuilder();
        ackEmbedBuilder.WithColor(new DiscordColor(_config.Discord.Color));
        ackEmbedBuilder.WithTitle("Member Kicked Successfully");
        ackEmbedBuilder.WithFooter(ctx.Member!.GetName(), ctx.Member.AvatarUrl);
        ackEmbedBuilder.WithDescription(reason);
        ackEmbedBuilder.AddField("Member", $"<@{member}>");
        ackMsgBuilder.WithEmbed(ackEmbedBuilder);
        return ackMsgBuilder;
    }

    
}