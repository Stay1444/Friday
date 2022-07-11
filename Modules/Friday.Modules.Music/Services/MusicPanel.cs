using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using Friday.Common;
using Friday.Modules.Music.Enums;
using Friday.Modules.Music.Processing;
using Serilog;

namespace Friday.Modules.Music.Services;

public class MusicPanel : IDisposable
{
    private readonly static Dictionary<ulong, MusicPanel> MusicPanels = new ();

    private readonly GuildMusic _music;
    private readonly DiscordChannel _channel;
    private MusicModuleBase _musicModuleBase;
    private DiscordMessage? _message;
    private CancellationTokenSource? _cancellationTokenSource;
    private DateTime _lastUpdate = DateTime.MinValue;
    private string? _imageUrl = null;
    private System.Timers.Timer _imgUpdateTimer;
    private MusicPanel(GuildMusic music, DiscordChannel textChannel, MusicModuleBase musicModuleBase)
    {
        this._music = music;
        this._channel = textChannel;
        _musicModuleBase = musicModuleBase;
        _music.Client.ComponentInteractionCreated += E_ComponentInteractionCreated;
        _imgUpdateTimer = new (15000);
        _imgUpdateTimer.AutoReset = true;
        _imgUpdateTimer.Elapsed += _imgUpdateTimer_Elapsed;
    }

