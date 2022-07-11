using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace Miuex.Modules.Proxmox.Commands;

public class OtherCommands : ApplicationCommandModule
{

    [SlashCommand("help", "Help message")]
    public async Task HelpCommand(InteractionContext ctx)
    {
        DiscordInteractionResponseBuilder helpBuilder = new DiscordInteractionResponseBuilder();
        DiscordEmbedBuilder helpEmbed = new DiscordEmbedBuilder();
        helpEmbed.WithTitle("Help Categories");
        helpEmbed.WithDescription("Select a category using the buttons below.");
        helpEmbed.WithColor(Constants.COLOR_DEFAULT);

        List<DiscordComponent> helpButtons = new List<DiscordComponent>();
        
        helpButtons.Add(new DiscordButtonComponent(ButtonStyle.Primary, "vps", "VPS Commands"));

        if (ctx.IsAdmin())
        {
            helpButtons.Add(new DiscordButtonComponent(ButtonStyle.Primary, "nodes", "Nodes Commands"));
            helpButtons.Add(new DiscordButtonComponent(ButtonStyle.Primary, "admin", "Admin Commands"));
        }
        
        helpBuilder.AddEmbed(helpEmbed);
        helpBuilder.AddComponents(helpButtons);
        await ctx.CreateResponseAsync(helpBuilder);
        var result = await ctx.Client.GetInteractivity()
            .WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User, TimeSpan.FromSeconds(30));

        if (result.TimedOut)
        {
            await ctx.DeleteResponseAsync();
            return;
        }
        DiscordEmbedBuilder helpCategoryEmbed = new DiscordEmbedBuilder();
        if (result.Result.Id == "vps")
        {
            helpCategoryEmbed.WithTitle("VPS Commands");
            helpCategoryEmbed.WithColor(Constants.COLOR_DEFAULT);
            helpCategoryEmbed.AddField("Start VPS", "`/vps start <vps name>`");
            helpCategoryEmbed.AddField("Stop VPS", "`/vps stop <vps name>`");
            helpCategoryEmbed.AddField("Restart VPS", "`/vps restart <vps name>`");
            helpCategoryEmbed.AddField("List VPS", "`/vps list`");
            helpCategoryEmbed.AddField("VPS Status", "`/vps status <vps name>`");
            helpCategoryEmbed.AddField("VPS Rename", "`/vps rename <vps name> <new name>`");
        }else if (result.Result.Id == "nodes")
        {
            helpCategoryEmbed.WithTitle("Nodes Commands");
            helpCategoryEmbed.WithColor(Constants.COLOR_DEFAULT);
            helpCategoryEmbed.AddField("Node List", "`/nodes list`");
            helpCategoryEmbed.AddField("Node Info", "`/nodes info <node name>`");
            helpCategoryEmbed.AddField("Node Add", "`/nodes add`");
            helpCategoryEmbed.AddField("Node Remove", "`/nodes remove <node name>`");
            helpCategoryEmbed.AddField("Node Vms", "`/nodes vms <node name>`");
        }else if (result.Result.Id == "admin")
        {
            helpCategoryEmbed.WithTitle("Admin Commands");
            helpCategoryEmbed.WithColor(Constants.COLOR_DEFAULT);
            helpCategoryEmbed.AddField("Admin Link Vps", "`/admin linkvps <nodeId> <vmId> <@user> <name>`");
            helpCategoryEmbed.AddField("Admin Unlink Vps", "`/admin unlinkvps <name> <@user>`");
            helpCategoryEmbed.AddField("Admin List Vps", "`/admin listvps <@user>`");
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(helpCategoryEmbed));
    }
}