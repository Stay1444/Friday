using System.Text.Json.Serialization;

namespace Friday.Modules.Minesprout.Minesprout.Entities;

public record MinesproutServerListing : IMinesproutServer
{
    public required string? Name { get; set; }
    [JsonPropertyName("desc")]
    public required string Description { get; set; }
    [JsonPropertyName("iconURL")]
    public required string IconUrl { get; set; }
    [JsonPropertyName("bannerURL")]
    public required string BannerURL { get; set; }
    public required string Ip { get; set; }
    public int Id { get; set; }
    public required string MinVersion { get; set; }
    public required string MaxVersion { get; set; }
    public required string MainMode { get; set; }
    public required string Type { get; set; }
    public required string Country { get; set; }
    public MinesproutServerStatus? Status { get; set; }
}
