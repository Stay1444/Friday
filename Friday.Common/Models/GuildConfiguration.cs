namespace Friday.Common.Models;

public record GuildConfiguration
{
    public string Prefix { get; init; } = "f";
    public string Language { get; init; } = "en";
}