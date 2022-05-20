using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Backups.Entities;

namespace Friday.Modules.Backups.Services;

public class BackupService
{
    private BackupsModule _module;
    internal BackupService(BackupsModule module)
    {
        this._module = module;   
    }

    public async Task<(long id, string code, Backup backup)?> CreateBackupAsync(DiscordGuild guild, DiscordUser owner)
    {
        var code = new char[8].RandomAlphanumeric();

        while (await _module.Database.GetBackupAsync(code) is not null)
        {
            code = new char[8].RandomAlphanumeric();
        }

        var backup = new Backup();
        await backup.From(_module, guild);
        
        var id = DateTime.UtcNow.Ticks;
        await _module.Database.InsertBackupAsync(id, backup, code, owner.Id);
        
        return (id, code, backup);
    }

    public async Task LoadBackupAsync(DiscordGuild guild, Backup bk, string? caller)
    {
        var (_, used) = await _module.RoleCooldownService.GetUsed(guild.Id);
        if (used + bk.Roles.Count > RoleCooldownService.RoleCountPerDay) throw new Exception("Role quota exceeded");

        Stream? guildIcon = null;
        if (!string.IsNullOrEmpty(bk.Icon))
        {
            guildIcon = await _module.CdnClient.DownloadAsync(Guid.Parse(bk.Icon));
        }
        
        await guild.ModifyAsync(x =>
        {
            x.Name = bk.Name;
            if (guildIcon is not null)
            {
                x.Icon = guildIcon;
            }
        });
        
        var channelDeletionTask = Task.Run(async () =>
        {
            foreach (var discordChannel in guild.Channels)
            {
                try
                {
                    await discordChannel.Value.DeleteAsync();
                }
                catch
                {
                    // ignored
                }
            }
        });
        
        foreach (var (_, value) in guild.Roles)
        {
            if (value is null) continue;
            if (value.IsManaged) continue;
            try
            {
                await value.DeleteAsync($"Loading Backup {caller}");
            }catch 
            { 
                // ignore //
            }
        }

        var roleMapping = new Dictionary<ulong, ulong>();
        await _module.RoleCooldownService.AddToUsed(guild.Id, bk.Roles.Count);
        foreach (var role in bk.Roles.OrderByDescending(x => x.Position))
        {
            if (!role.IsEveryone)
            {
                Stream? icon = null;
                if (role.Icon is not null)
                {
                    icon = await _module.CdnClient.DownloadAsync(Guid.Parse(role.Icon));
                }
                
                var newRole = await guild.CreateRoleAsync(role.Name, role.Permissions,
                    new DiscordColor(role.Color[0], role.Color[1], role.Color[2]), role.IsHoisted, role.IsMentionable,
                    $"Loading Backup {caller}", icon);
                
                roleMapping.Add(role.Id, newRole.Id);
            }
            else
            {
                roleMapping.Add(role.Id, guild.EveryoneRole.Id);
                await guild.EveryoneRole.ModifyAsync(x =>
                {
                    x.Permissions = role.Permissions;
                    x.Color = new DiscordColor(role.Color[0], role.Color[1], role.Color[2]);
                    x.Hoist = role.IsHoisted;
                    x.Mentionable = role.IsMentionable;
                });
            }
        }

        await channelDeletionTask;

        var parentMapping = new Dictionary<ulong, ulong>();
        
        foreach (var category in bk.Channels.Where(x => x.Type == ChannelType.Category).OrderBy(x => x.Position))
        {
            var overwrites = new List<DiscordOverwriteBuilder>();

            foreach (var overwrite in category.Overwrites)
            {
                if (overwrite.MemberId is not null) continue;
                if (overwrite.RoleId is null) continue;
                var role = guild.Roles.Values.FirstOrDefault(x => x.Id == roleMapping[overwrite.RoleId.Value]);
                if (role is null) continue;
                
                overwrites.Add(new DiscordOverwriteBuilder(role).Allow(overwrite.Allowed).Deny(overwrite.Denied));
            }
            
            var newCategory = await guild.CreateChannelAsync(category.Name, category.Type, null, category.Topic,
                category.Bitrate, category.UserLimit, overwrites, category.Nsfw,
                category.PerUserRateLimit, null, null, $"Loading Backup {caller}");
            
            parentMapping.Add(category.Id, newCategory.Id);
        }

        foreach (var channel in bk.Channels.Where(x => x.Type != ChannelType.Category).OrderBy(x => x.Position))
        {
            var overwrites = new List<DiscordOverwriteBuilder>();

            foreach (var overwrite in channel.Overwrites)
            {
                if (overwrite.MemberId is not null) continue;
                if (overwrite.RoleId is null) continue;
                
                var role = guild.Roles.Values.FirstOrDefault(x => x.Id == roleMapping[overwrite.RoleId.Value]);
                
                if (role is null) continue;
                
                overwrites.Add(new DiscordOverwriteBuilder(role).Allow(overwrite.Allowed).Deny(overwrite.Denied));
            }
            
            DiscordChannel? parent = null;
            if (channel.ParentId is not null)
            {
                parent = guild.Channels.Values.FirstOrDefault(x => x.Id == parentMapping[channel.ParentId.Value]);
            }
            
            await guild.CreateChannelAsync(channel.Name, channel.Type, parent, channel.Topic,
                channel.Bitrate, channel.UserLimit, overwrites, channel.Nsfw,
                channel.PerUserRateLimit, null, null, $"Loading Backup {caller}");
        }
    }
}