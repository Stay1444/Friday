
using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Serilog;

namespace Friday.Modules.AntiRaid.Entities;

public class GuildAntiRaid
{
    private AntiRaidModule _antiRaidModule;
    private DiscordGuild? _guild;
    private DiscordClient? _client;
    public ulong GuildId { get; private set; }
    public AntiRaidSettings? Settings { get; private set; }
    
    private readonly Dictionary<ulong, ChannelCreationHandler> _channelCreationHandlers = new Dictionary<ulong, ChannelCreationHandler>();
    private readonly Dictionary<ulong, ChannelDeletionHandler> _channelDeletionHandlers = new Dictionary<ulong, ChannelDeletionHandler>();
    private readonly Dictionary<ulong, ChannelUpdateHandler> _channelUpdateHandlers = new Dictionary<ulong, ChannelUpdateHandler>();
    
    internal readonly Dictionary<ulong, List<ulong>> GuildChannelChildren = new Dictionary<ulong, List<ulong>>();
    internal readonly Dictionary<ulong, List<DiscordMessage>> GuildChannelMessages = new Dictionary<ulong, List<DiscordMessage>>();
    internal GuildAntiRaid(AntiRaidModule module, ulong guildId)
    {
        GuildId = guildId;
        _antiRaidModule = module;
    }

    internal async Task LoadAsync(DiscordShardedClient client)
    {
        var db = _antiRaidModule.AntiRaidDatabase;
        if (await db.SettingsExistsForGuild(GuildId))
        {
            Settings = await db.GetSettingsForGuild(GuildId);
        }else
        {
            Settings = new AntiRaidSettings();
            await db.InsertSettingsForGuild(GuildId, Settings);
        }

        var guild = await client.GetGuildAsync(GuildId);
        Log.Information("[AntiRaid] Loaded guild {Name} ({Id})", guild!.Name, guild.Id);
        _guild = guild;
        _client = client.GetClient(guild);
        await UpdateChildren();
    }

    internal Task UpdateChildren()
    {
        Log.Debug("Updating children for guild {0}", GuildId);
        GuildChannelChildren.Clear();
        //var channels = await _guild!.GetChannelsAsync();
        foreach (var channel in _guild!.Channels.Values)
        {
            GuildChannelChildren.Add(channel.Id, new List<ulong>());
            GuildChannelChildren[channel.Id].AddRange(_guild!.Channels.Values.Where(x => x.Parent?.Id == channel.Id).Select(x => x.Id));
        }
        
        return Task.CompletedTask;
    }

    public async Task SaveSettingsAsync()
    {
        var db = _antiRaidModule.AntiRaidDatabase;
        await db.UpdateSettingsForGuild(GuildId, Settings!);
    }

    private async Task<bool> ShouldSkip(DiscordGuild guild, DiscordUser user)
    {
        if (!Settings!.Enabled) return true;
        if (user.Id == _client!.CurrentUser.Id)
            return true;
        if (await _antiRaidModule.AntiRaidDatabase.IsUserInWhitelist(guild.Id, user.Id)) return true;
        var member = await guild.GetMemberAsync(user.Id);
        if (member == null) return true;
        if (member.Permissions.HasFlag(Permissions.Administrator) && Settings!.AdminsCanBypass) return true;
        if (member.Id == guild.OwnerId) return true;
        
        return false;
    }

    internal bool ShouldLog(DiscordGuild guild, out DiscordChannel? logChannel)
    {
        logChannel = null;
        if (!Settings!.Logs.Enabled) return false;
        if (Settings!.Logs.ChannelId == 0) return false;
        if (guild.Channels.ContainsKey(Settings!.Logs.ChannelId))
        {
            logChannel = guild.Channels[Settings!.Logs.ChannelId];
            return true;
        }
        
        return false;
    }
    
    internal async Task ChannelCreated(DiscordClient client, DiscordGuild guild, DiscordUser createdBy, DiscordChannel createdChannel)
    {
        if (!Settings!.CreateChannels.Enabled) return;
        if (await ShouldSkip(guild,createdBy)) return;
        var member = await guild.GetMemberAsync(createdBy.Id);
        if (member == null) return;

        if (!_channelCreationHandlers.ContainsKey(createdBy.Id))
        {
            _channelCreationHandlers.Add(createdBy.Id, new ChannelCreationHandler(this, guild, createdBy));
        }

        await _channelCreationHandlers[createdBy.Id].Handle(createdChannel);
    }

    internal async Task ChannelDeleted(DiscordClient client, DiscordGuild guild, DiscordUser deletedBy, DiscordChannel deletedChannel)
    {
        if (!Settings!.DeleteChannels.Enabled) return;
        if (await ShouldSkip(guild,deletedBy)) return;
        var member = await guild.GetMemberAsync(deletedBy.Id);
        if (member == null) return;
        
        if (!_channelDeletionHandlers.ContainsKey(deletedBy.Id))
        {
            _channelDeletionHandlers.Add(deletedBy.Id, new ChannelDeletionHandler(this, guild, deletedBy));
        }
        
        await _channelDeletionHandlers[deletedBy.Id].Handle(deletedChannel);
    }

    internal async Task ChannelUpdated(DiscordClient client, DiscordGuild guild, DiscordAuditLogChannelEntry update)
    {
        if (!Settings!.UpdateChannels.Enabled) return;
        if (await ShouldSkip(guild,update.UserResponsible)) return;
        var member = await guild.GetMemberAsync(update.UserResponsible.Id);
        if (member == null) return;
        
        if (!_channelUpdateHandlers.ContainsKey(update.UserResponsible.Id))
        {
            _channelUpdateHandlers.Add(update.UserResponsible.Id, new ChannelUpdateHandler(this, guild, update.UserResponsible));
        }
        
        await _channelUpdateHandlers[update.UserResponsible.Id].Handle(update);
    }

    internal async Task RoleCreated()
    {
        
    }
    
    internal async Task RoleDeleted()
    {
        
    }
    
    internal async Task RoleUpdated()
    {
        
    }
    
    internal async Task RoleGranted()
    {
        
    }
    
    internal async Task RoleRevoked()
    {
        
    }
    
    internal async Task MemberKicked(DiscordClient client, DiscordGuild guild, DiscordUser kickedBy)
    {
        
    }
    
    internal async Task MemberBanned(DiscordClient client, DiscordGuild guild, DiscordUser bannedBy)
    {
        
    }
    
}