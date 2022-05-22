using DSharpPlus.Entities;

namespace Friday.Common.Services;

public class FridayVerifiedServerService
{
    private DatabaseProvider _databaseProvider;
    private List<ulong> _verifiedServers = new List<ulong>();
    private DateTime _lastUpdate = DateTime.MinValue;
    private TimeSpan _updateInterval = TimeSpan.FromMinutes(5);
    public FridayVerifiedServerService(DatabaseProvider databaseProvider)
    {
        _databaseProvider = databaseProvider;
    }

    public async Task<bool> IsVerified(ulong serverId)
    {
        
        if (_verifiedServers.Contains(serverId))
            return true;
        
        if (_lastUpdate.Add(_updateInterval) < DateTime.Now)
        {
            _verifiedServers.Clear();
            _lastUpdate = DateTime.Now;
        }
        
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM friday_verified_servers WHERE server_id = @serverId";
        command.Parameters.AddWithValue("@serverId", serverId);
        await using var reader = await command.ExecuteReaderAsync();
        var result = await reader.ReadAsync();
        
        if (result && !_verifiedServers.Contains(serverId))
            _verifiedServers.Add(serverId);
        
        return result;
    }

    public Task<bool> IsVerified(DiscordGuild guild)
    {
        return IsVerified(guild.Id);   
    }
    
    public async Task AddServer(ulong serverId)
    {
        if (await IsVerified(serverId))
            return;
        
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO friday_verified_servers (server_id) VALUES (@serverId)";
        command.Parameters.AddWithValue("@serverId", serverId);
        await command.ExecuteNonQueryAsync();
        _verifiedServers.Add(serverId);
    }
}