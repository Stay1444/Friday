using Friday.Common;
using Friday.Common.Models;
using Friday.Common.Services;
using Friday.Modules.Backups.Entities;
using Friday.Modules.Backups.Services;
using Serilog;
using SimpleCDN.Wrapper;
using Tomlyn;

namespace Friday.Modules.Backups;

public class BackupsModule : ModuleBase
{
    internal DatabaseService Database { get; }
    internal BackupService BackupService { get; }
    internal RoleCooldownService RoleCooldownService { get; }
    internal SimpleCdnClient CdnClient { get; private set; } = null!;
    
    public const int BackupsPerUser = 500;
    
    private FridayConfiguration _fridayConfiguration;
    
    public BackupsModule(DatabaseProvider provider, FridayConfiguration _configuration)
    {
        this._fridayConfiguration = _configuration;
        Database = new DatabaseService(provider);
        this.BackupService = new BackupService(this);
        this.RoleCooldownService = new RoleCooldownService(provider);
    }
    public override Task OnLoad()
    {
        Log.Information("[Backups] Module loaded. Starting timers");
        this.CdnClient = new SimpleCdnClient(this._fridayConfiguration.SimpleCdn.Host, Guid.Parse(this._fridayConfiguration.SimpleCdn.ApiKey ?? Guid.Empty.ToString()));
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        Log.Information("[Backups] Module unloaded. Stopping timers");
        
        return Task.CompletedTask;
    }
}