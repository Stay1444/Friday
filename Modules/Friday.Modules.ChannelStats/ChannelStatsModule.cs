using DSharpPlus;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.ChannelStats.Services;
using Serilog;

namespace Friday.Modules.ChannelStats;

public class ChannelStatsModule : ModuleBase
{
    private CancellationTokenSource _cancellationTokenSource;
    public const int ChannelStatsIntervalSeconds = (60 * 5) + 30; // 5 minutes is the Discord Rate Limit for Channel Updates. We add 30 seconds just to be sure.
    private DiscordShardedClient _client;
    internal DatabaseService DatabaseService;
    internal VariablesService VariablesService;
    internal DateTime LastChannelStatsUpdate = DateTime.UtcNow;
    public const int MaxChannelsPerGuild = 10;

    public TimeSpan UntilNextUpdate => LastChannelStatsUpdate +
        TimeSpan.FromSeconds(ChannelStatsModule.ChannelStatsIntervalSeconds) - DateTime.UtcNow;
    
    public ChannelStatsModule(DiscordShardedClient client, DatabaseProvider databaseProvider)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _client = client;
        DatabaseService = new DatabaseService(databaseProvider);
        VariablesService = new VariablesService(this);
    }

    public override Task OnLoad()
    {
        Log.Information("[ChannelStats] Starting timer");
        _cancellationTokenSource = new CancellationTokenSource();
        _ = Timer();
        Log.Information("[ChannelStats] Timer started");
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        _cancellationTokenSource.Cancel();
        Log.Information("[ChannelStats] Timer stopped");
        return Task.CompletedTask;
    }

    private async Task Timer()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                await UpdateGuilds();
                LastChannelStatsUpdate = DateTime.UtcNow;
            }catch(Exception e)
            {
                Log.Error(e, "[ChannelStats] Error in timer");
            }
            await Task.Delay(ChannelStatsIntervalSeconds * 1000);
        }
    }

    private async Task UpdateGuilds()
    {
        Log.Debug("[ChannelStats] Updating guilds");
        var tasks = new List<Task>();
        foreach (var discordGuild in _client.GetGuilds())
        {
            var t = Task.Run(async () =>
            {
                try
                {
                    var channels = await DatabaseService.GetGuildStatsChannelsAsync(discordGuild.Id);
                    foreach (var statsChannel in channels)
                    {
                        if (!discordGuild.Channels.ContainsKey(statsChannel.Id)) continue;
                        var channel = discordGuild.Channels[statsChannel.Id];
                        var desiredName = await VariablesService.Process(discordGuild, statsChannel.Value);
                        if (channel.Name == desiredName) continue;
                        await channel.ModifyAsync(x => x.Name = desiredName.MaxLength(99));
                    }
                }catch
                {
                    // ignored
                }
            });
            tasks.Add(t);
        }
        
        await Task.WhenAll(tasks);
        
        Log.Debug("[ChannelStats] Guilds updated");
    }
}