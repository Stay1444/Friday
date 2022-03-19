using DSharpPlus.Entities;
using Friday.Common.Models;
using Serilog;

namespace Friday.Common.Services;

public class GuildConfigurationProvider
{
    private DatabaseProvider _db;

    public GuildConfigurationProvider(DatabaseProvider databaseProvider)
    {
        _db = databaseProvider;
    }

    public Task<GuildConfiguration> GetConfiguration(DiscordGuild guild)
    {
        return GetConfiguration(guild.Id);
    }

    public async Task<GuildConfiguration> GetConfiguration(ulong guildId)
    { 
        var guildConfig = await _db.QueryFirstOrDefaultAsync<GuildConfiguration>("SELECT * FROM guild_config WHERE id = @Id", new { Id = guildId });
        if (guildConfig is null)
        {
            Log.Debug("No configuration found for guild {guildId}, creating...", guildId);
            try
            {
                await _db.ExecuteAsync("INSERT INTO guild_config (id, prefix, language) VALUES (@Id, @Prefix, @Language)", new { Id = guildId, Prefix = "f", Language = "en" });
            }catch(Exception e)
            {
                Log.Error(e, "Failed to create configuration for guild {guildId}", guildId);
                throw;
            }
            return await GetConfiguration(guildId);
        }
        
        return guildConfig;
    }
}