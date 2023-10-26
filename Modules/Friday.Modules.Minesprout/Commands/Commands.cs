using Friday.Common.Attributes;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Modules.Minesprout.Minesprout;

namespace Friday.Modules.Minesprout.Commands;

[RequireFridayModerator]
public partial class Commands : FridayCommandModule
{
    private readonly MinesproutModule _module;
    private readonly MinesproutClient _minesproutClient;
    private readonly FridayConfiguration _configuration;
    
    public Commands(MinesproutModule module, FridayConfiguration configuration)
    {
        this._module = module;
        _configuration = configuration;
        this._minesproutClient = module.CreateClient();
    }
}
