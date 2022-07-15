using System.Reflection;

namespace Friday.Common.Entities;

public interface IModuleInfo
{
    public string Name { get; }
    public string Version { get; }
    public string[] Authors { get; }
    public string Description { get; }
    public string? Icon { get; }
    
    public ModuleBase? Instance { get; }
    
    public Assembly Assembly { get; }
}

