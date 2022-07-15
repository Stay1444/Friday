using System.Reflection;
using Friday.Common;
using Friday.Common.Entities;
using Friday.Models;

namespace Friday.Entities;

public class ModuleInfo : IModuleInfo
{
    public ModuleInfo(string name, string version, string[] authors, string description, string? icon, ModuleBase? instance, Assembly assembly)
    {
        Name = name;
        Version = version;
        Authors = authors;
        Description = description;
        Icon = icon;
        Instance = instance;
        Assembly = assembly;
    }

    public string Name { get; set; }
    public string Version { get; set; }
    public string[] Authors { get; set; }
    public string Description { get; set; }
    public string? Icon { get; set; }
    public ModuleBase? Instance { get; set; }
    public Assembly Assembly { get; set; }
 
    public ModuleConfigModel? ConfigModel { get; set; }
}