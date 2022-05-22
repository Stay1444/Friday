using DSharpPlus;
using Friday.Common;

namespace Friday.Modules.CustomCommands;

public class CustomCommandsModule : ModuleBase
{
    public CustomCommandsModule(DiscordShardedClient client)
    {
        Client = client;
    }

    internal DiscordShardedClient Client { get; }
    public override Task OnLoad()
    {
        throw new NotImplementedException();
    }

    public override Task OnUnload()
    {
        throw new NotImplementedException();
    }
}