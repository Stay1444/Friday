using Friday.Common.Attributes;

namespace Friday.Modules.Moderation.Models;

public record UserBanState
{
    [ColumnName("user_id")]
    public ulong UserId { get; init; }
    [ColumnName("guild_id")]
    public ulong GuildId { get; init; }
    [ColumnName("banned_by")]
    public ulong BannedBy { get; init; }
    [ColumnName("reason")]
    public string? Reason { get; init; }
    [ColumnName("banned_at")]
    public DateTime? BanDate { get; init; }
    [ColumnName("expires_at")]
    public DateTime? Expiration { get; init; }
}