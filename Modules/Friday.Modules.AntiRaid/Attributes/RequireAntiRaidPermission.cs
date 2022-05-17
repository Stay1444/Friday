using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Friday.Modules.AntiRaid.Attributes;

public class RequireAntiRaidPermission : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Guild.OwnerId == ctx.User.Id)
            return true;

        var moderatorService = ctx.Services.GetService<FridayModeratorService>();
        if (await moderatorService!.IsModerator(ctx.User))
        {
            return true;
        }
        
        if (!ctx.Member!.Permissions.HasFlag(Permissions.Administrator))
        {
            return false;
        }
        var antiRaidModule = ctx.Services.GetService<AntiRaidModule>()!;

        var antiRaid = await antiRaidModule.GetAntiRaid(ctx.Guild);

        if (antiRaid.Settings!.AdminsCanBypass)
            return true;
        
        return false;
    }
}