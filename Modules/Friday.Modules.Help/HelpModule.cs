using Friday.Common;

namespace Friday.Modules.Help;

public class HelpModule : ModuleBase
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