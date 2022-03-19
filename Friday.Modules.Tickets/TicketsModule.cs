using Friday.Common;
using Friday.Modules.EmbedCreator;

namespace Friday.Modules.Tickets;

public class TicketsModule : IModule
{
    private EmbedCreatorModule _embedCreator;

    public TicketsModule(EmbedCreatorModule embedCreator)
    {
        _embedCreator = embedCreator;
    }

    public Task OnLoad()
    {
        return Task.CompletedTask;
    }
}