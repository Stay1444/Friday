using System.Reflection;

namespace Friday.Modules.Help.Entities;

public class CommandDefinition
{
    public string Name { get; set; } = string.Empty;
    public string[] Aliases { get; set; } = Array.Empty<string>();
}