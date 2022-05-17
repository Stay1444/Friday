using DSharpPlus;
using DSharpPlus.Entities;

namespace Friday.Modules.Backups.Entities;

public record Backup
{
    public record Role
    {
        public ulong Id { get; init; }
        public string Name { get; init; } = String.Empty;
        public int[] Color { get; init; } = Array.Empty<int>();
        public int Position { get; init; }
        public bool IsMentionable { get; init; }
        public bool IsHoisted { get; init; }
        public Permissions Permissions { get; init; }
        
        public Role(){ }

        public Role(DiscordRole role)
        {
            Id = role.Id;
            Name = role.Name;
            Color = new int[] {role.Color.R, role.Color.G, role.Color.B};
            Position = role.Position;
            IsMentionable = role.IsMentionable;
            IsHoisted = role.IsHoisted;
            Permissions = role.Permissions;
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
    
    public async Task From(DiscordGuild guild)
    {
        this.Name = guild.Name;
        this.Icon = guild.IconUrl;
        this.Date = DateTime.UtcNow;
        
        this.Roles = guild.Roles
            .Where(x => !x.Value.IsManaged)
            .Select(x => new Role(x.Value)).ToList();


        foreach (var guildChannel in guild.Channels)
        {
            var channel = new Channel();
            await channel.From(guildChannel.Value);
            Channels.Add(channel);
        }

    }
    
}