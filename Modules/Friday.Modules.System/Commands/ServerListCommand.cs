using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common;
using Friday.Common.Attributes;
using Friday.Common.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Friday.Modules.System.Commands;

public class ServerListCommand : FridayCommandModule
{
    [RequireFridayModerator, Command("serverlist")]
    public async Task Command(CommandContext ctx)
    {
        var shardedClient = ctx.Services.GetService<DiscordShardedClient>();
        if (shardedClient is null) return;

        var guilds = shardedClient.GetGuilds().OrderByDescending(x => x.MemberCount);
        int count = 0;
        foreach (var guild in guilds)
        {
            count++;
            await ctx.RespondAsync($"{guild.Name} - {guild.Id} - {guild.MemberCount}");
            if (count == 10) break;
        }
    }
}