using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Friday.Common.Services;
using Friday.Helpers;
using Friday.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomlyn;
try
{
    Startup.CleanTempFiles();

    Startup.LoggerStartup();

    Log.Information("Starting up");

    Log.Information("Reading config...");
    
    var config = Startup.LoadConfiguration();
    
    Log.Information("Config read successfully");
    
    Log.Information("Creating Discord client...");
    
    var services = new ServiceCollection();

    services.AddSingleton(config);
    
    var client = new DiscordShardedClient(new()
    {
        AutoReconnect = true,
        ReconnectIndefinitely = true,
        Intents = DiscordIntents.All,
        Token = config.Discord.Token
    });
    
    services.AddSingleton(client);
    
    var dbProvider = new DatabaseProvider(config);
    services.AddSingleton(dbProvider);

    var guildConfigurationProvider = new GuildConfigurationProvider(dbProvider);
    services.AddSingleton(guildConfigurationProvider);
    
    var userConfigurationProvider = new UserConfigurationProvider(dbProvider);
    services.AddSingleton(userConfigurationProvider);
    
    var prefixResolver = new PrefixResolver(dbProvider, guildConfigurationProvider, userConfigurationProvider);
    services.AddSingleton(prefixResolver);

    var languageProvider = new LanguageProvider(dbProvider, userConfigurationProvider, guildConfigurationProvider, ModuleLoader.GetValidAssemblies());
    languageProvider.Build();
    services.AddSingleton(languageProvider);

    Log.Information("Loading modules");
    
    var modules = Startup.LoadModules(services);
    if (!modules.Any())
    {
        Log.Information("No modules found");
    }else
    {
        Log.Information("Loaded {0} modules", modules.Length);
    }
    
    
    
    var serviceProvider = services.BuildServiceProvider();

    Log.Information("Discord client created successfully");
    if (config.Lavalink.Enabled)
    {
        await client.UseLavalinkAsync();
        Log.Information("Lavalink client created successfully");
    }

    

    
    await client.UseInteractivityAsync();
    var commandsNextExtensions = await client.UseCommandsNextAsync(new()
    {
        CaseSensitive = false,
        EnableDefaultHelp = false,
        EnableDms = true,
        IgnoreExtraArguments = false,
        EnableMentionPrefix = true,
        Services = serviceProvider,
        PrefixResolver = prefixResolver.ResolvePrefixAsync
    });

    foreach (var module in modules)
    {
        commandsNextExtensions.RegisterCommands(module.GetType().Assembly);
    }
    
    await client.UseSlashCommandsAsync();




    Log.Information("Starting client");

    await client.StartAsync();
    if (config.Lavalink.Enabled)
    {
        Log.Information("Lavalink connecting...");
        try
        {
            var lavaLink = await client.GetLavalinkAsync();
            foreach (var lavalinkExtension in lavaLink)
            {
                var node = await lavalinkExtension.Value.ConnectAsync(new()
                {
                    SocketAutoReconnect = true,
                    SocketEndpoint = new()
                    {
                        Hostname = config.Lavalink.Host,
                        Port = config.Lavalink.Port,
                    },
                    RestEndpoint = new()
                    {
                        Hostname = config.Lavalink.Host,
                        Port = config.Lavalink.Port,
                    },
                    Password = config.Lavalink.Password
                });
                node.Disconnected += (_, _) =>
                {
                    Log.Error("Lavalink node disconnected");
                    return Task.CompletedTask;
                };
                Log.Information("Lavalink connected to node {node}.", node.NodeEndpoint);
            }
        }
        catch (Exception e)
        {
            Log.Error(e,"Lavalink connection failed");
            await client.StopAsync();
            return;
        }
    }

    await Task.Delay(-1);
}
catch(Exception e)
{
    Log.Error(e, "Fatal error");
}