namespace Friday.Modules.Backups.Entities;

public record GuildConfig
{
    public bool AdminsCanBackup { get; set; }
    public bool AdminsCanRestore { get; set; }
    public long Interval { get; set; }
    public long MaxBackups { get; set; }
    
    public GuildConfig(bool adminsCanBackup, bool adminsCanRestore, long interval, long maxBackups)
    {
        AdminsCanBackup = adminsCanBackup;
        AdminsCanRestore = adminsCanRestore;
        Interval = interval;
        MaxBackups = maxBackups;
    }
}