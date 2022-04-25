using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Friday.Common;

public static class AuditLogExtensions
{
    public static async Task<DiscordAuditLogBanEntry?> GetBanInfo(this GuildMemberRemoveEventArgs e)
    {
        var auditLogs = await e.Guild.GetAuditLogsAsync(5);

        foreach (var auditLogEntry in auditLogs)
        {
            if (auditLogEntry is DiscordAuditLogBanEntry banEntry)
            {
                if (banEntry.Target.Id == e.Member.Id)
                {
                    return banEntry;
                }
            }
        }
        
        return null;
    }
    
    public static async Task<DiscordAuditLogKickEntry?> GetKickInfo(this GuildMemberRemoveEventArgs e)
    {
        var auditLogs = await e.Guild.GetAuditLogsAsync(5);

        foreach (var auditLogEntry in auditLogs)
        {
            if (auditLogEntry is DiscordAuditLogKickEntry kickEntry)
            {
                if (kickEntry.Target.Id == e.Member.Id)
                {
                    return kickEntry;
                }
            }
        }
        
        return null;
    }

    public static async Task<DiscordAuditLogChannelEntry?> GetChannelDeletedInfo(this ChannelDeleteEventArgs e)
    {
        var auditLogs = await e.Guild.GetAuditLogsAsync(5);
        
        foreach (var auditLogEntry in auditLogs)
        {
            if (auditLogEntry is DiscordAuditLogChannelEntry channelEntry)
            {
                if (channelEntry.ActionType == AuditLogActionType.ChannelDelete &&
                    channelEntry.Target.Id == e.Channel.Id)
                {
                    return channelEntry;
                }
            }
        }
        
        return null;
    }
    
    public static async Task<DiscordAuditLogChannelEntry?> GetChannelCreatedInfo(this ChannelCreateEventArgs e)
    {
        var auditLogs = await e.Guild.GetAuditLogsAsync(5);
        
        foreach (var auditLogEntry in auditLogs)
        {
            if (auditLogEntry is DiscordAuditLogChannelEntry channelEntry)
            {
                if (channelEntry.ActionType == AuditLogActionType.ChannelCreate &&
                    channelEntry.Target.Id == e.Channel.Id)
                {
                    return channelEntry;
                }
            }
        }
        
        return null;
    }
    
    public static async Task<DiscordAuditLogChannelEntry?> GetChannelUpdatedInfo(this ChannelUpdateEventArgs e)
    {
        var auditLogs = await e.Guild.GetAuditLogsAsync(5);
        
        foreach (var auditLogEntry in auditLogs)
        {
            if (auditLogEntry is DiscordAuditLogChannelEntry channelEntry)
            {
                if (channelEntry.ActionType == AuditLogActionType.ChannelUpdate &&
                    channelEntry.Target.Id == e.ChannelBefore.Id)
                {
                    return channelEntry;
                }
            }
        }
        
        return null;
    }
    
}