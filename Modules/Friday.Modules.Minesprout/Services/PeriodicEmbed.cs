using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Minesprout.Minesprout;
using Friday.Modules.Minesprout.UI;
using Friday.UI;
using Friday.UI.Entities;
using Serilog;

namespace Friday.Modules.Minesprout.Services;

public class PeriodicEmbed
{
    private MinesproutModule _module;
    private MinesproutClient _minesproutClient;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private DiscordShardedClient _discordClient;
    public PeriodicEmbed(MinesproutModule module, DiscordShardedClient client)
    {
        _module = module;
        this._discordClient = client;
        _minesproutClient = module.CreateClient();
        Task.Run(UpdateLoop);
    }

    private async Task UpdateLoop()
    {
        var guild = await _discordClient.GetGuildAsync(_module.Configuration.PeriodicEmbed.GuildId);
        var myself = _discordClient.CurrentUser.Id;
        
        if (guild is null)
        {
            Log.Error("Minesprout: Cannot find PeriodicEmbed guild");
            return;
        }

        if (!guild.Channels.ContainsKey(_module.Configuration.PeriodicEmbed.ChannelId))
        {
            Log.Error("Minesprout: Cannot find PeriodicEmbed channel;");
            return;
        }

        var channel = guild.GetChannel(_module.Configuration.PeriodicEmbed.ChannelId);
        
        await foreach (var msg in channel.GetMessagesAsync(1))
        {
            if (msg.Author.Id != myself) continue;

            try
            {
                await msg.DeleteAsync();
            }catch { /* ignored */}
            
            break;
        }
        
        var ui = new FridayUIBuilder();

        ui.OnRenderAsync(async x =>
        {
            var servers = x.GetState("servers", await _minesproutClient.GetServersAsync(true, 0, 5));
            var serverId = servers.Value?.Servers.FirstOrDefault()?.Id;

            if (serverId is null) {
                x.Embed.Description = "Could not fetch latest servers";
                return;
            }

            var server = await _minesproutClient.GetServerAsync((ulong)serverId.Value);

            if (server is null)
            {
                x.Embed.Description = "Could not fetch latest server";
                return;
            }
            
            await ServerUI.RenderAsync(server, x, _module, _minesproutClient);

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(_module.Configuration.PeriodicEmbed.Interval), _cts.Token);
                Log.Information("Minesprout: Refreshing Periodic Embed");
                if (_cts.IsCancellationRequested)
                {
                    x.Stop();
                }

                servers.Value = await _minesproutClient.GetServersAsync(true, 0, 5);
                
                x.ForceRender();
            });
        });

        await channel.SendUIAsync(_discordClient.GetClient(channel.Guild), ui);
    }
    
    public void Stop()
    {
        this._cts.Cancel();
    }
}