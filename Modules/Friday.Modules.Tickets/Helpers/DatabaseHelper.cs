using System.Text.Json;
using Friday.Common.Services;
using Friday.Modules.Tickets.Entities;
using Friday.Modules.Tickets.Enums;

namespace Friday.Modules.Tickets.Helpers;

public class DatabaseHelper
{
    private DatabaseProvider db;
    public DatabaseHelper(DatabaseProvider databaseProvider)
    {
        this.db = databaseProvider;
    }

    public async Task<bool> ExistsTicketPanel(ulong messageId)
    {
        await using var con = db.GetConnection();
        await con.OpenAsync();
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM fmt_ticket_panel WHERE message_id = @message_id";
        cmd.Parameters.AddWithValue("@message_id", messageId);
        var result = await cmd.ExecuteReaderAsync();
        return result.HasRows;
    }

    public async Task<(bool exists, ulong panelId)> ExistsTicketPanelFromTicket(ulong channelId)
    {
        await using var con = db.GetConnection();
        await con.OpenAsync();
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT panel_id FROM fmt_ticket WHERE channel_id = @channel_id";
        cmd.Parameters.AddWithValue("@channel_id", channelId);
        var result = await cmd.ExecuteReaderAsync();
        
        if (!result.HasRows)
            return (false, 0);
        
        await result.ReadAsync();
        return (true, (ulong)result.GetInt64(0));
    }
    
    public async Task<(ulong messageId, ulong channelId, TicketPanelType type, string jsonData)?> GetTicketPanelData(ulong panelId)
    {
        await using var con = db.GetConnection();
        await con.OpenAsync();
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT message_id, channel_id, type, json_data FROM fmt_ticket_panel WHERE message_id = @panel_id";
        cmd.Parameters.AddWithValue("@panel_id", panelId);
        var result = await cmd.ExecuteReaderAsync();
        if (!result.HasRows)
            return null;
        await result.ReadAsync();
        
        return ((ulong)result.GetInt64(0), (ulong)result.GetInt64(1), (TicketPanelType)result.GetInt32(2), result.GetString(3));
    }

    public async Task CreateTicketPanel(ButtonTicketPanel panel)
    {
        await using var con = db.GetConnection();
        await con.OpenAsync();
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO fmt_ticket_panel (message_id, channel_id, type, json_data) VALUES (@message_id, @channel_id, @type, @json_data)";
        cmd.Parameters.AddWithValue("@message_id", panel.MessageId);
        cmd.Parameters.AddWithValue("@channel_id", panel.ChannelId);
        cmd.Parameters.AddWithValue("@type", TicketPanelType.Button);
        cmd.Parameters.AddWithValue("@json_data", JsonSerializer.Serialize(panel));
        await cmd.ExecuteNonQueryAsync();
    }
}