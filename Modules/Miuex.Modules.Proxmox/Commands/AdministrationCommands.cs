using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Miuex.Modules.Proxmox.Attributes;
using Miuex.Modules.Proxmox.Models;
using Miuex.Modules.Proxmox.Services;

namespace Miuex.Modules.Proxmox.Commands;

[SlashCommandGroup("admin", "Administration commands")]
public class AdministrationCommands : ApplicationCommandModule
{
    
    private APIService _apiService;
    private VPSSqlService _vpsSqlService;

    public AdministrationCommands(ProxmoxModule module)
    {
        _apiService = module.Api;
        _vpsSqlService = module.VpsSqlService;
    }

    [SlashCommand("linkvps", "Links a vps to a user"), RequireAdminRole]
    public async Task LinkVpsCommand(InteractionContext ctx, [Option("Node", "Node Id")] long node,
        [Option("vmId", "VmID")] long vmId, [Option("User", "User")] DiscordUser user,
        [Option("Name", "Name")] string name)
    {
        if (name.Length > 64)
        {
            await ctx.CreateResponseAsync("Name is too long (max 64 characters)");
            return;
        }

        var vm = await _apiService.GetVirtualMachineAsync((int) node, (int) vmId);

        if (vm == null)
        {
            await ctx.CreateResponseAsync("VM not found with the given ID and node", true);
            return;
        }

        var sqlVm = await _vpsSqlService.GetVps(name);

        if (sqlVm is not null)
        {
            if (sqlVm.Value.nodeId == node && sqlVm.Value.vmId == vmId && sqlVm.Value.owner == user.Id)
            {
                await ctx.CreateResponseAsync("VM is already linked to this user", true);
                return;
            }
            else
            {
                await ctx.CreateResponseAsync("That VM name is already used", true);
                return;
            }
        }

        await _vpsSqlService.AddVps(user.Id, (int) node, (int) vmId, name, DateTime.Now + TimeSpan.FromDays(365));

        await ctx.CreateResponseAsync($"VM {vm.Name} has been linked to {user.Username}", true);
        await ctx.Log(new DiscordEmbedBuilder().WithTitle("VPS Linked")
            .WithDescription($"{vm.Name} has been linked to {user.Username} by {ctx.User.Mention}")
            .WithColor(DiscordColor.Green));

    }
    
    [SlashCommand("unlinkvps", "Unlinks a vps"), RequireAdminRole]
    public async Task UnlinkVpsCommand(InteractionContext ctx, [Option("Name", "Name")] string name,
        [Option("User", "User")] DiscordUser user)
    {
        await _vpsSqlService.RemoveVps(name);
        await ctx.Log(new DiscordEmbedBuilder().WithTitle("VPS Unlinked")
            .WithDescription($"VPS {name} has been unlinked from {user.Username} by {ctx.User.Mention}")
            .WithColor(DiscordColor.Red));
        await ctx.CreateResponseAsync("VM has been unlinked from user", true);
    }

    [SlashCommand("listvps", "Lists all vps"), RequireAdminRole]
    public async Task ListVpsCommand(InteractionContext ctx, [Option("User", "User")] DiscordUser user)
    {
        var vpsSqlList = await _vpsSqlService.GetBindedVps(user.Id);

        if (vpsSqlList.Count == 0)
        {
            await ctx.CreateResponseAsync("User has no vps linked", true);
            return;
        }

        var vms = new List<VirtualMachine>();

        foreach (var vpsSql in vpsSqlList)
        {
            var vm = await _apiService.GetVirtualMachineAsync(vpsSql.nodeId, vpsSql.vmId);
            if (vm is null)
            {
                await ctx.CreateResponseAsync($"Internal error (VM not found({vpsSql.vmId}?))");
                return;
            }

            vms.Add(vm);
        }

        List<DiscordEmbed> pages = new List<DiscordEmbed>();
        var redBall = DiscordEmoji.FromName(ctx.Client, ":red_circle:");
        var greenBall = DiscordEmoji.FromName(ctx.Client, ":green_circle:");
        while (vms.Any())
        {
            var page = new DiscordEmbedBuilder();
            page.WithColor(Constants.COLOR_DEFAULT);
            page.WithTitle("All VPS");
            string description = "";
            int added = 0;
            for (int i = 0; i < vms.Count && i < 9; i++)
            {
                added++;
                description += $"{vms[i].Name} - {(vms[i].Status == PveStatus.Running ? greenBall : redBall)}\n";
                page.WithFooter($"Page {pages.Count + 1}/{Math.Ceiling(vms.Count / 9.0)} | {vms.Count} VMs");
            }
            page.WithDescription(description);
            pages.Add(page);
            vms.RemoveRange(0, added);
        }

        if (pages.Count > 1)
        {
            await ctx.SendPaginatedMessageAsync(pages);
        }
        else
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(pages[0]));
        }

    }

    [SlashCommand("listallvps", "Lists all vps"), RequireAdminRole]
    public async Task ListAllVpsCommand(InteractionContext ctx)
    {
        var vpsSqlList = await _vpsSqlService.GetAllVps();

        if (vpsSqlList.Count == 0)
        {
            await ctx.CreateResponseAsync("No vps linked");
            return;
        }

        var vms = new List<VirtualMachine>();
        var ordered = vpsSqlList.OrderBy(x => x.expiration);
        foreach (var vpsSql in ordered)
        {
            var vm = await _apiService.GetVirtualMachineAsync(vpsSql.nodeId, vpsSql.vmId);
            if (vm is null)
            {
                await ctx.CreateResponseAsync($"Internal error (VM not found({vpsSql.vmId}?))");
                return;
            }

            vms.Add(vm);
        }

        List<DiscordEmbed> pages = new List<DiscordEmbed>();
        var redBall = DiscordEmoji.FromName(ctx.Client, ":red_circle:");
        var greenBall = DiscordEmoji.FromName(ctx.Client, ":green_circle:");
        while (vms.Any())
        {
            var page = new DiscordEmbedBuilder();
            page.WithColor(Constants.COLOR_DEFAULT);
            page.WithTitle("All VPS");
            string description = "";
            int added = 0;
            for (int i = 0; i < vms.Count && i < 9; i++)
            {
                added++;
                description += $"{vms[i].Name} - {(vms[i].Status == PveStatus.Running ? greenBall : redBall)}\n";
                page.WithFooter($"Page {pages.Count + 1}/{Math.Ceiling(vms.Count / 9.0)} | {vms.Count} VMs");
            }
            page.WithDescription(description);
            pages.Add(page);
            vms.RemoveRange(0, added);
        }
        
                if (pages.Count > 1)
                {
                    await ctx.SendPaginatedMessageAsync(pages);
                }
                else
                {
                    await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(pages[0]));
                }
    }
    
    [SlashCommand("addtime", "Adds time to a vps"), RequireAdminRole]
    public async Task AddTimeCommand(InteractionContext ctx, [Option("Name", "VPS Name")] string name, [Option("Time", "Time")]TimeSpan? time)
    {
        if (time is null)
        {
            await ctx.CreateResponseAsync("Missing time");
            return;
        }
        
        var vpsSql = await _vpsSqlService.GetVps(name);
        if (vpsSql is null)
        {
            await ctx.CreateResponseAsync($"VPS {name} not found");
            return;
        }
        
        await _vpsSqlService.AddTime(name, time.Value);
        
        await ctx.CreateResponseAsync($"Added {time} to {name}. New expiration: {vpsSql.Value.expiration + time}");
    }
    
}
