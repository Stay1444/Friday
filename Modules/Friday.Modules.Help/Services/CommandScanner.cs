using System.Reflection;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Modules.Help.Entities;

namespace Friday.Modules.Help.Services;

public class CommandScanner
{
    private IModuleManager _moduleManager;

    private Dictionary<IModuleInfo, List<CommandDefinition>> _definitions =
        new Dictionary<IModuleInfo, List<CommandDefinition>>();

    internal CommandScanner(IModuleManager moduleManager)
    {
        this._moduleManager = moduleManager;
    }

    public void Build()
    {
        foreach (var moduleInfo in _moduleManager.Modules)
        {
            _definitions.Add(moduleInfo, new List<CommandDefinition>());
            var assembly = moduleInfo.Assembly;
            var methods = 
                (from type in assembly.GetTypes()
                from method in type.GetMethods()
                where method.GetCustomAttribute<CommandAttribute>() is not null select method);

            foreach (var method in methods)
            {
                
            }
        }
    }

    public IReadOnlyList<CommandDefinition> GetCommands(IModuleInfo moduleInfo)
    {
        return _definitions[moduleInfo];
    }

    public IReadOnlyList<CommandDefinition> ResolveCommandsAsync(IModuleInfo moduleInfo, CommandContext ctx)
    {
        var commands = GetCommands(moduleInfo);
        return new List<CommandDefinition>();
    }
}