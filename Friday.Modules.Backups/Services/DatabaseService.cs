using System.Data;
using System.Text.Json;
using Friday.Common.Services;
using Friday.Modules.Backups.Entities;

namespace Friday.Modules.Backups.Services;

internal class DatabaseService
{
    private readonly DatabaseProvider _databaseProvider;
    public DatabaseService(DatabaseProvider provider)
    {
        _databaseProvider = provider;
    }

    public async Task<(Backup backup, string code, ulong owner)?> GetBackupAsync(long id)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM bak_backups WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;
        
        var code = reader.GetString(1);
        var backupJson = reader.GetString(2);
        var owner = (ulong) reader.GetInt64(3);
        var backup = JsonSerializer.Deserialize<Backup>(backupJson);
        
        if (backup == null)
            return null;
        
        return (backup, code, owner);
    }

    public async Task<(long id, Backup backup, string code, ulong owner)?> GetBackupAsync(string code)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM bak_backups WHERE code = @code";
        command.Parameters.AddWithValue("@code", code);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;
        
        var id = reader.GetInt64(0);
        var backupJson = reader.GetString(2);
        var owner = (ulong) reader.GetInt64(3);
        var backup = JsonSerializer.Deserialize<Backup>(backupJson);
        
        if (backup == null)
            return null;
        
        return (id, backup, code, owner);
    }

    public async Task<List<(long id, Backup backup, string code, ulong owner)>> GetBackupsAsync(ulong owner)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM bak_backups WHERE owner = @owner";
        command.Parameters.AddWithValue("@owner", owner);
        await using var reader = await command.ExecuteReaderAsync();
        var backups = new List<(long id, Backup backup, string code, ulong owner)>();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var code = reader.GetString(1);
            var backupJson = reader.GetString(2);
            var backup = JsonSerializer.Deserialize<Backup>(backupJson);
            if (backup == null)
                continue;
            backups.Add((id, backup, code, owner));
        }
        
        return backups;
    }
    
    public async Task InsertBackupAsync(long id, Backup backup, string code, ulong owner)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO bak_backups (id, code, data, owner) VALUES (@id, @code, @backup, @owner)";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@backup", JsonSerializer.Serialize(backup));
        command.Parameters.AddWithValue("@owner", owner);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<bool> DoesGuildConfigExistAsync(ulong guild)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM bak_guild_settings WHERE id = @id";
        command.Parameters.AddWithValue("@id", guild);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }
    
    public async Task<GuildConfig> GetGuildConfigAsync(ulong guild)
    {
        if (!await DoesGuildConfigExistAsync(guild))
            await InsertGuildConfigAsync(guild);
        
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM bak_guild_settings WHERE id = @id";
        command.Parameters.AddWithValue("@id", guild);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return new GuildConfig(reader.GetBoolean(1), reader.GetBoolean(2),
            reader.GetInt64(3), reader.GetInt64(4));
    }
    
    private async Task InsertGuildConfigAsync(ulong guild)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO bak_guild_settings (id, admins_can_backup, admins_can_restore, `interval`, max_backups) VALUES (@id, @admins_can_backup, @admins_can_restore, @interval, @max_backups)";
        command.Parameters.AddWithValue("@id", guild);
        command.Parameters.AddWithValue("@admins_can_backup", false);
        command.Parameters.AddWithValue("@admins_can_restore", false);
        command.Parameters.AddWithValue("@interval", 0);
        command.Parameters.AddWithValue("@max_backups", 10);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateGuildConfigAsync(ulong guild, GuildConfig update)
    {
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE bak_guild_settings SET admins_can_backup = @admins_can_backup, admins_can_restore = @admins_can_restore, `interval` = @interval, max_backups = @max_backups WHERE id = @id";
        command.Parameters.AddWithValue("@id", guild);
        command.Parameters.AddWithValue("@admins_can_backup", update.AdminsCanBackup);
        command.Parameters.AddWithValue("@admins_can_restore", update.AdminsCanRestore);
        command.Parameters.AddWithValue("@interval", update.Interval);
        command.Parameters.AddWithValue("@max_backups", update.MaxBackups);
        await command.ExecuteNonQueryAsync();
    }
    
}