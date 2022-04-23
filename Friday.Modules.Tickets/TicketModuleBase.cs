using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.Tickets.Entities;
using Friday.Modules.Tickets.Enums;
using Friday.Modules.Tickets.Helpers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Friday.Modules.Tickets;

public class TicketModuleBase : ModuleBase
{
    private readonly DatabaseProvider _db;
    private readonly DatabaseHelper _dbHelper;
    private readonly DiscordShardedClient _client;
    public TicketModuleBase(DatabaseProvider db, DiscordShardedClient client)
    {
        this._db = db;
        _client = client;
        this._dbHelper = new DatabaseHelper(db);
    }

    public override Task OnLoad()
    {
        _client.ComponentInteractionCreated += OnComponentInteractionCreated;
        return Task.CompletedTask;
    }

    private async Task OnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (e.User.IsBot) return;
        var ticketPanel = await GetTicketPanel(e.Message.Id);
        if (ticketPanel == null) return;
    }

    public async Task<ITicketPanel?> GetTicketPanel(ulong messageId)
    {
        if (!await _dbHelper.ExistsTicketPanel(messageId))
        {
            return null;
        }
        
        var data = await _dbHelper.GetTicketPanelData(messageId);
        if (data == null)
        {
            return null;
        }

        if (data.Value.type == TicketPanelType.Button)
        {
            var d = JsonSerializer.Deserialize<ButtonTicketPanel>(data.Value.jsonData);
            if (d == null)
            {
                return null;
            }
            await d.Load(_client);
            
            return d;
        }
        
        if (data.Value.type == TicketPanelType.Select)
        {
            var d = JsonSerializer.Deserialize<SelectTicketPanel>(data.Value.jsonData);

            return d;
        }
        
        return null;
    }

    public async Task<ITicketPanel?> GetTicketPanel(DiscordChannel ticket)
    {
        var result = await _dbHelper.ExistsTicketPanelFromTicket(ticket.Id);
        
        if (!result.exists)
        {
            return null;
        }
        
        return await GetTicketPanel(result.panelId);
    }

    public async Task CreateButtonTicketPanel(DiscordMessage message, DiscordChannel category,
        DiscordChannel? closedCategory, DiscordChannel? archivedCategory, DiscordRole[] supportRoles, 
        int ticketsPerUser, string namingFormat, string cpTitle, string cpDescription, string cpColor)
    {
        var panel = new ButtonTicketPanel
        {
            GuildId = message.Channel.Guild.Id,
            MessageId = message.Id,
            ChannelId = message.Channel.Id,
            CategoryId = category.Id,
            ClosedCategoryId = closedCategory?.Id,
            ArchivedCategoryId = archivedCategory?.Id,
            SupportRoleIds = supportRoles.Select(x => x.Id).ToArray(),
            TicketsPerUser = ticketsPerUser,
            NamingFormat = namingFormat,
            ControlPanelTitle = cpTitle,
            ControlPanelDescription = cpDescription,
            ControlPanelColor = cpColor
        };
        
        await _dbHelper.CreateTicketPanel(panel);
    }
}