using System.Text.Json;
using Miuex.Modules.Proxmox.Models;

namespace Miuex.Modules.Proxmox.Services;

public class ConfigurationService
{
    private readonly string _configurationFilePath;

    private Configuration? _configuration;
    
    
    public ConfigurationService(string path)
    {
        this._configurationFilePath = path;
    }
    
    public Configuration GetConfiguration()
    {
        try
        {
            if (this._configuration == null)
            {
                this._configuration = this.LoadConfiguration();
            }

            return this._configuration;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error loading configuration: " + e.Message);
            throw;
        }
    }
    
    private Configuration LoadConfiguration()
    {
        try
        {
            
            var configuration = new Configuration();
            if (!File.Exists(this._configurationFilePath))
            {
                configuration.ApiUrl = "https://localhost:1444";
                configuration.ApiKey = new Guid().ToString();
                configuration.ConnectionString =
                    "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
                File.WriteAllText(this._configurationFilePath, JsonSerializer.Serialize(configuration, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
                return configuration;
            }

            var json = File.ReadAllText(this._configurationFilePath);
            configuration = JsonSerializer.Deserialize<Configuration>(json);

            if (configuration is not null)
            {
                if (configuration.ApiUrl is null) throw new ArgumentNullException(nameof(configuration.ApiUrl));
                if (configuration.ConnectionString is null) throw new ArgumentNullException(nameof(configuration.ConnectionString));
                if (configuration.ApiKey is null) throw new ArgumentNullException(nameof(configuration.ApiUrl));
                if (configuration.Token is null) throw new ArgumentNullException(nameof(configuration.Token));
            }
            else
            {
                Console.WriteLine("There was a problem loading the configuration file.");
            }


            return configuration ?? new Configuration();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error loading configuration: " + e);
            throw;
        }

    }
}