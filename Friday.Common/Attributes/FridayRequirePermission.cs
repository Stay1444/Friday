using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Friday.Common.Attributes;

public class FridayRequirePermission : CheckBaseAttribute
{
    private readonly Permissions _permissions;
    public FridayRequirePermission(Permissions permissions)
    {
        _permissions = permissions;
    }
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Member is null)  return false;
        if (ctx.Member.Id == ctx.Guild.Id) return true;
        if (ctx.Member.Permissions.HasFlag(_permissions)) return true;
        var fridayModerators = ctx.Services.GetService<FridayModeratorService>();
        if (await fridayModerators!.IsModerator(ctx.User.Id)) return true;
        
        return false;
    }
}