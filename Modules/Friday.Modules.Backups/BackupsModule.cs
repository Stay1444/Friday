using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.Backups.Services;
using Serilog;

namespace Friday.Modules.Backups;

public class BackupsModule : ModuleBase
{
    internal DatabaseService Database { get; }
    internal BackupService BackupService { get; }
    public const int BackupsPerUser = 500;
    public BackupsModule(DatabaseProvider provider)
    {
        Database = new DatabaseService(provider);
        this.BackupService = new BackupService(this);
    }
    public override Task OnLoad()
    {
        Log.Information("[Backups] Module loaded. Starting timers");  
        
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        Log.Information("[Backups] Module unloaded. Stopping timers");
        
        return Task.CompletedTask;
    }
}