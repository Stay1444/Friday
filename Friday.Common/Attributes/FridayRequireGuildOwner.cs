using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Friday.Common.Attributes;

public class FridayRequireGuildOwner : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Member is null)
        {
            return false;
        }
        
        if (ctx.Guild.OwnerId == ctx.Member.Id)
        {
            return true;
        }

        var fridayModeration = ctx.Services.GetService<FridayModeratorService>()!;

        if (await fridayModeration.IsModerator(ctx.Member.Id))
        {
            return true;
        }
        
        return false;
    }
}