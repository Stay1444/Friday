using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;

namespace Friday.Modules.Misc.Commands;

public partial class Commands : FridayCommandModule
{
    private FridayModeratorService _moderatorService;
    private FridayConfiguration _fridayConfiguration;

    public Commands(FridayModeratorService moderatorService, FridayConfiguration fridayConfiguration)
    {
        _moderatorService = moderatorService;
        _fridayConfiguration = fridayConfiguration;
    }
}