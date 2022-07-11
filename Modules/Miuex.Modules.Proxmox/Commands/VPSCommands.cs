using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Miuex.Modules.Proxmox.Models;
using Miuex.Modules.Proxmox.Services;

namespace Miuex.Modules.Proxmox.Commands;

[SlashCommandGroup("vps", "VPS commands")]
public class VPSCommands : ApplicationCommandModule
{
    private VPSSqlService _vpsSqlService;
    private APIService _apiService;
    public VPSCommands(ProxmoxModule module)
    {
        this._vpsSqlService = module.VpsSqlService;
        this._apiService = module.Api;
    }
    
    [SlashCommand("start", "Starts a vps")]
    public async Task StartCommand(InteractionContext ctx, [Option("Name", "VM Name")] string name)
    {
        var sqlVm = await _vpsSqlService.GetVps(name);
        if (sqlVm == null)
        {
            await ctx.CreateResponseAsync("VPS not found", true);
            return;
        }
        
        if (sqlVm.Value.owner != ctx.User.Id && !ctx.IsAdmin())
        {
            await ctx.CreateResponseAsync("You are not the owner of this VPS", true);
            return;
        }
        

        var vm = await _apiService.GetVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);

        if (vm is null)
        {
            await ctx.CreateResponseAsync("Internal error (VM not found(?))", true);
            return;
        }

