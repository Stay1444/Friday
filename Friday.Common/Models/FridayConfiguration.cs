namespace Friday.Common.Models;

public record FridayConfiguration
{
    public record FridayConfigurationDiscord
    {
        public string Token { get; init; } = Guid.Empty.ToString();
        public string Color { get; init; } = "#4287f5";
        
        public string OfficialServer { get; init; } = "https://discord.gg/friday";
        public string BotInvite { get; init; } = "https://discordapp.com/oauth2/authorize?client_id=0&scope=bot&permissions=8";
    }

    public record FridayConfigurationDatabase
    {
        public string ConnectionString { get; init; } = "Server=127.0.0.1;Port=3306;Database=friday;Username=root;Password=root;";
    }

    public record FridayConfigurationLavalink
    {
        public bool Enabled { get; init; } = false;
        public string Password { get; init; } = "youshallnotpass";
        public string Host { get; init; } = "localhost";
        public ushort Port { get; init; } = 2333;
    }

    public record FridayConfigurationEmojis
    {
        public ulong Transparent { get; init; } = 0;
        public ulong Mod { get; init; } = 0;
        public ulong Boost { get; init; } = 0;
        public ulong Verified { get; init; } = 0;
    }

    public record FridayConfigurationSimpleCdn
    {
        public string Host { get; init; } = "https://cdn.friday.gg";
        public string ApiKey { get; init; } = Guid.Empty.ToString();
    }
    
    public bool Debug { get; init; } = false;
    public FridayConfigurationDiscord Discord { get; init; } = new FridayConfigurationDiscord();
    public FridayConfigurationDatabase Database { get; init; } = new FridayConfigurationDatabase();
    public FridayConfigurationLavalink Lavalink { get; init; } = new FridayConfigurationLavalink();
    public FridayConfigurationEmojis Emojis { get; init; } = new FridayConfigurationEmojis();
    public FridayConfigurationSimpleCdn SimpleCdn { get; init; } = new FridayConfigurationSimpleCdn();
}