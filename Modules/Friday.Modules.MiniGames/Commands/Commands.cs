using Friday.Common.Entities;

namespace Friday.Modules.MiniGames.Commands;

public partial class Commands : FridayCommandModule
{
    private MiniGamesModule _module;

    public Commands(MiniGamesModule module)
    {
        _module = module;
    }
}