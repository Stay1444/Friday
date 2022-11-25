using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.Moderation.Models;
using Serilog;
using Timer = System.Timers.Timer;

namespace Friday.Modules.Moderation;

public class ModerationModuleBase : ModuleBase
{

    private DatabaseProvider _db;
    private Timer _banTimer;
    private ModerationConfiguration? _config;
    private DiscordShardedClient _client;
    public ModerationModuleBase(DatabaseProvider db, DiscordShardedClient client)
    {
        _db = db;
        _client = client;
        _banTimer = new Timer(60000);
    }

    public override async Task OnLoad()
    {
        Log.Information("[Moderation] Module loaded.");
        Log.Information("[Moderation] Loading config...");
        _config = await ReadConfiguration<ModerationConfiguration>();
        Log.Information("[Moderation] Config loaded.");
        Log.Information("[Moderation] Starting ban timer...");
        _banTimer = new Timer(_config.BanCheckIntervalSeconds * 1000);
        _banTimer.Start();
        _banTimer.AutoReset = false;
        _banTimer.Elapsed += (a,b) => _ = BanTimerOnElapsed();
        Log.Information("[Moderation] Ban timer started. Interval: {0} seconds", _config.BanCheckIntervalSeconds);
    }

    public override Task OnUnload()
    {
        throw new NotImplementedException();
    }

    private async Task BanTimerOnElapsed()
    {
        Log.Debug("[Moderation] Checking for bans...");
        var bans = await GetAllBans();
        
        foreach (var ban in bans)
        {
            if (ban.Expiration is null)
                continue;
            
            if (ban.Expiration < DateTime.UtcNow)
            {
                await Unban(ban);
            }
        }
        
        _banTimer.Start();
    }

    public Task<UserBanState?> GetBanState(DiscordMember member)
    {
        return GetBanState(member.Guild.Id, member.Id);
    }
    
    public Task<UserBanState?> GetBanState(DiscordGuild guild, DiscordUser user)
    {
        return GetBanState(guild.Id, user.Id);
    }
    
    public async Task<UserBanState?> GetBanState(ulong guildId, ulong userId)
    {
        var banState = await _db.QueryFirstOrDefaultAsync<UserBanState>(
            "SELECT * FROM mod_user_bans WHERE user_id = @UserId AND guild_id=@GuildId", new
            {
                UserId = userId,
                GuildId = guildId
            });

        return banState;
    }

    public async Task<UserBanState?> BanAsync(DiscordMember member, DiscordMember bannedBy, string? reason, DateTime? Expiration)
    {
        if (await GetBanState(member) is not null)
        {
            throw new Exception("User is already banned");
        }
        
        var banState = new UserBanState
        {
            UserId = member.Id,
            GuildId = member.Guild.Id,
            BannedBy = bannedBy.Id,
            Reason = reason,
            BanDate = DateTime.UtcNow,
            Expiration = Expiration
        };
        
        await _db.ExecuteAsync("INSERT INTO mod_user_bans (user_id, guild_id, banned_by, reason, banned_at, expires_at) VALUES (@UserId, @GuildId, @BannedBy, @Reason, @BanDate, @Expiration)", banState);
        await member.BanAsync(0, reason ?? "No reason provided");
        return banState;
    }

    internal Task RemoveBanState(UserBanState banState)
    {
        return RemoveBanState(banState.UserId, banState.GuildId);
    }
    
    internal Task RemoveBanState(ulong userId, ulong guildId)
    {
        return _db.ExecuteAsync("DELETE FROM mod_user_bans WHERE user_id = @UserId AND guild_id=@GuildId", new {UserId = userId, GuildId = guildId});
    }
    
    public async Task<UserBanState?> BanAsync(ulong guildId, ulong memberId, ulong bannedBy, string? reason, DateTime? Expiration)
    {
        if (await GetBanState(guildId, memberId) is not null)
        {
            throw new Exception("User is already banned");
        }
        
        var banState = new UserBanState
        {
            UserId = memberId,
            GuildId = guildId,
            BannedBy = bannedBy,
            Reason = reason,
            BanDate = DateTime.UtcNow,
            Expiration = Expiration
        };
        
        await _db.ExecuteAsync("INSERT INTO mod_user_bans (user_id, guild_id, banned_by, reason, banned_at, expires_at) VALUES (@UserId, @GuildId, @BannedBy, @Reason, @BanDate, @Expiration)", banState);
        var guild = await _client.GetGuildAsync(guildId);
        if (guild is null)
        {
            throw new Exception("Guild not found");
        }
        
        await guild.BanMemberAsync(memberId, 0, reason ?? "No reason provided");
        return banState;
    }
    
    public async Task<UserBanState[]> GetBans(ulong guildId)
    {
        var r = await _db.QueryAsync<UserBanState>(
            "SELECT * FROM mod_user_bans WHERE guild_id = @GuildId", new
            {
                GuildId = guildId
            });
        
        return r.ToArray();
    }

    public async Task<UserBanState[]> GetAllBans()
    {
        var r = await _db.QueryAsync<UserBanState>(
            "SELECT * FROM mod_user_bans");
        
        return r.ToArray();
    }
    
    public async Task Unban(UserBanState ban)
    {
        await _db.ExecuteAsync("DELETE FROM mod_user_bans WHERE user_id = @UserId AND guild_id = @GuildId", new
        {
            ban.UserId,
            ban.GuildId
        });

        var guild = await _client.GetGuildAsync(ban.GuildId);
        
        if (guild is null)
            return;
        
        await guild.UnbanMemberAsync(ban.UserId, "Unbanned by moderator module");
    }

    public async Task Unban(DiscordGuild guild, DiscordUser user)
    {
        await _db.ExecuteAsync("DELETE FROM mod_user_bans WHERE user_id = @UserId AND guild_id = @GuildId", new
        {
            UserId = user.Id,
            GuildId = guild.Id
        });
        
        await guild.UnbanMemberAsync(user, "Unbanned by moderation module");
    }
    
    private Task UpdateBanState(ulong guildId, ulong userId, UserBanState state)
    {
        return _db.ExecuteAsync("UPDATE mod_user_bans SET banned_by = @BannedBy, reason = @Reason, banned_at = @BannedAt, expires_at = @ExpiresAt WHERE user_id = @UserId AND guild_id = @GuildId", new
        {
            state.BannedBy,
            state.Reason,
            BannedAt = state.BanDate,
            ExpiresAt = state.Expiration,
            UserId = userId,
            GuildId = guildId
        });
    }
}