        if (vm.Status != PveStatus.Stopped)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Error starting VPS")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is already running. Current status: `" + vm.Status.ToString() + "`")
            ));
            
            return;
        }
        
        var success = await _apiService.StartVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        vm = await _apiService.GetVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        
        if (vm is null)
        {
            await ctx.CreateResponseAsync("Internal error (VM not found(?))", true);
            return;
        }
        
        if (success)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("VPS started")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is now running. Current status: `" + vm.Status.ToString() + "`")
            ));
            
            await ctx.Log(new DiscordEmbedBuilder().WithTitle("VPS started")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is now running. Current status: `" + vm.Status.ToString() + "`")
                .WithFooter("VPS name: " + sqlVm.Value.name)
                .AddField("Owner", $"<@{sqlVm.Value.owner}>")
                .AddField("Action by", ctx.User.Mention)
                .WithTimestamp(DateTime.Now));
        }
        else
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Error starting VPS")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("Unknown error. Current status: `" + vm.Status.ToString() + "`")
            ));
        }
    }
    
    [SlashCommand("stop", "Stops a vps")]
    public async Task StopCommand(InteractionContext ctx, [Option("Name", "VM Name")] string name)
    {
        var sqlVm = await _vpsSqlService.GetVps(name);
        if (sqlVm == null)
        {
            await ctx.CreateResponseAsync("VPS not found", true);
            return;
        }
        
        if (sqlVm.Value.owner != ctx.User.Id && !ctx.IsAdmin())
        {
            await ctx.CreateResponseAsync("You are not the owner of this VPS", true);
            return;
        }
        
        var vm = await _apiService.GetVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        
        if (vm is null)
        {
            await ctx.CreateResponseAsync("Internal error (VM not found(?))", true);
            return;
        }
        
        if (vm.Status == PveStatus.Stopped)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Error stopping VPS")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is already stopped. Current status: `" + vm.Status.ToString() + "`")
            ));
            return;
        }
        
        var success = await _apiService.StopVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        vm = await _apiService.GetVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        if (vm is null)
        {
            await ctx.CreateResponseAsync("Internal error (VM not found(?))", true);
            return;
        }
        
        if (success)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("VPS stopped")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is now stopped. Current status: `" + vm.Status.ToString() + "`")
            ));
            
            await ctx.Log(new DiscordEmbedBuilder().WithTitle("VPS stopped")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is now stopped. Current status: `" + vm.Status.ToString() + "`")
                .WithFooter("VPS name: " + sqlVm.Value.name)
                .AddField("Owner", $"<@{sqlVm.Value.owner}>")
                .AddField("Action by", ctx.User.Mention)
                .WithTimestamp(DateTime.Now));
            
        }
        else
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Error stopping VPS")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("Unknown error. Current status: `" + vm.Status.ToString() + "`")
            ));
        }
    }
    
    [SlashCommand("restart", "Restarts a vps")]
    public async Task RestartCommand(InteractionContext ctx, [Option("Name", "VM Name")] string name)
    {
        var sqlVm = await _vpsSqlService.GetVps(name);
        if (sqlVm == null)
        {
            await ctx.CreateResponseAsync("VPS not found", true);
            return;
        }
        
        if (sqlVm.Value.owner != ctx.User.Id && !ctx.IsAdmin())
        {
            await ctx.CreateResponseAsync("You are not the owner of this VPS", true);
            return;
        }

        var vm = await _apiService.GetVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        
        if (vm is null)
        {
            await ctx.CreateResponseAsync("Internal error (VM not found(?))", true);
            return;
        }
        
        if (vm.Status == PveStatus.Stopped)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Error restarting VPS")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is already stopped. Current status: `" + vm.Status.ToString() + "`")
            ));
            return;
        }
        
        var success = await _apiService.ResetVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        vm.Status = PveStatus.Unknown;        
        if (success)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("VPS restarted")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is now restarting. Current status: `" + vm.Status.ToString() + "`")
            ));
            
            await ctx.Log(new DiscordEmbedBuilder().WithTitle("VPS restarted")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("VPS is now restarting. Current status: `" + vm.Status.ToString() + "`")
                .WithFooter("VPS name: " + sqlVm.Value.name)
                .AddField("Owner", $"<@{sqlVm.Value.owner}>")
                .AddField("Action by", ctx.User.Mention)
                .WithTimestamp(DateTime.Now));
            
        }
        else
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Error restarting VPS")
                .WithColor(Constants.COLOR_DEFAULT)
                .WithDescription("Unknown error. Current status: `" + vm.Status.ToString() + "`")
            ));
        }
    }
    
    [SlashCommand("list", "Lists your vps")]
    public async Task ListCommand(InteractionContext ctx)
    {
        var vpsSqlList = await _vpsSqlService.GetBindedVps(ctx.User.Id);
        
        if (vpsSqlList.Count == 0)
        {
            await ctx.CreateResponseAsync("You don't have any vps binded to your account");
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

        while (vms.Any())
        {
            var page = new DiscordEmbedBuilder();
            page.WithColor(Constants.COLOR_DEFAULT);
            page.WithTitle("Your VPSs");
            
            for (int i = 0; i < vms.Count && i < 9; i++)
            {
                var vmSql = vpsSqlList.First(x => x.nodeId == vms[i].Node && x.vmId == vms[i].Id);
                page.AddField(vmSql.name, 
                    $@"```ml
Status : {vms[i].Status}
CPU    : {vms[i].Resources?.Cpu.Round(2) * 100 ?? 0}%
Uptime : {vms[i].Uptime.Humanize()}
- Memory -
Total  : {vms[i].Resources?.TotalMemory / 1024 / 1024 / 1024 ?? 0}Gb
Used   : {Math.Round(vms[i].Resources?.UsedMemory / 1024 / 1024 / 1024.0f ?? 0, 2)}Gb
Used%  : {(vms[i].Resources?.Memory ?? 0 * 100).Round(2)}%
```", true);
                page.WithFooter($"Page {pages.Count + 1}/{Math.Ceiling(vms.Count / 9.0)} | {vms.Count} VPSs");
                     
            }
            
            pages.Add(page.Build());
            vms.RemoveRange(0, page.Fields.Count);
        }

        if (pages.Count > 1)
        {
            await ctx.SendPaginatedMessageAsync(pages);
        }else
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(pages[0]));
        }
        
    }
    
    [SlashCommand("status", "Status of a vps")]
    public async Task StatusCommand(InteractionContext ctx, [Option("Name", "VM Name")] string name)
    {
        var sqlVm = await _vpsSqlService.GetVps(name);
        
        if (sqlVm is null)
        {
            await ctx.CreateResponseAsync("You don't have a vps with that name");
            return;
        }
        
        if (sqlVm.Value.owner != ctx.User.Id && !ctx.IsAdmin())
        {
            await ctx.CreateResponseAsync("You don't have access to this vps");
            return;
        }
        
        var vm = await _apiService.GetVirtualMachineAsync(sqlVm.Value.nodeId, sqlVm.Value.vmId);
        
        if (vm is null)
        {
            await ctx.CreateResponseAsync("Internal error (VM not found)");
            return;
        }
        
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
            .WithTitle("VPS status")
            .WithColor(Constants.COLOR_DEFAULT)
            .WithDescription($"```ml\n" +
                             $"Status  : {vm.Status}\n" +
                             $"CPU     : {vm.Resources?.Cpu.Round(2) * 100 ?? 0}%\n" +
                             $"Uptime  : {vm.Uptime.Humanize()}\n" +
                             $"- Memory -\n" +
                             $"Total   : {vm.Resources?.TotalMemory / 1024 / 1024 / 1024 ?? 0}Gb\n" +
                             $"Used    : {vm.Resources?.UsedMemory / 1024 / 1024 / 1024 ?? 0}Gb\n" +
                             $"Used%   : {vm.Resources?.Memory.Round(2) * 100 ?? 0}%\n" +
                             $"- File System -\n" +
                             $"Total   : {vm.Resources?.TotalDisk / 1024 / 1024 / 1024 ?? 0}Gb\n" +
                             $"```")
        ));
    }
    
    [SlashCommand("rename", "Renames a vps")]
    public async Task RenameCommand(InteractionContext ctx, [Option("Name", "VM Name")] string name, [Option("NewName", "New VM Name")] string newName)
    {
        if (newName.Length > 64)
        {
            await ctx.CreateResponseAsync("New name is too long. Max 64 characters");
            return;
        }
        
        
        var sqlVm = await _vpsSqlService.GetVps(name);
        
        if (sqlVm is null)
        {
            await ctx.CreateResponseAsync("You don't have a vps with that name");
            return;
        }
        
        if (sqlVm.Value.owner != ctx.User.Id && !ctx.IsAdmin())
        {
            await ctx.CreateResponseAsync("You don't have access to this vps");
            return;
        }

        var success = await _vpsSqlService.RenameVps(ctx.User.Id, sqlVm.Value.nodeId, sqlVm.Value.vmId, newName, sqlVm.Value.expiration);
        
        if (!success)
        {
            await ctx.CreateResponseAsync("Error renaming vps, the name is probably already taken");
            return;
        }
        
        await ctx.CreateResponseAsync("VPS renamed to " + newName);
        await ctx.Log(new DiscordEmbedBuilder().WithTitle("VPS renamed").WithDescription($"{name} renamed to {newName}. Renamed by {ctx.User.Mention}").WithColor(Constants.COLOR_DEFAULT));
    }
}