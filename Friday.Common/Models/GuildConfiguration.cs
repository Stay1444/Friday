namespace Friday.Common.Models;

public record GuildConfiguration
{
    public string Prefix { get; set; } = "f";
    public string Language { get; set; } = "en";
}