using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;

namespace Friday.Modules.Backups.Entities;

public record Backup
{
    private static HttpClient Client { get; } = new HttpClient();
    public record Role
    {
        public ulong Id { get; init; }
        public string Name { get; init; } = String.Empty;
        public byte[] Color { get; init; } = Array.Empty<byte>();
        public int Position { get; init; }
        public bool IsMentionable { get; init; }
        public bool IsHoisted { get; init; }
        public Permissions Permissions { get; init; }
        public bool IsEveryone { get; init; }
        public string? Icon { get; set; }
        public Role(){ }

        public Role(DiscordGuild guild, DiscordRole role)
        {
            Id = role.Id;
            Name = role.Name;
            Color = new byte[] {role.Color.R, role.Color.G, role.Color.B};
            Position = role.Position;
            IsMentionable = role.IsMentionable;
            IsHoisted = role.IsHoisted;
            Permissions = role.Permissions;
            IsEveryone = guild.EveryoneRole.Id == role.Id;
        }

        public async Task From(BackupsModule module, DiscordRole role)
        {
            if (string.IsNullOrEmpty(role.IconHash))
            {
                return;
            }

            try
            {
                var response = await Client.GetAsync(role.IconUrl);
                var icon = await response.Content.ReadAsStreamAsync();
                var iconName = $"bak_R_{role.Id}.png";
                var id = await module.CdnClient.UploadAsync(iconName, icon, true);
                Icon = id.ToString();
            }catch(Exception e)
            {
                Log.Error(e, "Failed to upload role icon");
            }
        }
    }

    public record Overwrite
    {
        public Permissions Allowed { get; set; }
        public Permissions Denied { get; set; }
        public OverwriteType Type { get; set; }
        public ulong Id { get; set; }
        
        public ulong? MemberId { get; set; }
        public ulong? RoleId { get; set; }
        
        public async Task From(DiscordOverwrite overwrite)
        {
            Allowed = overwrite.Allowed;
            Denied = overwrite.Denied;
            Type = overwrite.Type;
            Id = overwrite.Id;

            if (Type == OverwriteType.Member)
            {
                var member = await overwrite.GetMemberAsync();
                MemberId = member.Id;
            }else if (Type == OverwriteType.Role)
            {
                var role = await overwrite.GetRoleAsync();
                RoleId = role.Id;
            }else
            {
                throw new ArgumentException("Invalid Overwrite Type");
            }
        }
    }

    public record Channel
    {
        public ulong Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string? Topic { get; set; }
        public int? Bitrate { get; set; }
        public int Position { get; set; }
        public ulong? ParentId { get; set; }
        public ChannelType Type { get; set; }
        public int? UserLimit { get; set; }
        public bool? Nsfw { get; set; }
        public int? PerUserRateLimit { get; set; }
        
        public List<Overwrite> Overwrites { get; init; } = new List<Overwrite>();

        public async Task From(DiscordChannel channel)
        {
            Id = channel.Id;
            Name = channel.Name;
            Topic = channel.Topic;
            Bitrate = channel.Bitrate;
            Position = channel.Position;
            ParentId = channel.Parent?.Id;
            Type = channel.Type;
            UserLimit = channel.UserLimit;
            Nsfw = channel.IsNSFW;
            PerUserRateLimit = channel.PerUserRateLimit;

            foreach (var overwrite in channel.PermissionOverwrites)
            {
                var newOverwrite = new Overwrite();
                await newOverwrite.From(overwrite);
                Overwrites.Add(newOverwrite);
            }
        }
    }
    
    public string Name { get; set; } = String.Empty;
    public string Icon { get; set; } = String.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    
    public List<Role> Roles { get; set; } = new List<Role>();

    public List<Channel> Channels { get; set; } = new List<Channel>();
    
    public async Task From(BackupsModule module, DiscordGuild guild)
    {
        this.Name = guild.Name;
        this.Date = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(guild.IconHash))
        {
            var response = await Client.GetAsync(guild.IconUrl);
            var icon = await response.Content.ReadAsStreamAsync();
            var iconName = $"bak_G_{guild.Id}.png";
            var id = await module.CdnClient.UploadAsync(iconName, icon, true);
            this.Icon = id.ToString();
        }
        
        foreach (var role in guild.Roles
                     .Where(x => !x.Value.IsManaged))
        {
            var bRole = new Role(guild, role.Value);
            await bRole.From(module, role.Value);
            Roles.Add(bRole);
        }
        
        foreach (var guildChannel in guild.Channels)
        {
            var channel = new Channel();
            await channel.From(guildChannel.Value);
            Channels.Add(channel);
        }
    }
    
}