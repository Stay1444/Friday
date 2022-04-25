
using DSharpPlus;
using DSharpPlus.Entities;

namespace Friday.Modules.AntiRaid.Entities;

public class GuildAntiRaid
{
    private AntiRaidModule _antiRaidModule;
    public ulong GuildId { get; private set; }
    public AntiRaidSettings? Settings { get; private set; }

    internal GuildAntiRaid(AntiRaidModule module, ulong guildId)
    {
        GuildId = guildId;
        _antiRaidModule = module;
    }

    internal async Task LoadAsync()
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
    }

    public async Task SaveSettingsAsync()
    {
        var db = _antiRaidModule.AntiRaidDatabase;
        await db.UpdateSettingsForGuild(GuildId, Settings!);
    }

    private async Task<bool> ShouldSkip(DiscordGuild guild, DiscordUser user)
    {
        if (await _antiRaidModule.AntiRaidDatabase.IsUserInWhitelist(guild.Id, user.Id)) return true;
        var member = await guild.GetMemberAsync(user.Id);
        if (member == null) return true;
        if (member.Permissions.HasFlag(Permissions.Administrator) && Settings!.AdminsCanBypass) return true;
        if (member.Id == guild.OwnerId) return true;
        
        return false;
    }
    
    internal async Task ChannelCreated(DiscordClient client, DiscordGuild guild, DiscordUser createdBy, DiscordChannel createdChannel)
    {
        if (await ShouldSkip(guild,createdBy)) return;
    }

    internal async Task ChannelDeleted(DiscordClient client, DiscordGuild guild, DiscordUser deletedBy, DiscordChannel deletedChannel)
    {
        if (await ShouldSkip(guild,deletedBy)) return;
    }

    internal async Task ChannelUpdated(DiscordClient client, DiscordGuild guild, DiscordAuditLogChannelEntry update)
    {
        if (await ShouldSkip(guild,update.UserResponsible)) return;
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