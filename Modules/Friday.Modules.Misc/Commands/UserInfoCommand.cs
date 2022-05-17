using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Serilog;

namespace Friday.Modules.Misc.Commands;

public partial class Commands
{
    [Command("userinfo"), RequireGuild]
    public async Task UserInfoCommand(CommandContext ctx, DiscordMember? member = null)
    {
        if (member == null)
        {
            member = ctx.Member;
        }

        if (member is null) return;
        
        var embedBuilder = new DiscordEmbedBuilder();
        embedBuilder.Transparent();
        embedBuilder.WithTitle(member.IsBot ? "Bot Info" : member.IsSystem ?? false ? "System Info" : member.IsOwner ? "Owner Info" : "User Info");
        embedBuilder.WithAuthor(member.Username, null, member.AvatarUrl);
        embedBuilder.WithThumbnail(member.BannerUrl ?? member.AvatarUrl);
        embedBuilder.AddField("ID", "`"+ member.Id + "`");
        embedBuilder.AddField("Join Date", member.JoinedAt.DateTime.ToLongDateString() + ", " + member.JoinedAt.DateTime.ToShortTimeString(), true);
        embedBuilder.AddField("Created At", member.CreationTimestamp.DateTime.ToLongDateString() + ", " + member.CreationTimestamp.DateTime.ToShortTimeString(), true);
        embedBuilder.AddField("Nickname", member.Nickname ?? "None");
        if (member.Roles.Any())
        {
            embedBuilder.AddField("Roles", string.Join(", ", member.Roles.Take(10).Select(x => x.Mention)) + (member.Roles.Count() > 10 ? "..." : ""), true);
        }else
        {
            embedBuilder.AddField("Roles", "None", true);
        }

        if (member.IsCurrent)
        {
            embedBuilder.WithColor(DiscordColor.Azure);
            embedBuilder.WithFooter(member.Username, member.AvatarUrl);
        }else if (await _moderatorService.IsModerator(member))
        {
            try
            {
                var moderatorEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Mod);
                embedBuilder.WithFooter($"Moderator", moderatorEmoji.Url);
                embedBuilder.WithColor(DiscordColor.Yellow);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to add moderator emoji to userinfo embed");
            }
        }else if (member.PremiumSince is not null)
        {
            try
            {
                var premiumEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Boost);
                embedBuilder.WithFooter($"Booster", premiumEmoji.Url);
                embedBuilder.WithColor(DiscordColor.Magenta);
            }catch (Exception e)
            {
                Log.Error(e, "Failed to add booster emoji to userinfo embed");
            }
        }

        await ctx.RespondAsync(embed: embedBuilder);
    }
}