using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;

namespace Friday.Common;

public abstract class ModuleBase
{
    public abstract Task OnLoad();
    public abstract Task OnUnload();
    
    public virtual async Task HandleFailedChecks(Type failedCheck, CommandsNextExtension extension,
        CommandErrorEventArgs args)
    {
        await args.Context.Channel.SendMessageAsync("Unhandled exception in module " + this.GetType().Name + " - " + failedCheck.Name);
    }
    
    public virtual void RegisterSlashCommands(SlashCommandsExtension extension) { }
}