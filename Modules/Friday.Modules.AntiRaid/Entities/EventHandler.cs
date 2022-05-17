using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Friday.Common;

namespace Friday.Modules.AntiRaid.Entities;

internal class EventHandler
{

    private AntiRaidModule _module;
    public EventHandler(AntiRaidModule module)
    {
        _module = module;
    }

    public void Register()
    {
        _module.Client.GuildDownloadCompleted += GuildDownloadCompleted;
        _module.Client.GuildCreated += GuildCreated;
        _module.Client.GuildDeleted += GuildDeleted;
        _module.Client.ChannelDeleted += ChannelDeleted;
        _module.Client.ChannelCreated += ChannelCreated;
        _module.Client.ChannelUpdated += ChannelUpdated;
        _module.Client.GuildRoleCreated += RoleCreated;
        _module.Client.GuildRoleDeleted += RoleDeleted;
        _module.Client.GuildRoleUpdated += RoleUpdated;
        _module.Client.GuildMemberRemoved += MemberRemoved;
        _module.Client.GuildMemberUpdated += MemberUpdated;
        _module.Client.MessageCreated += MessageCreated;
    }

    

    public void Unregister()
    {
        _module.Client.GuildDownloadCompleted -= GuildDownloadCompleted;
        _module.Client.GuildCreated -= GuildCreated;
        _module.Client.GuildDeleted -= GuildDeleted;
        _module.Client.ChannelDeleted -= ChannelDeleted;
        _module.Client.ChannelCreated -= ChannelCreated;
        _module.Client.ChannelUpdated -= ChannelUpdated;
        _module.Client.GuildRoleCreated -= RoleCreated;
        _module.Client.GuildRoleDeleted -= RoleDeleted;
        _module.Client.GuildRoleUpdated -= RoleUpdated;
        _module.Client.GuildMemberRemoved -= MemberRemoved;
        _module.Client.GuildMemberUpdated -= MemberUpdated;
        _module.Client.MessageCreated -= MessageCreated;
    }
    
    private async Task GuildDownloadCompleted(DiscordClient client , GuildDownloadCompletedEventArgs e)
    {
        foreach (var downloadedGuild in e.Guilds)
        {
            await _module.GetAntiRaid(downloadedGuild.Value);
        }
    }

    private async Task GuildCreated(DiscordClient client, GuildCreateEventArgs e)
    {
        await _module.GetAntiRaid(e.Guild);
    }

    private Task GuildDeleted(DiscordClient client, GuildDeleteEventArgs e)
    {
        _module.Guilds.Remove(e.Guild.Id);
        return Task.CompletedTask;
    }

    private async Task ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
    {
        var guildAntiRaid = await _module.GetAntiRaid(e.Guild.Id);

        var channelDeletedInfo = await e.GetChannelDeletedInfo();

        if (channelDeletedInfo is not null)
        {
            await guildAntiRaid.ChannelDeleted(client, e.Guild, channelDeletedInfo.UserResponsible, e.Channel);
            await guildAntiRaid.UpdateChildren();
        }
    }
    
    private async Task ChannelCreated(DiscordClient client, ChannelCreateEventArgs e)
    {
        var guildAntiRaid = await _module.GetAntiRaid(e.Guild.Id);
        
        var channelCreatedInfo = await e.GetChannelCreatedInfo();
        
        if (channelCreatedInfo is not null)
        {
            await guildAntiRaid.ChannelCreated(client, e.Guild, channelCreatedInfo.UserResponsible, e.Channel);
            await guildAntiRaid.UpdateChildren();

        }
    }
    
    private async Task ChannelUpdated(DiscordClient client, ChannelUpdateEventArgs e)
    {
        var guildAntiRaid = await _module.GetAntiRaid(e.Guild.Id);
        
        var channelUpdatedInfo = await e.GetChannelUpdatedInfo();
        
        if (channelUpdatedInfo is not null)
        {
            await guildAntiRaid.ChannelUpdated(client, e.Guild, channelUpdatedInfo);
            await guildAntiRaid.UpdateChildren();

        }
    }
    
    private async Task RoleDeleted(DiscordClient client, GuildRoleDeleteEventArgs e)
    {
        var guildAntiRaid = await _module.GetAntiRaid(e.Guild.Id);
        
    }
    
    private async Task RoleCreated(DiscordClient client, GuildRoleCreateEventArgs e)
    {
        
    }
    
    private async Task RoleUpdated(DiscordClient client, GuildRoleUpdateEventArgs e)
    {
        
    }

    private async Task MemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
    {
        var guildAntiRaid = await _module.GetAntiRaid(e.Guild.Id);
        
        var banInfo = await e.GetBanInfo();
        if (banInfo is not null)
        {
            await guildAntiRaid.MemberBanned(client, e.Guild, banInfo.UserResponsible);
            return;
        }
        
        var kickInfo = await e.GetKickInfo();
        if (kickInfo is not null)
        {
            await guildAntiRaid.MemberKicked(client, e.Guild, kickInfo.UserResponsible);
            return;
        }
    }
    
    private async Task MemberUpdated(DiscordClient client, GuildMemberUpdateEventArgs e)
    {
        
    }
    
    private async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Message.WebhookMessage)
            return;
        if (e.Channel.IsPrivate) return;
        var guildAntiRaid = await _module.GetAntiRaid(e.Guild.Id);

        if (guildAntiRaid.GuildChannelMessages.ContainsKey(e.Channel.Id))
        {
            guildAntiRaid.GuildChannelMessages[e.Channel.Id].Add(e.Message);
        }else
        {
            guildAntiRaid.GuildChannelMessages.Add(e.Channel.Id, new List<DiscordMessage>() {e.Message});
        }
        
        if (guildAntiRaid.GuildChannelMessages[e.Channel.Id].Count > 100)
        {
            guildAntiRaid.GuildChannelMessages[e.Channel.Id].RemoveAt(0);
        }
        
    }
}