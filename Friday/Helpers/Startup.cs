using Friday.Common;
using Friday.Common.Models;
using Serilog;

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

    public static bool DoesConfigurationExist()
    {
        return File.Exists("config/friday.yaml");
    }
    
    public static FridayConfiguration LoadConfiguration()
    {
        Directory.CreateDirectory("config");
        if (!File.Exists("config/friday.yaml"))
        {
            File.WriteAllText("config/friday.yaml", FridayYaml.Serializer.Serialize(new FridayConfiguration()));
        }

        var config = FridayYaml.Deserializer.Deserialize<FridayConfiguration>(File.ReadAllText("config/friday.yaml"));
        
        return config;
    }

}