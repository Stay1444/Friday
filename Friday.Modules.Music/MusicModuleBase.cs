using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Music.Models;
using Friday.Modules.Music.Services;
using Serilog;
using Tomlyn;
using VideoLibrary;

namespace Friday.Modules.Music;

public class MusicModuleBase : ModuleBase
{
    private Dictionary<ulong, GuildMusic> _musicPlayers = new Dictionary<ulong, GuildMusic>();
    private DiscordShardedClient _client;
    private MusicConfig? _config;
    public MusicConfig Config => _config ?? throw new InvalidOperationException("Config not loaded");
    public MusicModuleBase(DiscordShardedClient client)
    {
        _client = client;
    }

    public Task OnLoad()
    {
        Log.Information("[Music] Module loaded.");
        Log.Information("[Music] Loading config...");
        if (!File.Exists("conf/music.toml"))
        {
            File.WriteAllText("conf/music.toml", Toml.FromModel(new MusicConfig()));
        }
        var config = Toml.ToModel<MusicConfig>(File.ReadAllText("conf/music.toml"));
        _config = config;
        Log.Information("[Music] Config loaded.");
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