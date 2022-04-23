using DSharpPlus.Entities;

namespace Friday.Modules.Tickets.Entities;

public interface ITicketPanel
{
    public DiscordMessage Message { get; }
    public DiscordGuild Guild { get; }
    public DiscordRole[] SupportRoles { get; }
    public int TicketsPerUser { get; }
    public Task<Ticket[]> GetTickets();
}