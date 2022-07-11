using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Humanizer;
using Miuex.Modules.Proxmox.Attributes;
using Miuex.Modules.Proxmox.Models;
using Miuex.Modules.Proxmox.Services;

namespace Miuex.Modules.Proxmox.Commands;

[SlashCommandGroup("nodes", "Nodes command group")]
public class NodeCommands : ApplicationCommandModule
{
    private APIService _apiService;
    public NodeCommands(ProxmoxModule apiService)
    {
        this._apiService = apiService.Api;
    }
    
    [SlashCommand("list", "List all nodes"), RequireAdminRole]
    public async Task NodeList(InteractionContext ctx)
    {
        await ctx.DeferAsync();
        var nodes = await _apiService.GetNodesAsync();

        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("Node list");
        embed.WithColor(Constants.COLOR_DEFAULT);
        
        var nodesList = nodes.ToList().OrderBy(x => x.Id).ToList();
        if (nodesList.Count == 0)
        {
            embed.AddField("Nodes", "No nodes found");
        }
        else
        {
            List<DiscordEmbed> pages = new List<DiscordEmbed>();

            while (nodesList.Any())
            {
                var page = new DiscordEmbedBuilder();
                page.WithColor(Constants.COLOR_DEFAULT);
                page.WithTitle("Node list");
                for (int i = 0; i < nodesList.Count && i < 9; i++)
                {
                    string endpoint = nodesList[i].Endpoint;
                    if (endpoint.Trim() == "")
                    {
                        endpoint = nodesList[i].Name;
                    }
                    page.AddField(nodesList[i].Name,
                        $@"```ml
Id     : {nodesList[i].Id}
Name   : {nodesList[i].Name}
Status : {nodesList[i].Status}
CPU    : {nodesList[i]?.Resources?.Cpu.Round(2) * 100?? 0}% 
Memory : {nodesList[i]?.Resources?.Memory.Round(2) * 100 ?? 0}%
```", true
                    );
                    page.WithFooter($"Page {pages.Count + 1}/{Math.Ceiling(nodesList.Count / 9.0)} | {nodesList.Count} Nodes");
                }
                
                pages.Add(page);
                nodesList.RemoveRange(0, page.Fields.Count);
            }

            if (pages.Count > 1)
            {
                await ctx.SendPaginatedMessageAsync(pages);
            }else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(pages.First()));
            }
        }
        
    }

    [SlashCommand("add", "Adds a node"), RequireAdminRole]
    public async Task AddNode(InteractionContext ctx)
    {
        try
        {
            var response = new DiscordInteractionResponseBuilder();

            response
                .WithTitle("Add a node")
                .WithCustomId("add-node")
                .AddComponents(new TextInputComponent(label: "Name", customId: "name", placeholder: "PVENode",
                    max_length: 32))
                .AddComponents(new TextInputComponent("Endpoint", "endpoint", "http://10.0.10.15:8006", required: true,
                    max_length: 64))
                .AddComponents(new TextInputComponent("API Key", "api-key",
                    "user@pam!name=00000000-0000-0000-0000-000000000000", required: true, max_length: 128));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, response);
            var result = await ctx.Client.GetInteractivity()
                .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == ctx.User.Id);

            if (result.TimedOut)
                return;
            
            string name = result.Result.Values["name"];
            string endpoint = result.Result.Values["endpoint"];
            string apiKey = result.Result.Values["api-key"];

            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate, null);

            var apiResult = await _apiService.AddNodeAsync(name, endpoint, apiKey);

            if (apiResult.success)
            {
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Node added").WithColor(Constants.COLOR_DEFAULT).WithDescription("Node added successfully")));
                await ctx.Log(new DiscordEmbedBuilder().WithTitle("Node added").WithColor(Constants.COLOR_DEFAULT)
                    .WithDescription($"Name: {name}\nEndpoint: {endpoint}\nAdded by: {ctx.User.Username}"));
            }
            else
            {
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error").WithColor(Constants.COLOR_DEFAULT).WithDescription(apiResult.message)));
            }
        }catch(BadRequestException e)
        {
            Console.WriteLine(e.Errors);
        }
    }
    
    [SlashCommand("remove", "Removes a node"), RequireAdminRole]
    public async Task RemoveNode(InteractionContext ctx)
    {
        try
        {
            var response = new DiscordInteractionResponseBuilder();

            response
                .WithTitle("Remove a node")
                .WithCustomId("remove-node")
                .AddComponents(new TextInputComponent(label: "Name", customId: "name", placeholder: "PVENode",
                    max_length: 32));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, response);
            var result = await ctx.Client.GetInteractivity()
                .WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == ctx.User.Id);

            if (result.TimedOut)
                return;
            
            string name = result.Result.Values["name"];

            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate, null);

            var nodes = await _apiService.GetNodesAsync();
            var node = nodes.FirstOrDefault(x => x.Name == name);

            if (node == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error").WithColor(Constants.COLOR_DEFAULT).WithDescription("Node not found")));
                return;
            }
            
            var apiResult = await _apiService.RemoveNodeAsync(node.Id);
            
            if (apiResult)
            {
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Node removed").WithColor(Constants.COLOR_DEFAULT).WithDescription("Node removed successfully")));
                
                await ctx.Log(new DiscordEmbedBuilder().WithTitle("Node removed").WithColor(Constants.COLOR_DEFAULT)
                    .WithDescription($"Name: {name}\nRemoved by: {ctx.User.Username}"));
                
            }
            else
            {
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error").WithColor(Constants.COLOR_DEFAULT).WithDescription("Node not removed")));
            }
        }catch(BadRequestException e)
        {
            Console.WriteLine(e.Errors);
        }
    }
    

    [SlashCommand("info", "Get node info"), RequireAdminRole]
    public async Task NodeInfo(InteractionContext ctx, [Option("name", "Node name")] string name)
    {
        await ctx.DeferAsync();
        
        var nodes = await _apiService.GetNodesAsync();
        var node = nodes.FirstOrDefault(x => x.Name == name);
        
        if (node == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Node not found"));
            return;
        }

        var embed = new DiscordEmbedBuilder();
        embed.WithTitle(node.Name);
        embed.WithColor(Constants.COLOR_DEFAULT);
        embed.AddField("Status", node.Status.ToString());
        embed.AddField("VM Count", node.VmCount.ToString());
        if (node.Resources is not null)
        {
            var memoryInGb = node.Resources.UsedMemory / 1024 / 1024 / 1024;
            var totalMemoryInGb = node.Resources.TotalMemory / 1024 / 1024 / 1024;
            embed.AddField("Resources", $"CPU: {Math.Round(node.Resources.Cpu * 100, 1)}%\nRAM: {Math.Round(node.Resources.Memory * 100,1)}% ({memoryInGb}GB / {totalMemoryInGb}GB)");
        }
        
        embed.AddField("Uptime", node.Uptime.Humanize());

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

    }

    [SlashCommand("vms", "List all VMs"), RequireAdminRole]
    public async Task NodeListVms(InteractionContext ctx, [Option("name", "Node name")] string name)
    {
        await ctx.DeferAsync();
        var nodes = await _apiService.GetNodesAsync();
        var node = nodes.FirstOrDefault(x => x.Name == name);
        
        if (node == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Node not found"));
            return;
        }
        
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle(node.Name);
        embed.WithColor(Constants.COLOR_DEFAULT);
        embed.AddField("Status", node.Status.ToString());
        
        var vms = await _apiService.GetVmsAsync(node.Id);
        var vmsList = vms.ToList().OrderBy(x => x.Name).ToList();
        if (vms.Length == 0)
        {
            embed.AddField("VMs", "No VMs found");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
        else
        {
            List<DiscordEmbed> pages = new List<DiscordEmbed>();
            var redBall = DiscordEmoji.FromName(ctx.Client, ":red_circle:");
            var greenBall = DiscordEmoji.FromName(ctx.Client, ":green_circle:");
            while (vmsList.Any())
            {
                var page = new DiscordEmbedBuilder();
                page.WithColor(Constants.COLOR_DEFAULT);
                page.WithTitle(node.Name + " VMs");
                string description = "";
                int added = 0;
                for (int i = 0; i < vmsList.Count && i < 9; i++)
                {
                    added++;
                    description += $"{vmsList[i].Name} - {(vmsList[i].Status == PveStatus.Running ? greenBall : redBall)}\n";
                    page.WithFooter($"Page {pages.Count + 1}/{Math.Ceiling(vmsList.Count / 9.0)} | {vmsList.Count} VMs");
                }
                page.WithDescription(description);
                pages.Add(page);
                vmsList.RemoveRange(0, added);
            }

            if (pages.Count > 1)
            {
                await ctx.SendPaginatedMessageAsync(pages);
            }else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(pages.First()));
            }
        }
        
    }
}