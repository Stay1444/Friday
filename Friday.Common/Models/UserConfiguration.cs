using Friday.Common.Attributes;

namespace Friday.Common.Models;

public record UserConfiguration
{
    [ColumnName("prefix_override")]
    public string? PrefixOverride { get; init; }
    
    [ColumnName("language_override")]
    public string? LanguageOverride { get; init; }
}