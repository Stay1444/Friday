using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Friday.Common.Attributes;

public class RequireFridayModerator : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        var fridayModeratorService = ctx.Services.GetService<FridayModeratorService>();

        if (fridayModeratorService is null) return false;
        
        if (!await fridayModeratorService.IsModerator(ctx.User))
        {
            return false;
        }
        
        return true;
    }
}