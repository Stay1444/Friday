using DSharpPlus;
using DSharpPlus.EventArgs;

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
        
    }
    
    private async Task ChannelCreated(DiscordClient client, ChannelCreateEventArgs e)
    {
        
    }
    
    private async Task ChannelUpdated(DiscordClient client, ChannelUpdateEventArgs e)
    {
        
    }
    
    private async Task RoleDeleted(DiscordClient client, GuildRoleDeleteEventArgs e)
    {
        
    }
    
    private async Task RoleCreated(DiscordClient client, GuildRoleCreateEventArgs e)
    {
        
    }
    
    private async Task RoleUpdated(DiscordClient client, GuildRoleUpdateEventArgs e)
    {
        
    }

    private async Task MemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
    {
        
    }
}