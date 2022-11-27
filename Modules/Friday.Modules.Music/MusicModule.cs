using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Music.Models;
using Friday.Modules.Music.Services;
using Serilog;
using SimpleCDN.Wrapper;

namespace Friday.Modules.Music;

public class MusicModule : ModuleBase
{
    private readonly Dictionary<ulong, GuildMusic> _musicPlayers = new Dictionary<ulong, GuildMusic>();
    private DiscordShardedClient _client;
    private MusicConfig? _config;
    public MusicConfig Config => _config ?? throw new InvalidOperationException("Config not loaded");
    public SimpleCdnClient SimpleCdnClient;
    public MusicModule(DiscordShardedClient client, SimpleCdnClient simpleCdnClient)
    {
        _client = client;
        SimpleCdnClient = simpleCdnClient;
    }

    public override async Task OnLoad()
    {
        Log.Information("[Music] Module loaded.");
        Log.Information("[Music] Loading config...");
        _config = await ReadConfiguration<MusicConfig>();
        Log.Information("[Music] Config loaded.");
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }

    public GuildMusic GetGuildMusic(DiscordGuild guild)
    {
        if (_musicPlayers.ContainsKey(guild.Id))
        {
            return _musicPlayers[guild.Id];
        }
        var musicPlayer = new GuildMusic(_client.GetClient(guild), guild);
        _musicPlayers.Add(guild.Id, musicPlayer);
        return musicPlayer;
    }
    
    public void DeleteMusicPlayer(DiscordGuild guild)
    {
        if (_musicPlayers.ContainsKey(guild.Id))
        {
            _musicPlayers.Remove(guild.Id);
        }
    }
}