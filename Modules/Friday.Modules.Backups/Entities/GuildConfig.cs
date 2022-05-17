namespace Friday.Modules.Backups.Entities;

public record GuildConfig(bool AdminsCanBackup, bool AdminsCanRestore, long Interval, long MaxBackups);