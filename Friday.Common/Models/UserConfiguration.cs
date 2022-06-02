using Friday.Common.Attributes;

namespace Friday.Common.Models;

public record UserConfiguration
{
    [ColumnName("prefix_override")]
    public string? PrefixOverride { get; set; }
    
    [ColumnName("language_override")]
    public string? LanguageOverride { get; set; }
}