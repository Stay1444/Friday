using Friday.Common;
using Friday.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomlyn;

namespace Friday.Helpers;

public static class Startup
{
    public static void CleanTempFiles()
    {
        if (Directory.Exists(".tmp"))
        {
            Directory.Delete(".tmp", true);
        }
    }
    
    public static void LoggerStartup()
    {
        Directory.CreateDirectory("logs");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Hour)
            .CreateLogger();
    }
    
    public static FridayConfiguration LoadConfiguration()
    {
        Directory.CreateDirectory("conf");
        if (!File.Exists("config.toml"))
        {
            File.WriteAllText("config.toml", Toml.FromModel(new FridayConfiguration()));
        }

        var config = Toml.ToModel<FridayConfiguration>(File.ReadAllText("config.toml"));
        
        return config;
    }

    public static ModuleBase[] LoadModules(ServiceCollection services)
    {
        return ModuleLoader.LoadModules(services);
    }
}