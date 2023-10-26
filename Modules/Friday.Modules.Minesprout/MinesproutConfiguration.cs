namespace Friday.Modules.Minesprout;


public class MinesproutConfiguration
{
    public string Token { get; set; } = "minesprout-token";

    public class PeriodicEmbedConfig
    {
        public bool Enabled { get; set; } = false;
        public ulong Interval { get; set; } = 5;
        public ulong GuildId { get; set; } = 0;
        public ulong ChannelId { get; set; } = 0;
    }

    public PeriodicEmbedConfig PeriodicEmbed { get; set; } = new PeriodicEmbedConfig();
}