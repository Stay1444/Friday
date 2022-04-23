using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common.Services;

namespace Friday.Modules.Tickets.Entities;

public class Ticket
{   
    public DiscordUser Owner { get; private set; }
    public DiscordChannel Channel { get; private set; }
    public ITicketPanel Panel { get; private set; }
    
    private DatabaseProvider _db;
    private DiscordClient _client;
    private LanguageProvider _lang;
    
    public Ticket(DiscordUser owner, DiscordChannel ticket, ITicketPanel ticketPanel, LanguageProvider lang, DiscordClient client, DatabaseProvider db)
    {
        this.Owner = owner;
        this.Channel = ticket;
        this.Panel = ticketPanel;
        _lang = lang;
        _client = client;
        _db = db;
    }
    
    public async Task Close()
    {
        await Task.Delay(0);
    }
    
    public async Task Archive()
    {
        await Task.Delay(0);
    }
    
    public async Task SendMessageAs(DiscordUser user, string message)
    {
        throw new NotImplementedException();
    }
    
    
}