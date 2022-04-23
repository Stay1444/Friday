using System.Text.Json.Serialization;
using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
#pragma warning disable CS8618

namespace Friday.Modules.Tickets.Entities;

public class ButtonTicketPanel : ITicketPanel
{
    [JsonIgnore]
    public DiscordMessage Message { get; private set; }
    [JsonIgnore]
    public DiscordGuild Guild { get; private set; }
    [JsonIgnore]
    public DiscordRole[] SupportRoles { get; private set; }

    public int TicketsPerUser { get; set; }
    public ulong MessageId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; } 
    public ulong CategoryId { get; set; }
    public ulong[] SupportRoleIds { get; set; } = new ulong[0];
    public string NamingFormat { get; set; } = "Ticket #{0}";
    public int TimesUsed { get; set; } = 0;
    public ulong? ClosedCategoryId { get; set; }
    public ulong? ArchivedCategoryId { get; set; }
    public string? ControlPanelTitle { get; set; }
    public string? ControlPanelDescription { get; set; }
    public string? ControlPanelColor { get; set; }
    private DiscordClient _client { get; set; }
    
    public Task<Ticket> Open(DiscordUser user)
    {
        throw new NotImplementedException();
    }

    public Task<Ticket[]> GetTickets()
    {
        throw new NotImplementedException();
    }

    public async Task Load(DiscordShardedClient shardedClient)
    {
        var guild = await shardedClient.GetGuildAsync(GuildId);
        if (guild == null)
            throw new Exception("Guild not found");
        Guild = guild;
        _client = shardedClient.GetClient(guild);
        
        var channel = await _client.GetChannelAsync(ChannelId);
        if (channel == null)
            throw new Exception("Channel not found");
        
        var message = await channel.GetMessageAsync(MessageId);
        if (message == null)
            throw new Exception("Message not found");
        Message = message;

        var supportRoles = new List<DiscordRole>();
        foreach (var roleId in SupportRoleIds)
        {
            var role = guild.GetRole(roleId);
            if (role == null)
                throw new Exception("Role not found");
            supportRoles.Add(role);
        }
        
        SupportRoles = supportRoles.ToArray();
    }
}