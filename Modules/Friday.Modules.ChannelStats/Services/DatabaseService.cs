using DSharpPlus.Entities;
using Friday.Common.Services;
using Friday.Modules.ChannelStats.Entities;

namespace Friday.Modules.ChannelStats.Services;

public class DatabaseService
{
    private DatabaseProvider _db;
    internal DatabaseService(DatabaseProvider provider)
    {
        this._db = provider;
    }

    public async Task<GuildStatsChannel?> GetGuildStatsChannelAsync(ulong channelId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM gcs_channels WHERE id = @id";
        command.Parameters.AddWithValue("@id", channelId);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new GuildStatsChannel()
        {
            Id = reader.GetFieldValue<ulong>(0),
            Value = reader.GetFieldValue<string>(2)
        };
    }

    public async Task<List<GuildStatsChannel>> GetGuildStatsChannelsAsync(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM gcs_channels WHERE guild_id = @guild_id";
        command.Parameters.AddWithValue("@guild_id", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        var channels = new List<GuildStatsChannel>();
        while (await reader.ReadAsync())
        {
            channels.Add(new GuildStatsChannel()
            {
                Id = (ulong) reader.GetInt64(0),
                Value = reader.GetFieldValue<string>(2)
            });
        }
        return channels;
    }

    private async Task<bool> DoesChannelExistsAsync(ulong channelId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM gcs_channels WHERE id = @id";
        command.Parameters.AddWithValue("@id", channelId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    private async Task UpdateChannelAsync(ulong id, GuildStatsChannel data)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE gcs_channels SET value = @value WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@value", data.Value);
        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertChannelASync(ulong guildId, ulong channelId, string value)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO gcs_channels (id, guild_id, value) VALUES (@id, @guild_id, @value)";
        command.Parameters.AddWithValue("@id", channelId);
        command.Parameters.AddWithValue("@guild_id", guildId);
        command.Parameters.AddWithValue("@value", value);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateIdAsync(ulong oldId, ulong newId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE gcs_channels SET id = @newId WHERE id = @oldId";
        command.Parameters.AddWithValue("@oldId", oldId);
        command.Parameters.AddWithValue("@newId", newId);
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task InsertOrUpdate(ulong guildId, ulong channelId, string value)
    {
        if (await DoesChannelExistsAsync(channelId))
            await UpdateChannelAsync(channelId, new GuildStatsChannel()
            {
                Id = channelId,
                Value = value
            });
        else
            await InsertChannelASync(guildId, channelId, value);
    }
    
    public Task InsertOrUpdateAsync(DiscordChannel channel, GuildStatsChannel data)
        => InsertOrUpdate(channel.Guild.Id, channel.Id, data.Value);
    
    public Task InsertOrUpdate(DiscordChannel channel, string value)
        => InsertOrUpdate(channel.Guild.Id, channel.Id, value);

    public async Task DeleteAsync(ulong id)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM gcs_channels WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteGuildAsync(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM gcs_channels WHERE guild_id = @guild_id";
        command.Parameters.AddWithValue("@guild_id", guildId);
        await command.ExecuteNonQueryAsync();
    }
}