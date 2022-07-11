using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Miuex.Modules.Proxmox.Models;

namespace Miuex.Modules.Proxmox.Attributes;

public class RequireAdminRole : SlashCheckBaseAttribute
{
    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        var config = ctx.Services.GetService<Configuration>();
        if (config is null)
            return Task.FromResult(false);
        
        var guild = ctx.Guild;
        if (guild is null)
            return Task.FromResult(false);
        
        if (config?.AdminRole is null)
            return Task.FromResult(false);
        
        var adminRole = guild.GetRole(config.AdminRole.Value);

        if (adminRole is null)
            return Task.FromResult(false);
        
        var member = ctx.Member;
        
        if (member is null)
            return Task.FromResult(false);
        
        return Task.FromResult(member.Roles.Contains(adminRole));
    }
}