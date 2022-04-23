using System.Text.Json.Serialization;
using DSharpPlus.Entities;

namespace Friday.Modules.Tickets.Entities;

public class SelectTicketPanel : ITicketPanel
{
    [JsonIgnore]
    public DiscordMessage Message { get; private set; }
    [JsonIgnore]
    public DiscordGuild Guild { get; private set; }
    [JsonIgnore]
    public DiscordRole[] SupportRoles { get; private set; }

    public int TicketsPerUser { get; set; }
    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; } 
    public List<(ulong category, string namingFormat, ulong[] supportRoles)> Options { get; set; }
    
    public Task<Ticket?> Open(DiscordUser user, int option)
    {
        return null;
    }

    public async Task<Ticket[]> GetTickets()
    {
        return Array.Empty<Ticket>();
    }
}