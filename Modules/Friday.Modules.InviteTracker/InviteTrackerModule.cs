using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.InviteTracker.Entities;
using Friday.Modules.InviteTracker.Services;
using Serilog;

namespace Friday.Modules.InviteTracker;

public class InviteTrackerModule : ModuleBase
{
    public InviteTrackerModule(DatabaseProvider dbProvider, DiscordShardedClient client, LanguageProvider languageProvider)
    {
        _database = new InviteTrackerDatabase(dbProvider);
        _dbProvider = dbProvider;
        _eventListener = new EventListener(this);
        Client = client;
        LanguageProvider = languageProvider;
    }
    
    internal LanguageProvider LanguageProvider { get; }
    internal DiscordShardedClient Client { get; }
    private readonly InviteTrackerDatabase _database;
    private readonly DatabaseProvider _dbProvider;
    private readonly EventListener _eventListener;
    
    internal Dictionary<ulong, List<DiscordInvite>> States { get; } = new();

    public override Task OnLoad()
    {
        Client.GuildDownloadCompleted += async (_, _) =>
        {
            foreach (var guild in Client.GetGuilds())
            {
                States.Add(guild.Id, new List<DiscordInvite>(await guild.GetInvitesAsync()));
                Log.Information("[InviteTracker] Loaded {guild} invites ({count})", guild.Name, States[guild.Id].Count);
            }
            _eventListener.Register();
        };
        
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        _eventListener.Unregister();
        return Task.CompletedTask;
    }

    public async Task<InviteTrackerConfig> GetConfiguration(ulong guildId)
    {
        if (!await _database.DoesConfigurationExist(guildId))
        {
            await _database.CreateConfiguration(guildId);
        }
        
        return await _database.GetConfiguration(guildId);
    }

    public Task<InviteTrackerConfig> GetConfiguration(DiscordGuild guild)
    {
        return GetConfiguration(guild.Id);
    }
    
    public async Task SetConfiguration(ulong guildId, InviteTrackerConfig config)
    {
        if (!await _database.DoesConfigurationExist(guildId))
        {
            await _database.CreateConfiguration(guildId);
        }
        
        await _database.UpdateConfiguration(guildId, config);
    }
    
    public Task SetConfiguration(DiscordGuild guild, InviteTrackerConfig config)
    {
        return SetConfiguration(guild.Id, config);
    }
}