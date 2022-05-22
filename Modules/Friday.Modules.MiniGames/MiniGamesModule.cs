using Friday.Common;
using Friday.Common.Models;
using SimpleCDN.Wrapper;

namespace Friday.Modules.MiniGames;

public class MiniGamesModule : ModuleBase
{
    internal SimpleCdnClient SimpleCdnClient { get; }

    public MiniGamesModule(FridayConfiguration configuration)
    {
        this.SimpleCdnClient =
            new SimpleCdnClient(configuration.SimpleCdn.Host, Guid.Parse(configuration.SimpleCdn.ApiKey));
    }
    
    public override Task OnLoad()
    {
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }
}