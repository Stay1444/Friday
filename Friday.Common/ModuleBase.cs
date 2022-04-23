using DSharpPlus.CommandsNext;

namespace Friday.Common;

public abstract class ModuleBase
{
    public abstract Task OnLoad();

    public virtual async Task HandleFailedChecks(Type failedCheck, CommandsNextExtension extension,
        CommandErrorEventArgs args)
    {
        await args.Context.Channel.SendMessageAsync("Unhandled exception in module " + this.GetType().Name + " - " + failedCheck.Name);
    }
}