    private async void _imgUpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (_music.State == GuildMusicState.Stopped)
            return;
        var playingTrackImage = new PlayingTrackImage(_music,_music.State == GuildMusicState.Playing ? 15 : 0);
        try
        {
            var result = await playingTrackImage.ProcessAsync();
            var url = await UploadToCdn(result);
            this._imageUrl = url;
            await Update();
        }catch(Exception er)
        {
            Log.Error(er, "Error while processing playing track image");
        }
    }
    
    public static void Destroy(ulong guildId)
    {
        if (MusicPanels.TryGetValue(guildId, out var musicPanel))
        {
            musicPanel.Dispose();
            MusicPanels.Remove(guildId);
        }
    }

    public static MusicPanel? GetMusicPanel(GuildMusic music)
    {
        if (MusicPanels.ContainsKey(music.Guild.Id))
            return MusicPanels[music.Guild.Id];
        
        return null;
    }
    
    public static MusicPanel CreateMusicPanel(GuildMusic music, DiscordChannel textChannel, MusicModuleBase moduleBase)
    {
        MusicPanel panel = new MusicPanel(music, textChannel, moduleBase);
        MusicPanels.Add(music.Guild.Id, panel);
        return panel;
    }

    public async Task SendMessageAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
        _cancellationTokenSource = new CancellationTokenSource();
        _message = await _channel.SendMessageAsync(":hourglass:");
        await Update();
        _imgUpdateTimer.Start();
    }

    private async Task Update()
    {
        if (_message is null) return;
        
        var builder = BuildControlPanel(_imageUrl);
        await _message.ModifyAsync(builder);
    }
    
    private DiscordMessageBuilder BuildControlPanel(string? imgUrl)
    {
        DiscordMessageBuilder builder = new DiscordMessageBuilder();
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
        embed.Transparent();
        if (_imageUrl is null)
        {
            embed.WithDescription("Add a song with the <:youtube:952699382225076234> Youtube button!");   
        }
        else
        {
            embed.WithImageUrl(imgUrl);
        }
        
        builder.WithEmbed(embed);

        var c1 = new List<DiscordComponent>();
        var c2 = new List<DiscordComponent>();
        
        c1.Add(new DiscordButtonComponent(ButtonStyle.Secondary, "volume", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":speaker:"))));
        c1.Add(new DiscordButtonComponent(ButtonStyle.Danger, "youtube", "", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(_music.Client, 952699382225076234))));

        
        c1.Add(_music.Repeat switch
        {
            RepeatMode.None => new DiscordButtonComponent(ButtonStyle.Secondary, "repeat", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":repeat:"))),
            RepeatMode.Repeat => new DiscordButtonComponent(ButtonStyle.Success, "repeat", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":repeat:"))),
            RepeatMode.RepeatOne => new DiscordButtonComponent(ButtonStyle.Success, "repeat", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":repeat_one:"))),
            _ => throw new ArgumentOutOfRangeException(nameof(RepeatMode))
        });

        c1.Add(_music.Shuffle
            ? new DiscordButtonComponent(ButtonStyle.Success, "shuffle", "", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":twisted_rightwards_arrows:")))
            : new DiscordButtonComponent(ButtonStyle.Secondary, "shuffle", "", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":twisted_rightwards_arrows:"))));
        
        c1.Add(new DiscordButtonComponent(ButtonStyle.Secondary, "void32", "", true, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(_music.Client, 952937632600576111))));
        
        c2.Add(new DiscordButtonComponent(ButtonStyle.Secondary, "back", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":arrow_left:"))));
        
        c2.Add(_music.State switch
        {
            GuildMusicState.Paused => new DiscordButtonComponent(ButtonStyle.Secondary, "play", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":arrow_forward:"))),
            GuildMusicState.Playing => new DiscordButtonComponent(ButtonStyle.Secondary, "pause", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":pause_button:"))),
            GuildMusicState.Stopped => new DiscordButtonComponent(ButtonStyle.Secondary, "void2", "", true, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(_music.Client, 952937632600576111))),
            _ => throw new ArgumentOutOfRangeException(nameof(GuildMusicState))
        });
        
        c2.Add(new DiscordButtonComponent(ButtonStyle.Secondary, "forward", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":arrow_right:"))));

        c2.Add(new DiscordButtonComponent(ButtonStyle.Secondary, "void00005", "", true, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(_music.Client, 952937632600576111))));
        
        c2.Add(new DiscordButtonComponent(ButtonStyle.Secondary, "stop", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(_music.Client, ":x:"))));
        
        builder.AddComponents(c1);
        builder.AddComponents(c2);
        return builder;
    }

    private async Task<string> UploadToCdn(MemoryStream stream)
    {
        ulong cdnId = _musicModuleBase.Config.CdnChannelId;
        DiscordChannel cdnChannel = await _music.Client.GetChannelAsync(cdnId);
        stream.Position = 0;
        var msg = await cdnChannel.SendMessageAsync(new DiscordMessageBuilder().WithFile("img.gif", stream));
        string url = msg.Attachments.First().Url;
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            await msg.DeleteAsync();
        });
        return url;
    }

    private async Task E_ComponentInteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs e)
    {
        if (e.Message.Id != _message?.Id) return;
        if (e.User.IsBot) return;
        if (e.Interaction.Data.CustomId.Contains("void")) return;

        switch (e.Interaction.Data.CustomId.ToLower())
        {
            case "shuffle":
                _music.Shuffle = !_music.Shuffle;
                break;
            case "repeat":
                _music.Repeat = _music.Repeat switch
                {
                    RepeatMode.None => RepeatMode.Repeat,
                    RepeatMode.Repeat => RepeatMode.RepeatOne,
                    RepeatMode.RepeatOne => RepeatMode.None,
                    _ => throw new ArgumentOutOfRangeException(nameof(RepeatMode))
                };
                break;
            case "stop":
                await _music.Stop();
                _cancellationTokenSource?.Cancel();
                await _message!.DeleteAsync();
                _musicModuleBase.DeleteMusicPlayer(_music.Guild);
                Destroy(_music.Guild.Id);
                return;
            case "youtube":
                _ = YoutubeSongModel(e);
                return;
            case "pause":
                await _music.Pause();
                break;
            case "play":
                await _music.Resume();
                break;
            case "volume":
                _ = VolumeModel(e);
                return;
            case "back":
                if (_music.GetConnection()!.CurrentState.PlaybackPosition < TimeSpan.FromSeconds(5))
                {
                    await _music.Previous();
                }
                else
                {
                    await _music.SetPosition(TimeSpan.Zero);
                }
                break;
            case "forward":
                await _music.Next();
                break;
        }

        await Update();
        await e.Ack();
    }

    private async Task YoutubeSongModel(ComponentInteractionCreateEventArgs e)
    {
        var response = new DiscordInteractionResponseBuilder();
        response.WithTitle("Youtube Song")
            .WithCustomId("youtube-song-model")
            .AddComponents(
                new TextInputComponent("Search", "songSearch", "Search", "", false))
            .AddComponents(
                new TextInputComponent("Video URL", "songUrl", "https://www.youtube.com/watch?v=dQw4w9WgXcQ", "", false)
            );

        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, response);
        var result = await _music.Client.GetInteractivity().WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id, TimeSpan.FromSeconds(120));
        if (result.TimedOut) return;
        var search = result.Result.Values["songSearch"];
        var url = result.Result.Values["songUrl"];
        await result.Ack();
        if (string.IsNullOrEmpty(url))
        {
            Log.Information("Searching for {search}", search);
            await _music.PlayYTSearch(search);
            await Update();
        }
        else
        {
            Log.Information("Playing {url}", url);
            await _music.Play(new Uri(url));
            await Update();
        }
    }

    private async Task VolumeModel(ComponentInteractionCreateEventArgs e)
    {
        var response = new DiscordInteractionResponseBuilder();
        
        response.WithTitle("Volume")
            .WithCustomId("volume-model")
            .AddComponents(
                new TextInputComponent("Volume", "volume", "0-100", _music.Volume.ToString(), true, TextInputStyle.Short)
            );
        
        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, response);
        var result = await _music.Client.GetInteractivity().WaitForEventArgsAsync<ModalSubmitEventArgs>(x => x.Interaction.User.Id == e.User.Id, TimeSpan.FromSeconds(120));
        if (result.TimedOut) return;
        
        var volume = result.Result.Values["volume"];
        await result.Ack();
        if (int.TryParse(volume, out var vol))
        {
            await _music.SetVolume(vol);
        }
    }
    
    public void Dispose()
    {
        _music.Client.ComponentInteractionCreated -= E_ComponentInteractionCreated;
        _cancellationTokenSource?.Cancel();
        _imgUpdateTimer.Stop();
        _imgUpdateTimer.Dispose();
        _message?.DeleteAsync();
    }
}