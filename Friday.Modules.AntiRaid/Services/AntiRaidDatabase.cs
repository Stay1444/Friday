using System.Text.Json;
using Friday.Common.Services;
using Friday.Modules.AntiRaid.Entities;

namespace Friday.Modules.AntiRaid.Services;

internal class AntiRaidDatabase
{
    private DatabaseProvider _databaseProvider;
    public AntiRaidDatabase(DatabaseProvider databaseProvider)
    {
        _databaseProvider = databaseProvider;
    }

    
    public async Task<AntiRaidSettings> GetSettingsForGuild(ulong guildId)
    {
        if (!await SettingsExistsForGuild(guildId))
        {
            throw new KeyNotFoundException("Settings not found for guild");
        }
        
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT settings FROM antiraid_settings WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return JsonSerializer.Deserialize<AntiRaidSettings>(reader.GetString(0)) ?? throw new KeyNotFoundException("Settings not found for guild");
    }
    
    public async Task<bool> SettingsExistsForGuild(ulong guildId)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT settings FROM antiraid_settings WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    public async Task InsertSettingsForGuild(ulong guildId, AntiRaidSettings settings)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO antiraid_settings (id, settings) VALUES (@guildId, @settings)";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@settings", JsonSerializer.Serialize(settings));
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task UpdateSettingsForGuild(ulong guildId, AntiRaidSettings settings)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE antiraid_settings SET settings = @settings WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@settings", JsonSerializer.Serialize(settings));
        await command.ExecuteNonQueryAsync();
    }
}