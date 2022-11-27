using System.Reflection;
using Friday.Common;
using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Entities;
using Friday.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Friday.Services;

public class ModuleManager : IModuleManager
{
    private const string MODULE_FOLDER = "modules";
    
    private List<ModuleInfo> _modules;
    public IReadOnlyList<IModuleInfo> Modules => _modules;

    public ModuleManager()
    {
        this._modules = new List<ModuleInfo>();
    }

    bool IsValid(Assembly assembly)
    {
        return assembly.GetTypes().Any(t => t.IsSubclassOf(typeof(ModuleBase)));
    }

    private void LoadModule(Assembly assembly, string assemblyPath)
    {
        var moduleInfo = new ModuleInfo(assembly.GetName().Name ?? Path.GetFileName(assemblyPath),
            "1.0.0", new []{ "Unknown" },
            "Unknown",  null, null, assembly);

        try
        {
            var resource = Resource.Load(assembly, "Resources/module.yaml");
            var config = FridayYaml.Deserializer.Deserialize<ModuleConfigModel>(resource.ReadString());

            if (!string.IsNullOrEmpty(config.Name))
            {
                moduleInfo.Name = config.Name;
            }

            if (!string.IsNullOrEmpty(config.Version))
            {
                moduleInfo.Version = config.Version;
            }

            if (config.Authors is not null)
            {
                moduleInfo.Authors = config.Authors;
            }

            if (config.Description is not null)
            {
                moduleInfo.Description = config.Description;
            }

            if (config.Icon is not null)
            {
                moduleInfo.Icon = config.Icon;
            }

            moduleInfo.ConfigModel = config;
        }
        catch (Exception error)
        {
            Log.Error(error, "Error while loading module configuration");
            return;
        }
                
        _modules.Add(moduleInfo);
    }

    public void LoadModule<T>()
    {
        var assembly = typeof(T).Assembly;
        LoadModule(assembly, "unknown");
    }
    
    public void LoadModules()
    {
        if (!Directory.Exists(MODULE_FOLDER))
        {
            Directory.CreateDirectory(MODULE_FOLDER);
        }

        foreach (var assemblyPath in Directory.GetFiles(MODULE_FOLDER, "*.dll"))
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            if (IsValid(assembly))
            {
                LoadModule(assembly, assemblyPath);
            }
        }
    }

    public void CreateInstances(ServiceCollection services)
    {
        void CreateInstance(ModuleInfo module)
        {
            try
            {
                if (module.ConfigModel is not null && module.ConfigModel.Require.Any())
                {
                    foreach (var requiredAssembly in module.ConfigModel.Require)
                    {
                        if (!_modules.Any(x => x.Assembly.GetName().Name == requiredAssembly && x.Instance == null))
                        {
                            continue;
                        }

                        CreateInstance(_modules.First(x => x.Assembly.GetName().Name == requiredAssembly));
                    }
                }

                Type GetModuleType(Assembly assembly)
                {
                    return assembly.GetTypes().First(t => t.IsSubclassOf(typeof(ModuleBase)));
                }

                var instance = (ModuleBase?) ActivatorUtilities.CreateInstance(services.BuildServiceProvider(),
                    GetModuleType(module.Assembly));
                if (instance is null)
                {
                    Log.Error("Error loading {asm}", module.Assembly.GetName().Name);
                    return;
                }

                instance.Module = module;
                module.Instance = instance;
                services.AddSingleton(instance.GetType(), instance);
                Log.Information("Module {name} ({assembly}) by {authors} loaded!", module.Name,
                    module.Assembly.GetName().Name, string.Join(", ", module.Authors));
            }
            catch (Exception error)
            {
                Log.Error(error, "Fatal error while loading {assembly}", module.Assembly.FullName);
            }
        }
        
        foreach (var moduleInfo in _modules)
        {
            if (moduleInfo.Instance is not null) continue;
            CreateInstance(moduleInfo);
        }

        _modules.RemoveAll(x => x.Instance == null);
    }
    
    
}

