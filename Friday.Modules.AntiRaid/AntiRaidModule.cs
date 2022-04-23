using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.AntiRaid.Attributes;
using Friday.Modules.AntiRaid.Entities;
using Friday.Modules.AntiRaid.Services;

namespace Friday.Modules.AntiRaid;

public class AntiRaidModule : ModuleBase
{
    private DiscordShardedClient _client;
    private DatabaseProvider _database;
    private AntiRaidDatabase _antiRaidDatabase;
    private LanguageProvider _language;
    internal AntiRaidDatabase AntiRaidDatabase => _antiRaidDatabase;
    private Dictionary<ulong, GuildAntiRaid> _guilds;
    public AntiRaidModule(DiscordShardedClient client, DatabaseProvider database, LanguageProvider language)
    {
        _client = client;
        _database = database;
        _language = language;
        _antiRaidDatabase = new AntiRaidDatabase(_database);
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
        if (_guilds.ContainsKey(guildId))
        {
            return _guilds[guildId];
        }
        
        var antiRaid = new GuildAntiRaid(this, guildId);
        await antiRaid.LoadAsync();
        _guilds.Add(guildId, antiRaid);
        
        return antiRaid;
    }

    public override async Task HandleFailedChecks(Type failedCheck, CommandsNextExtension extension, CommandErrorEventArgs args)
    {
        if (failedCheck == typeof(RequireAntiRaidPermission))
        {
            var discordEmbedBuilder = new DiscordEmbedBuilder();
            discordEmbedBuilder.WithTitle("AntiRaid");
            discordEmbedBuilder.WithDescription("You don't have the required permissions to use this command.");
            discordEmbedBuilder.Transparent();
            
            await args.Context.RespondAsync(embed: discordEmbedBuilder);

            return;
        }

        await base.HandleFailedChecks(failedCheck, extension, args);
    }
    
}