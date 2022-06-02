using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;

namespace Friday.Modules.Misc.Commands;

public partial class Commands : FridayCommandModule
{
    private FridayModeratorService _moderatorService;
    private FridayConfiguration _fridayConfiguration;
    private FridayVerifiedServerService _fridayVerifiedServer;
    private DatabaseProvider _databaseProvider;
    private UserConfigurationProvider _userConfigurationProvider;
    private GuildConfigurationProvider _guildConfigurationProvider;
    public Commands(FridayModeratorService moderatorService, FridayConfiguration fridayConfiguration, FridayVerifiedServerService fridayVerifiedServer, DatabaseProvider databaseProvider, UserConfigurationProvider userConfigurationProvider, GuildConfigurationProvider guildConfigurationProvider)
    {
        _moderatorService = moderatorService;
        _fridayConfiguration = fridayConfiguration;
        _fridayVerifiedServer = fridayVerifiedServer;
        _databaseProvider = databaseProvider;
        _userConfigurationProvider = userConfigurationProvider;
        _guildConfigurationProvider = guildConfigurationProvider;
    }
}