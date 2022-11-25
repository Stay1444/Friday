using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Friday.Common.Entities;
using Serilog;

namespace Friday.Common;

public abstract class ModuleBase
{
    private const string CONFIGURATION_DIRECTORY = "config";
    public IModuleInfo? Module { get; } = null;
    public abstract Task OnLoad();
    public abstract Task OnUnload();
    
    public virtual async Task HandleFailedChecks(Type failedCheck, CommandsNextExtension extension,
        CommandErrorEventArgs args)
    {
        await args.Context.Channel.SendMessageAsync("Unhandled exception in module " + this.GetType().Name + " - " + failedCheck.Name);
    }

    protected async Task<T> ReadConfiguration<T>() where T :  class, new()
    {
        try
        {
            if (Module is null) throw new Exception("Not ready");
            Directory.CreateDirectory(CONFIGURATION_DIRECTORY);
            if (!File.Exists(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!)))
            {
                var def = new T();
                await File.WriteAllTextAsync(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!),
                    FridayYaml.Serializer.Serialize(def));
                return def;
            }

            var deserialized = FridayYaml.Deserializer.Deserialize<T>(
                await File.ReadAllTextAsync(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!)));

            return deserialized;
        }
        catch(Exception error)
        {
            Log.Error(error, "Error loading module {0} configuration", Module!.Name);
        }

        return new T();
    }

    protected async Task SaveConfiguration<T>(T config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (Module is null) throw new Exception("Not ready");
        Directory.CreateDirectory(CONFIGURATION_DIRECTORY);
        await File.WriteAllTextAsync(Path.Combine(CONFIGURATION_DIRECTORY, Module.Assembly.GetName().Name!), FridayYaml.Serializer.Serialize(config));
    }
    
    public virtual void RegisterSlashCommands(SlashCommandsExtension extension) { }
}