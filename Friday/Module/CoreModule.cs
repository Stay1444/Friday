using Friday.Common;

namespace Friday.Module;

public class CoreModule : ModuleBase
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