using Friday.Common;
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
    internal Configuration Configuration { get; private set; } = null!;

    internal SimpleCdnClient CdnClient { get; private set; } = null!;
    
    public const int BackupsPerUser = 500;
    public BackupsModule(DatabaseProvider provider)
    {
        Database = new DatabaseService(provider);
        this.BackupService = new BackupService(this);
        this.RoleCooldownService = new RoleCooldownService(provider);
    }
    public override Task OnLoad()
    {
        Log.Information("[Backups] Module loaded. Starting timers");
        if (!File.Exists("conf/backups.toml"))
        {
            File.WriteAllText("conf/backups.toml", Toml.FromModel(new Configuration()));
            Log.Information("[Backups] Created configuration file");
        }

        Configuration = Toml.ToModel<Configuration>(File.ReadAllText("conf/backups.toml"));
        this.CdnClient = new SimpleCdnClient(this.Configuration.CdnHost, Guid.Parse(this.Configuration.ApiKey ?? Guid.Empty.ToString()));
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        Log.Information("[Backups] Module unloaded. Stopping timers");
        
        return Task.CompletedTask;
    }
}