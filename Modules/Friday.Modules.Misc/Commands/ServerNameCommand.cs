using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common.Attributes;

namespace Friday.Modules.Misc.Commands;

public partial class Commands
{
    [Command("servername"), FridayRequirePermission(Permissions.Administrator), RequireGuild, Cooldown(1, 10, CooldownBucketType.Guild)]
    public async Task cmd_ServerName(CommandContext ctx, [RemainingText] string name)
    {
        var success = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
        var failure = DiscordEmoji.FromName(ctx.Client, ":x:");
        try
        {
            await ctx.Guild.ModifyAsync(x => x.Name = name);
            await ctx.Message.CreateReactionAsync(success);
        }catch
        {
            await ctx.Message.CreateReactionAsync(failure);
        }
    }
}