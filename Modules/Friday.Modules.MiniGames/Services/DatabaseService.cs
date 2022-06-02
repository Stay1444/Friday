using Friday.Common.Services;

namespace Friday.Modules.MiniGames.Services;

public class DatabaseService
{
    private DatabaseProvider _provider;
    public DatabaseService(DatabaseProvider provider)
    {
        this._provider = provider;
    }

    #region 2048
    
    private async Task<bool> Exists2048Stats(ulong userId)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgs_2048_leaderboard WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    private async Task Insert2048Stats(ulong userId, long maxScore, long totalScore, long played, TimeSpan playTime, string username)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO mgs_2048_leaderboard (id, max_score, total_score, played, playtime_seconds, recorded_username) VALUES (@userId, @maxScore, @totalScore, @played, @playTime, @username)";
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@maxScore", maxScore);
        command.Parameters.AddWithValue("@totalScore", totalScore);
        command.Parameters.AddWithValue("@played", played);
        command.Parameters.AddWithValue("@playTime", playTime.TotalSeconds);
        command.Parameters.AddWithValue("@username", username);
        await command.ExecuteNonQueryAsync();
    }

    private async Task Update2048Stats(ulong userId, long maxScore, long totalScore, long played, TimeSpan playTime, string username)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE mgs_2048_leaderboard SET max_score = @maxScore, total_score = @totalScore, played = @played, playtime_seconds = @playTime, recorded_username = @username WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@maxScore", maxScore);
        command.Parameters.AddWithValue("@totalScore", totalScore);
        command.Parameters.AddWithValue("@played", played);
        command.Parameters.AddWithValue("@playTime", playTime.TotalSeconds);
        command.Parameters.AddWithValue("@username", username);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<(long maxScore, long totalScore, long played, TimeSpan playTime, string username)> Read2048Stats(ulong userId)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT max_score, total_score, played, playtime_seconds, recorded_username FROM mgs_2048_leaderboard WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return (0, 0, 0, TimeSpan.Zero, String.Empty);
        
        return (reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), TimeSpan.FromSeconds(reader.GetInt64(3)),  reader.GetString(4));
    }
    
    public async Task<(long maxScore, long totalScore, long played, TimeSpan playTime, string username)> Get2048Stats(ulong userId)
    {
        if (!await Exists2048Stats(userId))
            return (0, 0, 0, TimeSpan.Zero, String.Empty)
                ;
        
        return await Read2048Stats(userId);
    }
    
    public async Task Set2048Stats(ulong userId, long maxScore, long totalScore, long played, TimeSpan playTime, string username)
    {
        if (await Exists2048Stats(userId))
            await Update2048Stats(userId, maxScore, totalScore, played, playTime, username);
        else
            await Insert2048Stats(userId, maxScore, totalScore, played, playTime, username);
    }
    
    public enum _2048LeaderBoardOrderBy
    {
        MaxScore,
        TotalScore,
        Played,
        PlayTime
    }


    public async Task<List<(ulong userId, long maxScore, long totalScore, long played, TimeSpan playTime, string username)>> Get2048Leaderboard(_2048LeaderBoardOrderBy orderBy, int max)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, max_score, total_score, played, playtime_seconds, recorded_username FROM mgs_2048_leaderboard ORDER BY ";
        switch (orderBy)
        {
            case _2048LeaderBoardOrderBy.MaxScore:
                command.CommandText += "max_score DESC";
                break;
            case _2048LeaderBoardOrderBy.TotalScore:
                command.CommandText += "total_score DESC";
                break;
            case _2048LeaderBoardOrderBy.Played:
                command.CommandText += "played DESC";
                break;
            case _2048LeaderBoardOrderBy.PlayTime:
                command.CommandText += "playtime_seconds DESC";
                break;
        }
        
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<(ulong userId, long maxScore, long totalScore, long played, TimeSpan playTime, string username)>();
        while (await reader.ReadAsync())
        {
            result.Add(((ulong) reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3), TimeSpan.FromSeconds(reader.GetInt64(4)), reader.GetString(5)));
            
            if (result.Count >= max)
                break;
        }
        
        return result;
    }

    public async Task<long> Get2048LeaderboardPosition(_2048LeaderBoardOrderBy orderBy, ulong user)
    {
        if (!await Exists2048Stats(user))
            return -1;
        
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM mgs_2048_leaderboard WHERE ";
        switch (orderBy)
        {
            case _2048LeaderBoardOrderBy.MaxScore:
                command.CommandText += "max_score > (SELECT max_score FROM mgs_2048_leaderboard WHERE id = @user)";
                break;
            case _2048LeaderBoardOrderBy.TotalScore:
                command.CommandText += "total_score > (SELECT total_score FROM mgs_2048_leaderboard WHERE id = @user)";
                break;
            case _2048LeaderBoardOrderBy.Played:
                command.CommandText += "played > (SELECT played FROM mgs_2048_leaderboard WHERE id = @user)";
                break;
            case _2048LeaderBoardOrderBy.PlayTime:
                command.CommandText += "playtime_seconds > (SELECT playtime_seconds FROM mgs_2048_leaderboard WHERE id = @user)";
                break;
        }
        
        command.Parameters.AddWithValue("@user", user);
        return (long) (await command.ExecuteScalarAsync() ?? 0);
    }
    
    #endregion
    
    #region Hangman

    private async Task<bool> ExistsHangmanStats(ulong userId)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgs_hangman_leaderboard WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    private async Task InsertHangmanStats(ulong userId, long wonCount, long lostCount, long played, TimeSpan playTime, string username)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO mgs_hangman_leaderboard (id, won_count, lost_count, played, playtime_seconds, recorded_username) VALUES (@userId, @wonCount, @lostCount, @played, @playTime, @username)";
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@wonCount", wonCount);
        command.Parameters.AddWithValue("@lostCount", lostCount);
        command.Parameters.AddWithValue("@played", played);
        command.Parameters.AddWithValue("@playTime", playTime.TotalSeconds);
        command.Parameters.AddWithValue("@username", username);
        await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateHangmanStats(ulong userId, long wonCount, long lostCount, long played, TimeSpan playTime, string username)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE mgs_hangman_leaderboard SET won_count = @wonCount, lost_count = @lostCount, played = @played, playtime_seconds = @playTime, recorded_username = @username WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@wonCount", wonCount);
        command.Parameters.AddWithValue("@lostCount", lostCount);
        command.Parameters.AddWithValue("@played", played);
        command.Parameters.AddWithValue("@playTime", playTime.TotalSeconds);
        command.Parameters.AddWithValue("@username", username);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<(long wonCount, long lostCount, long played, TimeSpan playTime, string username)> ReadHangmanStats(ulong userId)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT won_count, lost_count, played, playtime_seconds, recorded_username FROM mgs_hangman_leaderboard WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return (0, 0, 0, TimeSpan.Zero, String.Empty);
        
        return (reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), TimeSpan.FromSeconds(reader.GetInt64(3)),  reader.GetString(4));
    }
    
    public async Task<(long wonCount, long lostCount, long played, TimeSpan playTime, string username)> GetHangmanStats(ulong userId)
    {
        if (!await ExistsHangmanStats(userId))
            return (0, 0, 0, TimeSpan.Zero, String.Empty);
        
        return await ReadHangmanStats(userId);
    }
    
    public async Task SetHangmanStats(ulong userId, long wonCount, long lostCount, long played, TimeSpan playTime, string username)
    {
        if (await ExistsHangmanStats(userId))
            await UpdateHangmanStats(userId, wonCount, lostCount, played, playTime, username);
        else
            await InsertHangmanStats(userId, wonCount, lostCount, played, playTime, username);
    }
    
    public enum HangmanLeaderBoardOrderBy
    {
        Wins,
        Loses,
        Played,
        PlayTime
    }
    
    public async Task<List<(ulong userId, long wonCount, long lostCount, long played, TimeSpan playTime, string username)>> GetHangmanLeaderboard(HangmanLeaderBoardOrderBy orderBy, int max)
    {
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, won_count, lost_count, played, playtime_seconds, recorded_username FROM mgs_hangman_leaderboard ORDER BY ";
        switch (orderBy)
        {
            case HangmanLeaderBoardOrderBy.Wins:
                command.CommandText += "won_count DESC";
                break;
            case HangmanLeaderBoardOrderBy.Loses:
                command.CommandText += "lost_count DESC";
                break;
            case HangmanLeaderBoardOrderBy.Played:
                command.CommandText += "played DESC";
                break;
            case HangmanLeaderBoardOrderBy.PlayTime:
                command.CommandText += "playtime_seconds DESC";
                break;
        }
        
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<(ulong userId, long maxScore, long totalScore, long played, TimeSpan playTime, string username)>();
        while (await reader.ReadAsync())
        {
            result.Add(((ulong) reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3), TimeSpan.FromSeconds(reader.GetInt64(4)), reader.GetString(5)));
            
            if (result.Count >= max)
                break;
        }
        
        return result;
    }

    public async Task<long> GetHangmanLeaderboardPosition(HangmanLeaderBoardOrderBy orderBy, ulong user)
    {
        if (!await ExistsHangmanStats(user))
            return -1;
        
        await using var connection = _provider.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM mgs_hangman_leaderboard WHERE ";
        switch (orderBy)
        {
            case HangmanLeaderBoardOrderBy.Wins:
                command.CommandText += "won_count > (SELECT won_count FROM mgs_hangman_leaderboard WHERE id = @user)";
                break;
            case HangmanLeaderBoardOrderBy.Loses:
                command.CommandText += "lost_count > (SELECT lost_count FROM mgs_hangman_leaderboard WHERE id = @user)";
                break;
            case HangmanLeaderBoardOrderBy.Played:
                command.CommandText += "played > (SELECT played FROM mgs_hangman_leaderboard WHERE id = @user)";
                break;
            case HangmanLeaderBoardOrderBy.PlayTime:
                command.CommandText += "playtime_seconds > (SELECT playtime_seconds FROM mgs_hangman_leaderboard WHERE id = @user)";
                break;
        }
        
        command.Parameters.AddWithValue("@user", user);
        return (long) (await command.ExecuteScalarAsync() ?? 0);
    }
    
    #endregion
}