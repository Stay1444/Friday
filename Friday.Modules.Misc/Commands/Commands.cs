using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;

namespace Friday.Modules.Misc.Commands;

public partial class Commands : FridayCommandModule
{
    private FridayModeratorService _moderatorService;
    private FridayConfiguration _fridayConfiguration;
    private FridayVerifiedServerService _fridayVerifiedServer;

    public Commands(FridayModeratorService moderatorService, FridayConfiguration fridayConfiguration, FridayVerifiedServerService fridayVerifiedServer)
    {
        _moderatorService = moderatorService;
        _fridayConfiguration = fridayConfiguration;
        _fridayVerifiedServer = fridayVerifiedServer;
        
    }
}