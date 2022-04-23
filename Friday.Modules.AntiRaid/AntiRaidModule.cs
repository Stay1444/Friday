using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.AntiRaid.Entities;

namespace Friday.Modules.AntiRaid;

public class AntiRaidModule : ModuleBase
{
    private DiscordShardedClient _client;
    private Dictionary<ulong, GuildAntiRaid> _guilds;
    public AntiRaidModule(DiscordShardedClient client)
    {
        _client = client;
        _guilds = new Dictionary<ulong, GuildAntiRaid>();
    }
    
    public override Task OnLoad()
    {
        _client.GuildDownloadCompleted += async (sender, e) =>
        {
            foreach (var downloadedGuild in e.Guilds)
            {
                await GetAntiRaid(downloadedGuild.Value);
            }
        };
        
        _client.GuildCreated += async (sender, e) =>
        {
            await GetAntiRaid(e.Guild);
        };
        
        _client.GuildDeleted += (sender, e) =>
        {
            _guilds.Remove(e.Guild.Id);
            return Task.CompletedTask;
        };
        
        return Task.CompletedTask;
    }

    public Task<GuildAntiRaid> GetAntiRaid(DiscordGuild guild)
    {
        return this.GetAntiRaid(guild.Id);
    }
    
    public async Task<GuildAntiRaid> GetAntiRaid(ulong guildId)
    {
        
    }
}