using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.Help.Services;
using Serilog;

namespace Friday.Modules.Help;

public class HelpModule : ModuleBase
{
    public CommandScanner Scanner { get; }

    public HelpModule(IModuleManager manager)
    {
        Scanner = new CommandScanner(manager);
    }
    
    public override Task OnLoad()
    {
        Log.Information("Scanning...");
        Scanner.Build();
        Log.Information("Scan completed.");
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }
}