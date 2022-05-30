using Friday.Common;

namespace Friday.Modules.Misc;

public class MiscModule : ModuleBase
{
    public override Task OnLoad()
    {
        _ = Constants.ProcessStartTimeUtc.AddDays(0);
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }
}