namespace Friday.Common.Models;

public record FridayConfiguration
{
    public record FridayConfigurationDiscord
    {
        public string Token { get; init; } = Guid.Empty.ToString();
        public string Color { get; init; } = "#4287f5";
    }

    public record FridayConfigurationDatabase
    {
        public string Database { get; init; } = "Friday";
        public string Username { get; init; } = "root";
        public string Password { get; init; } = "root";
        public string Host { get; init; } = "localhost";
        public ushort Port { get; init; } = 3306;
    }

    public record FridayConfigurationLavalink
    {
        public bool Enabled { get; init; } = false;
        public string Password { get; init; } = "youshallnotpass";
        public string Host { get; init; } = "localhost";
        public ushort Port { get; init; } = 2333;
    }

    public FridayConfigurationDiscord Discord { get; init; } = new FridayConfigurationDiscord();
    public FridayConfigurationDatabase Database { get; init; } = new FridayConfigurationDatabase();
    public FridayConfigurationLavalink Lavalink { get; init; } = new FridayConfigurationLavalink();
}