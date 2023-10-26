using DSharpPlus;
using DSharpPlus.EventArgs;
using Friday.Common;

using Friday.Modules.Minesprout.Minesprout;
using Friday.Modules.Minesprout.Services;

using Serilog;
using SimpleCDN.Wrapper;

namespace Friday.Modules.Minesprout;
public class MinesproutModule : ModuleBase
{
    public ServerIconResolver IconResolver { get; }
    public ServerBannerResolver BannerResolver { get; }
    public MinesproutConfiguration Configuration = null!;
    private PeriodicEmbed? _periodicEmbed;
    private readonly DiscordShardedClient _discordShardedClient;
    public MinesproutModule(SimpleCdnClient cdnClient, DiscordShardedClient client)
    {
        this.IconResolver = new ServerIconResolver(cdnClient);
        this.BannerResolver = new ServerBannerResolver(cdnClient);
        this._discordShardedClient = client;
    }

    public override async Task OnLoad()
    {
        Configuration = await ReadConfiguration<MinesproutConfiguration>();

        if (Configuration.PeriodicEmbed.Enabled)
        {
            _periodicEmbed = new PeriodicEmbed(this, _discordShardedClient);
        }

        Log.Information("Minesprout: Loaded configuration");
    }

    public override Task OnUnload()
    {
        _periodicEmbed?.Stop();
        return Task.CompletedTask;
    }

    public MinesproutClient CreateClient()
    {
        Log.Information("Minesprout: Created client");
        return new MinesproutClient(this);
    }
}
