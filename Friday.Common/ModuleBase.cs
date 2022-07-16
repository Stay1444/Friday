using System.Text.Json;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Friday.Common.Entities;

namespace Friday.Common;

public abstract class ModuleBase
{
    private const string CONFIGURATION_DIRECTORY = "modules/config";
    public IModuleInfo? Module { get; } = null;
    public abstract Task OnLoad();
    public abstract Task OnUnload();
    
    public virtual async Task HandleFailedChecks(Type failedCheck, CommandsNextExtension extension,
        CommandErrorEventArgs args)
    {
        await args.Context.Channel.SendMessageAsync("Unhandled exception in module " + this.GetType().Name + " - " + failedCheck.Name);
    }

    protected async Task<T> ReadConfiguration<T>(T def)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
        if (Module is null) throw new Exception("Not ready");
        Directory.CreateDirectory(CONFIGURATION_DIRECTORY);
        if (!File.Exists(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!)))
        {
            await File.WriteAllTextAsync(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!),
                JsonSerializer.Serialize(def));
            return def;
        }

        var json = JsonSerializer.Deserialize<T>(
            await File.ReadAllTextAsync(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!)));

        if (json is null)
        {
            return def;
        }

        return json;
    }

    protected async Task SaveConfiguration<T>(T config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (Module is null) throw new Exception("Not ready");
        Directory.CreateDirectory(CONFIGURATION_DIRECTORY);
        await File.WriteAllTextAsync(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!), JsonSerializer.Serialize(config, new JsonSerializerOptions()
        {
            WriteIndented = true
        }));
    }
    
    public virtual void RegisterSlashCommands(SlashCommandsExtension extension) { }
}