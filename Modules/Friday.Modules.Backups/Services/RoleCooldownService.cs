using Friday.Common.Services;

namespace Friday.Modules.Backups.Services;

public class RoleCooldownService
{
    private DatabaseProvider _db;
    public const int RoleCountPerDay = 900;
    public RoleCooldownService(DatabaseProvider provider)
    {
        this._db = provider;
    }
    
    private async Task RemoveFromDb(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM bak_role_cooldown WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertDatabase(ulong guildId, int count)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO bak_role_cooldown (id, date, count) VALUES (@guildId, @date, @count)";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@date", DateTime.UtcNow);
        command.Parameters.AddWithValue("@count", count);
        await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateDatabase(ulong guildId, int count)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        // SUM count from bak_role_cooldown WHERE id = @guildId
        command.CommandText = "UPDATE bak_role_cooldown SET count = count + @count WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@count", count);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<bool> Exists(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM bak_role_cooldown WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    private async Task<(DateTime date, int count)> GetData(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM bak_role_cooldown WHERE id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return (DateTime.UtcNow, 0);
        
        return (reader.GetDateTime(1), reader.GetInt32(2));
    }

    public async Task AddToUsed(ulong guildId, int count)
    {
        if (!await Exists(guildId))
            await InsertDatabase(guildId, count);
        else
            await UpdateDatabase(guildId, count);
    }

    public async Task<(DateTime expiration, int used)> GetUsed(ulong guildId)
    {
        var (date, count) = await GetData(guildId);
        if (DateTime.UtcNow - date > TimeSpan.FromDays(1))
        {
            await RemoveFromDb(guildId);
            return (DateTime.UtcNow, 0);
        }
        
        return (date, count);
    }
}