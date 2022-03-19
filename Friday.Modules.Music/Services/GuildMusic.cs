using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Friday.Modules.Music.Enums;
using Friday.Modules.Music.Models;

namespace Friday.Modules.Music.Services;

public class GuildMusic
{
    private DiscordClient _client;
    public DiscordClient Client => _client;
    private LavalinkExtension _lavalink;
    public DiscordGuild Guild { get; init; }
    public GuildMusicState State { get; private set; } = GuildMusicState.Stopped;   
    private readonly MusicQueue _queue = new();
    public IReadOnlyList<LavalinkTrack> Queue => _queue.ToReadOnlyList();
    public LavalinkTrack? Playing { get; private set; }
    public DiscordChannel? Channel { get; private set; }
    public RepeatMode Repeat { get; set; } = RepeatMode.None;
    public bool Shuffle { get; set; } = false;
    public int Volume { get; private set; } = 25;
    internal GuildMusic(DiscordClient client, DiscordGuild guild)
    {
        this._client = client;
        this._lavalink = client.GetLavalink();
        this.Guild = guild;
        _client.VoiceStateUpdated += OnVoiceStateUpdated;
    }

    private async Task OnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
    {
        State = GuildMusicState.Stopped;
        if (e.Reason == TrackEndReason.Finished)
        {
            Console.WriteLine("Track finished");
            if (Repeat == RepeatMode.RepeatOne)
            {
                await Task.Delay(2500);
                
                await Play(e.Track);
            }
            else
            {
                if (Repeat == RepeatMode.Repeat)
                {
                    if (_queue.Peek() is null)
                    {
                        _queue.Reset();
                    }
                }
                
                if (Shuffle)
                {
                    var next = _queue.ShuffledDequeue();
                    if (next != null)
                    {
                        await Play(next);
                    }
                    else
                    {
                        await Stop();
                    }
                }
                else
                {
                    var next = _queue.Dequeue();
                    if (next != null)
                    {
                        Console.WriteLine("Playing next track... " + next.Title);
                        await Play(next);
                    }
                    else
                    {
                        await Stop();
                    }
                }
            }
        }
    }

    private async Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (Channel is null) return;
        
        if (e.Before?.Channel?.Id == Channel?.Id && e.After.Channel is null)
        {
            if (e.User.Id == _client.CurrentUser.Id)
            {
                // We left the channel
                MusicPanel.Destroy(Guild.Id);
                await Stop();
                return;
            }else if (e.User.Id == Guild.OwnerId)
            {
                if (e.After.Channel?.Users.Count == 1)
                {
                    // We were the only one in the channel
                    await Pause();
                    return;
                }
            }
        }
        
        if (e.Before?.Channel?.Id == Channel?.Id && e.After.Channel?.Id != Channel?.Id)
        {
            if (e.User.Id == _client.CurrentUser.Id)
            {
                // We moved to another channel, don't do anything
                return;
            }
        }
    }

    public async Task SetVolume(int volume)
    {
        volume = Math.Clamp(volume, 0, 100);
        
        if (Playing is null) return;
        
        Volume = volume;
        await GetConnection()!.SetVolumeAsync(volume);
    }
    
    public LavalinkGuildConnection? GetConnection()
    {
        return this._lavalink.GetGuildConnection(this.Guild);
    }
    
    private LavalinkNodeConnection GetNodeConnection()
    {
        return this._lavalink.ConnectedNodes.First().Value;
    }
    
    private async Task Play(LavalinkTrack track)
    {
        if (State == GuildMusicState.Playing)
        {
            _queue.Enqueue(track);
            return;
        }
        _queue.Enqueue(track);
        _queue.Dequeue();
        if (Channel is null) throw new InvalidOperationException("No channel set");
        
        await GetConnection()!.PlayAsync(track);
        State = GuildMusicState.Playing;
        Playing = track;
    }

    public async Task Play(Uri uri)
    {
        var track = await GetConnection()!.GetTracksAsync(uri);
        if (track is null) return;
        if (!track.Tracks.Any()) return;

        foreach (var t in track.Tracks)
        {
            await Play(t);
        }
    }

    public async Task PlayYTSearch(string search)
    {
        var ytSearch = await GetConnection()!.GetTracksAsync(search, LavalinkSearchType.Youtube);
        
        if (ytSearch is null) return;
        
        if (!ytSearch.Tracks.Any()) return;
        var track = ytSearch.Tracks.First();
        await Play(track);
    }

    public async Task Join(DiscordChannel channel)
    {
        var connection = GetConnection();
        if (connection is not null || Channel is not null)
        {
            return;
        }
        

        connection = await GetNodeConnection().ConnectAsync(channel);
        await GetConnection()!.StopAsync();
        GetConnection()!.PlaybackFinished += OnPlaybackFinished;
        Channel = channel;
    }

    public async Task Next()
    {
        if (State == GuildMusicState.Stopped) return;
        
        if (State == GuildMusicState.Playing)
        {
            await GetConnection()!.StopAsync();
            await Task.Delay(1000);
        }
        State = GuildMusicState.Stopped;
        if (_queue.Count > 0)
        {
            var next = _queue.Dequeue();
            if (next != null)
            {
                Console.WriteLine("Playing next track... " + next.Title);
                await GetConnection()!.PlayAsync(next);
                State = GuildMusicState.Playing;
                Playing = next;
            }
            else
            {
                await Stop();
            }
        }
        else
        {
            await Stop();
        }
    }
    
    public async Task Previous()
    {
        if (State == GuildMusicState.Stopped) return;
        
        if (State == GuildMusicState.Playing)
        {
            await GetConnection()!.StopAsync();
            await Task.Delay(1000);
        }

        State = GuildMusicState.Stopped;
        var next = _queue.Previous();
        if (next != null)
        {
            Console.WriteLine("Playing previous track... " + next.Title);
            await GetConnection()!.PlayAsync(next);
            State = GuildMusicState.Playing;
            Playing = next;
        }
        else
        {
            await Stop();
        }
    }

    public async Task SetPosition(TimeSpan span)
    {
        await GetConnection()!.SeekAsync(span);
    }
    
    public async Task Pause()
    {
        await GetConnection()!.PauseAsync();
        
        State = GuildMusicState.Paused;
    }

    public async Task Resume()
    {
        await GetConnection()!.ResumeAsync();
        
        if (State == GuildMusicState.Paused)
        {
            State = GuildMusicState.Playing;
        }
    }

    public async Task Stop()
    {
        MusicPanel.Destroy(Guild.Id);
        try
        {
            GetConnection()!.PlaybackFinished -= OnPlaybackFinished;
            await GetConnection()!.StopAsync();
            await GetConnection()!.DisconnectAsync();
            
        }catch
        {
            // ignored
        }
        

        Channel = null;
        State = GuildMusicState.Stopped;
        
        _queue.Clear();
        
        Playing = null;
    }
    
    
    
    
    
}