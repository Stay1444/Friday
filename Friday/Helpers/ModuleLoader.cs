using System.Reflection;
using Friday.Common;
using Friday.Common.Entities;
using Friday.Common.Enums;
using Friday.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomlyn;

namespace Friday.Helpers;

public class ModuleLoader
{
    public static IModule[] LoadModules(ServiceCollection services)
    {
        var modules = new List<IModule>();
        
        if (!Directory.Exists("modules"))
        {
            Directory.CreateDirectory("modules");
            return modules.ToArray();
        }
        
        if (Directory.GetFiles("modules").Length == 0)
        {
            return modules.ToArray();
        }

        foreach (var modulePath in Directory.GetFiles("modules", "*.dll"))
        {
            var moduleAssembly = Assembly.LoadFrom(modulePath);
            if (modules.Any(x => x.GetType().Assembly == moduleAssembly))
            {
                continue;
            }
            LoadModule(moduleAssembly, services, modules);
        }
        
        return modules.ToArray();
    }

    private static ModuleConfigModel? ReadConfig(Assembly module)
    {
        try
        {
            var res = Resource.Load(module, "Resources/module_config.toml", ResourceLifeSpan.Temporary);

            return Toml.ToModel<ModuleConfigModel>(res.ReadString());
        }
        catch (FileNotFoundException)
        {
            return null; // No config file
        }catch (Exception e)
        {
            Log.Error(e, "Failed to load module config");
            return null;
        }

    }
    
    private static void LoadModule(Assembly moduleAssembly, ServiceCollection services, List<IModule> loadedModules)
    {
        if (!IsValid(moduleAssembly))
        {
            Log.Warning("Module {0} does not implement {1} interface. Skipping.", 
                moduleAssembly.GetName().Name,
                nameof(IModule));
            return;
        }
        
        var moduleConfig = ReadConfig(moduleAssembly);
        
        if (moduleConfig is not null)
        {
            /*
             * Check if this module requires any other module. If so, check if they are loaded.
             * Load them if they are not loaded.
             * Required modules are specified in the config file.
             * Required module format: Module.Assembly.Name
             */
            foreach (var requiredModule in moduleConfig.RequiredModules)
            {
                if (!IsModuleLoaded(requiredModule, loadedModules))
                {
                    Log.Information("Module {0} requires module {1} which is not loaded. Loading module {1}.",
                        moduleAssembly.GetName().Name,
                        requiredModule);
                        
                    if (!File.Exists("modules/" + requiredModule + ".dll"))
                    {
                        Log.Error("Module {0} requires module {1} which is not loaded. Module {1} is not found.",
                            moduleAssembly.GetName().Name,
                            requiredModule);
                        return;
                    }
                    
                    
                    LoadModule(Assembly.LoadFrom($"modules/{requiredModule}.dll"), services, loadedModules);
                }
            }
        }
        
        var module = (IModule?) ActivatorUtilities.CreateInstance(services.BuildServiceProvider(), GetModuleType(moduleAssembly));
        
        Log.Information("Module {0} loaded.", moduleAssembly.GetName().Name);
        module!.OnLoad().GetAwaiter().GetResult();

        loadedModules.Add(module);
        services.AddSingleton(module.GetType(),module);
    }

    private static bool IsValid(Assembly module)
    {
        return module.GetTypes().Any(t => t.GetInterfaces().Contains(typeof(IModule)));
    }

    private static Type GetModuleType(Assembly module)
    {
        return module.GetTypes().First(t => t.GetInterfaces().Contains(typeof(IModule)));
    }
    private static bool IsModuleLoaded(string assemblyName, List<IModule> loadedModules)
    {
        foreach (var module in loadedModules)
        {
            if (module.GetType().Assembly.GetName().Name == assemblyName)
            {
                return true;
            }
        }
        
        return false;
    }

    public static Assembly[] GetValidAssemblies()
    {
        var assemblies = new List<Assembly>();

        foreach (var modulePath in Directory.GetFiles("modules", "*.dll"))
        {
            if (IsValid(Assembly.LoadFrom(modulePath)))
            {
                assemblies.Add(Assembly.LoadFrom(modulePath));
            }
        }
        
        return assemblies.ToArray();
    }
}