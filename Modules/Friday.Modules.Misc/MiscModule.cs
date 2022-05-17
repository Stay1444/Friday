using Friday.Common;

namespace Friday.Modules.Misc;

public class MiscModule : ModuleBase
{
    public override Task OnLoad()
    {
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }
}