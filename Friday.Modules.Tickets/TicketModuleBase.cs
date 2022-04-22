using Friday.Common;

namespace Friday.Modules.Tickets;

public class TicketModule : IModule
{
    public Task OnLoad()
    {
        return Task.CompletedTask;
    }
}