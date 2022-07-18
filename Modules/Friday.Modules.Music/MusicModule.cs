using Friday.Common;
using Friday.Modules.Music.Players;

namespace Friday.Modules.Music;

public class MusicModule : ModuleBase
{
    public IReadOnlyList<IMusicPlayer> Players = new List<IMusicPlayer>()
    {

    };

    public override Task OnLoad()
    {
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }
}