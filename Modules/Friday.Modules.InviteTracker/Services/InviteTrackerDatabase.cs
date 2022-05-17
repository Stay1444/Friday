using System.Text.Json;
using Friday.Common.Services;
using Friday.Modules.InviteTracker.Entities;

namespace Friday.Modules.InviteTracker.Services;

public class InviteTrackerDatabase
{
    private DatabaseProvider _db;
    internal InviteTrackerDatabase(DatabaseProvider provider)
    {
        this._db = provider;   
    }

    public async Task<bool> DoesConfigurationExist(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM invite_tracker WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    public async Task CreateConfiguration(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO invite_tracker (id, settings) VALUES (@guildId, @settings)";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@settings", JsonSerializer.Serialize(new InviteTrackerConfig()));
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<InviteTrackerConfig> GetConfiguration(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT settings FROM invite_tracker WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new Exception("No configuration found for guild");
        return JsonSerializer.Deserialize<InviteTrackerConfig>(reader.GetString(0)) ?? new InviteTrackerConfig();
    }
    
    public async Task UpdateConfiguration(ulong guildId, InviteTrackerConfig config)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE invite_tracker SET settings = @settings WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@settings", JsonSerializer.Serialize(config));
        await command.ExecuteNonQueryAsync();
    }
}