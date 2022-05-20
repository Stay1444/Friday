namespace Friday.Modules.Backups.Entities;

public record GuildConfig(bool AdminsCanBackup, bool AdminsCanRestore, long Interval, long MaxBackups)
{
    public bool AdminsCanBackup { get; set; }
    public bool AdminsCanRestore { get; set; }
    public long Interval { get; set; }
    public long MaxBackups { get; set; }
}