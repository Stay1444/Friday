namespace Friday.Models;

public class ModuleConfigModel
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string[]? Authors { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string[] Require { get; set; } = Array.Empty<string>();
}