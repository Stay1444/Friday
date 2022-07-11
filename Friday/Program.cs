using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using SimpleCDN.Wrapper;

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

    var client = new DiscordShardedClient(new DiscordConfiguration
    {
        AutoReconnect = true,
        ReconnectIndefinitely = true,
        Intents = DiscordIntents.All,
        Token = config.Discord.Token,
        LoggerFactory = new SerilogLoggerFactory().AddSerilog(Log.Logger)
    });

    services.AddSingleton(client);

    var langModuleAssemblies = ModuleLoader.GetValidAssemblies();
    var fridayAssemblyProvider =
        new FridayAssemblyCollector(langModuleAssemblies, Assembly.GetAssembly(typeof(Program))!);
    services.AddSingleton(fridayAssemblyProvider);
    var dbProvider = new DatabaseProvider(config);
    services.AddSingleton(dbProvider);
    services.AddSingleton(new FridayModeratorService(dbProvider));
    services.AddSingleton(new FridayVerifiedServerService(dbProvider));
    var guildConfigProvider = new GuildConfigurationProvider(dbProvider);
    services.AddSingleton(guildConfigProvider);
    var userConfigProvider = new UserConfigurationProvider(dbProvider);
    services.AddSingleton(userConfigProvider);
    var prefixResolver = new PrefixResolver(dbProvider, guildConfigProvider, userConfigProvider);
    services.AddSingleton(prefixResolver);
    services.AddSingleton(new LanguageProvider(dbProvider, userConfigProvider, guildConfigProvider,
        fridayAssemblyProvider));
    services.AddSingleton(new SimpleCdnClient(config.SimpleCdn.Host, Guid.Parse(config.SimpleCdn.ApiKey)));
    Log.Information("Loading modules");

    var modules = Startup.LoadModules(services);
    if (!modules.Any())
        Log.Information("No modules found");
    else
        Log.Information("Loaded {0} modules", modules.Length);

    foreach (var module in modules)
        try
        {
            var resource = Resource.Load(module.GetType().Assembly, "Resources/required.sql");
            var sql = resource.ReadString();
            if (string.IsNullOrEmpty(sql)) continue;
            await dbProvider.ExecuteAsync(sql);
        }
        catch (FileNotFoundException)
        {
            // ignored
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load required.sql");
        }

    var fridayResource = Resource.Load(typeof(Program).Assembly, "Resources/required.sql");
    var fridaySql = fridayResource.ReadString();
    await dbProvider.ExecuteAsync(fridaySql);

    var serviceProvider = services.BuildServiceProvider();

    Log.Information("Discord client created successfully");
    if (config.Lavalink.Enabled)
    {
        await client.UseLavalinkAsync();
        Log.Information("Lavalink client created successfully");
    }


    await client.UseInteractivityAsync();
    var commandsNextExtensions = await client.UseCommandsNextAsync(new CommandsNextConfiguration
    {
        CaseSensitive = false,
        EnableDefaultHelp = false,
        EnableDms = true,
        IgnoreExtraArguments = false,
        EnableMentionPrefix = true,
        Services = serviceProvider,
        PrefixResolver = prefixResolver.ResolvePrefixAsync
    });

    foreach (var module in modules) commandsNextExtensions.RegisterCommands(module.GetType().Assembly);

    Log.Information("Commands next created successfully");

    if (!Directory.Exists("issues"))
    {
        Directory.CreateDirectory("issues");
    }
    
    foreach (var cnext in commandsNextExtensions.Values)
        cnext.CommandErrored += async (e, error) =>
        {
            if (error.Exception is CommandNotFoundException || error.Exception is OperationCanceledException) return;
            if (error.Exception is BadRequestException badRequestException)
            {
                #if DEBUG
                    await error.Context.Channel.SendMessageAsync(badRequestException.JsonMessage);
                    return;
                #endif 
                
                File.WriteAllText($"issues/{Guid.NewGuid()}", badRequestException.JsonMessage + Environment.NewLine + error.Exception + Environment.NewLine + error.Exception.StackTrace);
                
            }

            if (error.Exception is ChecksFailedException checksFailedException)
            {
                foreach (var failedCheck in checksFailedException.FailedChecks)
                foreach (var moduleBase in modules)
                    if (moduleBase.GetType().Assembly == failedCheck.GetType().Assembly)
                    {
                        await moduleBase.HandleFailedChecks(failedCheck.GetType(), e, error);
                        break;
                    }
            }
            else
            {
                Log.Error(error.Exception, "Command error");
                await error.Context.RespondAsync("An error occured while executing this command :(\n" +
                                                 $"```{error.Exception.Message}```");
                
                File.WriteAllText($"issues/{Guid.NewGuid()}", error.Exception + Environment.NewLine + error.Exception.StackTrace);
            }
        };


    var slashCommands = await client.UseSlashCommandsAsync();

    foreach (var ex in slashCommands.Values)
    {
        foreach (var moduleBase in modules)
        {
            moduleBase.RegisterSlashCommands(ex);
        }
    }
    
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
                var node = await lavalinkExtension.Value.ConnectAsync(new LavalinkConfiguration
                {
                    SocketAutoReconnect = true,
                    SocketEndpoint = new ConnectionEndpoint
                    {
                        Hostname = config.Lavalink.Host,
                        Port = config.Lavalink.Port
                    },
                    RestEndpoint = new ConnectionEndpoint
                    {
                        Hostname = config.Lavalink.Host,
                        Port = config.Lavalink.Port
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
            Log.Error(e, "Lavalink connection failed");
            await client.StopAsync();
            return;
        }
    }

    bool modulesLoaded = false;
    client.Ready += async (_, _) =>
    {
        if (!modulesLoaded)
        {
            foreach (var moduleBase in modules)
            {
                await moduleBase.OnLoad();
            }
            
            modulesLoaded = true;
        }
    };
    
    
    await Task.Delay(-1);
}
catch (Exception e)
{
    Log.Error(e, "Fatal error");
}