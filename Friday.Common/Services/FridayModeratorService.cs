using DSharpPlus.Entities;

namespace Friday.Common.Services;

public class FridayModeratorService
{
    private DatabaseProvider _databaseProvider;
    private List<ulong> _moderators = new List<ulong>();
    private DateTime _lastUpdate = DateTime.MinValue;
    private TimeSpan _updateInterval = TimeSpan.FromMinutes(5);
    public FridayModeratorService(DatabaseProvider databaseProvider)
    {
        _databaseProvider = databaseProvider;
    }

    public async Task<bool> IsModerator(ulong userId)
    {
        
        if (_moderators.Contains(userId))
            return true;
        
        if (_lastUpdate.Add(_updateInterval) < DateTime.Now)
        {
            _moderators.Clear();
            _lastUpdate = DateTime.Now;
        }
        
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM friday_moderators WHERE user_id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        var result = await reader.ReadAsync();
        
        if (result && !_moderators.Contains(userId))
            _moderators.Add(userId);
        
        return result;
    }

    public Task<bool> IsModerator(DiscordUser user)
    {
        return IsModerator(user.Id);
    }
    
    public Task<bool> IsModerator(DiscordMember member)
    {
        return IsModerator(member.Id);   
    }
    
    public async Task AddModerator(ulong userId)
    {
        if (await IsModerator(userId))
            return;
        
        await using var connection = _databaseProvider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO friday_moderators (user_id) VALUES (@userId)";
        command.Parameters.AddWithValue("@userId", userId);
        await command.ExecuteNonQueryAsync();
        _moderators.Add(userId);
    }
}