using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Friday.Common.Attributes;

public class FridayRequireGuildOwner : CheckBaseAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Member is null)
        {
            return Task.FromResult(false);
        }
        
        if (ctx.Guild.OwnerId == ctx.Member.Id)
        {
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }
}