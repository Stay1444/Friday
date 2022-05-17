namespace Friday.Modules.AntiRaid.Entities;

public class AntiRaidSettings
{
    public class AntiRaidSettingsChannels
    {
        public bool Enabled { get; set; } = true;
        public int Count { get; set; } = 5;
        public bool Ban { get; set; } = true;
        public bool Restore { get; set; } = true;
        public TimeSpan Time { get; set; } = TimeSpan.FromMinutes(5);
    }

    public class AntiRaidSettingsRoles
    {
        public bool Enabled { get; set; } = true;
        public int Count { get; set; } = 5;
        public bool Ban { get; set; } = true;
        public bool Restore { get; set; } = true;
        public TimeSpan Time { get; set; } = TimeSpan.FromMinutes(5);
    }

    public class AntiRaidSettingsBans
    {
        public bool Enabled { get; set; } = true;
        public int Count { get; set; } = 5;
        public bool Ban { get; set; } = true;
        public TimeSpan Time { get; set; } = TimeSpan.FromMinutes(5);
    }

    public class AntiRaidSettingsKicks
    {
        public bool Enabled { get; set; } = true;
        public int Count { get; set; } = 5;
        public bool Ban { get; set; } = true;
        public TimeSpan Time { get; set; } = TimeSpan.FromMinutes(5);
    }
    
    public class AntiRaidSettingsLogs
    {
        public bool Enabled { get; set; } = false;
        public ulong ChannelId { get; set; } = 0;
    }
    
    public bool Enabled { get; set; }
    public bool AdminsCanBypass { get; set; }
    
    public AntiRaidSettingsLogs Logs { get; set; } = new AntiRaidSettingsLogs();
    
    public AntiRaidSettingsChannels CreateChannels { get; set; } = new AntiRaidSettingsChannels();
    public AntiRaidSettingsChannels DeleteChannels { get; set; } = new AntiRaidSettingsChannels();
    public AntiRaidSettingsChannels UpdateChannels { get; set; } = new AntiRaidSettingsChannels();
    
    public AntiRaidSettingsRoles CreateRoles { get; set; } = new AntiRaidSettingsRoles();
    public AntiRaidSettingsRoles DeleteRoles { get; set; } = new AntiRaidSettingsRoles();
    public AntiRaidSettingsRoles UpdateRoles { get; set; } = new AntiRaidSettingsRoles();
    public AntiRaidSettingsRoles GrantRoles { get; set; } = new AntiRaidSettingsRoles();
    public AntiRaidSettingsRoles RevokeRoles { get; set; } = new AntiRaidSettingsRoles();

    
    public AntiRaidSettingsBans Bans { get; set; } = new AntiRaidSettingsBans();
    
    public AntiRaidSettingsKicks Kicks { get; set; } = new AntiRaidSettingsKicks();
}