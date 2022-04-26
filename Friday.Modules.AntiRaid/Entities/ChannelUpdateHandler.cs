using DSharpPlus.Entities;

namespace Friday.Modules.AntiRaid.Entities;

public class ChannelUpdateHandler
{
    private GuildAntiRaid _guildAntiRaid;
    private DiscordGuild _guild;
    private DiscordUser _user;
    private List<DiscordChannel> _updatedChannels = new List<DiscordChannel>();
    private DateTime? _lastUpdate;
    internal ChannelUpdateHandler(GuildAntiRaid guildAntiRaid, DiscordGuild guild, DiscordUser user)
    {
        _guildAntiRaid = guildAntiRaid;
        _guild = guild;
        _user = user;
    }

    public async Task Handle(DiscordAuditLogChannelEntry e)
    {
        
    }

}