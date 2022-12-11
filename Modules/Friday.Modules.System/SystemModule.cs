using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Friday.Common;
using Friday.Common.Models;

namespace Friday.Modules.System;

public class SystemModule : ModuleBase
{
    private DiscordShardedClient _client;
    private FridayConfiguration _config;
    private CancellationTokenSource _timerCtSource = new();
    public SystemModule(DiscordShardedClient client, FridayConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public override Task OnLoad()
    {
        _client.GuildCreated += GuildCreated;
        _client.GuildDownloadCompleted += GuildDownloadCompleted;
        return Task.CompletedTask;
    }

    private Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        _ = Timer();
        return Task.CompletedTask;
    }

    private int _lastIter = 0;
    private async Task Timer()
    {
        Console.WriteLine("Timer started");
        while (!_timerCtSource.IsCancellationRequested)
        {
            if (_lastIter == 0)
            {
                await _client.UpdateStatusAsync(new DiscordActivity
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"fhelp | {_client.GetUserCount()} users"
                }, UserStatus.DoNotDisturb);
                _lastIter = 1;
            }else if (_lastIter == 1)
            {
                await _client.UpdateStatusAsync(new DiscordActivity()
                {
                    ActivityType = ActivityType.Watching,
                    Name = $"fhelp | {_client.GetGuildCount()} servers"
                });
                _lastIter = 0;
            }

            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }

    private async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        var guildOwner = e.Guild.Owner;

        var discordEmbedBuilder = new DiscordEmbedBuilder();
        discordEmbedBuilder.Title = "Thank you for adding me to your server!";
        discordEmbedBuilder.WithColor(new DiscordColor(_config.Discord.Color));
        discordEmbedBuilder.WithDescription("**Thanks for adding Friday to your server!**\n\n" +
                                            "You can use `fhelp` to see a list of commands.\n\n" +
                                            $"If you have any questions, feel free to join the [Official Server]({_config.Discord.OfficialServer}) and ask for help!\n\n");
        
        discordEmbedBuilder.AddField("Beta", $"Friday is currently in beta. If you find any bugs, please report them in the [Official Server]({_config.Discord.OfficialServer})");

        discordEmbedBuilder.WithFooter("Love from the Friday Team", _client.CurrentUser.AvatarUrl);
        
        try
        {
            await guildOwner.SendMessageAsync(embed: discordEmbedBuilder);
        }catch
        {
            // Direct Messages are disabled. Search a channel where we can send messages;

            foreach (var channel in e.Guild.Channels.Where(x => x.Value.Type == ChannelType.Text))
            {
                try
                {
                    await channel.Value.SendMessageAsync(embed: discordEmbedBuilder);
                    
                    break;
                }catch
                {
                    // ignored
                }
            }
        }
    }

    public override Task OnUnload()
    {
        _client.GuildCreated -= GuildCreated;
        _timerCtSource.Cancel();
        _client.GuildDownloadCompleted -= GuildDownloadCompleted;
        return Task.CompletedTask;
    }
    
    
}