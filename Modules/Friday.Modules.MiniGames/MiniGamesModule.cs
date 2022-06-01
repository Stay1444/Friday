using Friday.Common;
using Friday.Common.Models;
using Friday.Common.Services;
using Friday.Modules.MiniGames.Services;
using SimpleCDN.Wrapper;

namespace Friday.Modules.MiniGames;

public class MiniGamesModule : ModuleBase
{
    internal SimpleCdnClient SimpleCdnClient { get; }
    internal DatabaseService DatabaseService { get; }
    public MiniGamesModule(FridayConfiguration configuration, DatabaseProvider provider)
    {
        this.SimpleCdnClient =
            new SimpleCdnClient(configuration.SimpleCdn.Host, Guid.Parse(configuration.SimpleCdn.ApiKey));
        this.DatabaseService = new DatabaseService(provider);
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