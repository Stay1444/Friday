using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Friday.Modules.AntiRaid.Attributes;

public class RequireAntiRaidPermission : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        var antiRaidModule = ctx.Services.GetService<AntiRaidModule>()!;
        
        if (ctx.Guild.OwnerId == ctx.User.Id)
            return true;

        if (!ctx.Member!.Permissions.HasFlag(Permissions.Administrator))
        {
            return false;
        }
        
        var antiRaid = await antiRaidModule.GetAntiRaid(ctx.Guild);

        if (antiRaid.Settings!.AdminsCanBypass)
            return true;
        
        return false;
    }
}