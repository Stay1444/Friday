using DSharpPlus.Entities;
using Friday.Common.Models;
using Serilog;

namespace Friday.Common.Services;

public class UserConfigurationProvider
{
    private DatabaseProvider _db;

    public UserConfigurationProvider(DatabaseProvider databaseProvider)
    {
        _db = databaseProvider;
    }

    public async Task<UserConfiguration> GetConfiguration(ulong userId)
    {
        var userConfig = await _db.QueryFirstOrDefaultAsync<UserConfiguration>("SELECT * FROM user_config WHERE Id = @Id", new { Id = userId });
        if (userConfig is null)
        {
            Log.Debug("No configuration found for user {userId}, creating...", userId);
            try
            {
                await _db.ExecuteAsync("INSERT INTO user_config (id) VALUES (@Id)", new { Id = userId });
            }catch(Exception e)
            {
                Log.Error(e, "Failed to create user configuration for user {userId}", userId);
                throw;
            }
            return await GetConfiguration(userId);
        }
        return userConfig;
        
    }
    
    public Task<UserConfiguration> GetConfiguration(DiscordUser user)
    {
        return GetConfiguration(user.Id);
    }

    public Task<UserConfiguration> GetConfiguration(DiscordMember member)
    {
        return GetConfiguration(member.Id);
    }

    public async Task SaveConfiguration(ulong userId, UserConfiguration configuration)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE user_config SET language_override=@lang, prefix_override=@prefix WHERE id=@id";
        command.Parameters.AddWithValue("@id", userId);
        command.Parameters.AddWithValue("@lang", configuration.LanguageOverride);
        command.Parameters.AddWithValue("@prefix", configuration.PrefixOverride);
        
        await command.ExecuteNonQueryAsync();
    }
}