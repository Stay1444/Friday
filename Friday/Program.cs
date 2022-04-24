using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
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
        Token = config.Discord.Token,
        LoggerFactory = new SerilogLoggerFactory().AddSerilog(Log.Logger)
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

    var langModuleAssemblies = ModuleLoader.GetValidAssemblies().ToList();
    langModuleAssemblies.Add(Assembly.GetAssembly(typeof(Program))!);
    var languageProvider = new LanguageProvider(dbProvider, userConfigurationProvider, guildConfigurationProvider, langModuleAssemblies.ToArray());
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

    foreach (var module in modules)
    {
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
        }catch(Exception e)
        {
            Log.Error(e, "Failed to load required.sql");
        }
        
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
    
    Log.Information("Commands next created successfully");
    foreach(var cnext in commandsNextExtensions.Values)
    {
        #if DEBUG
        bool isDebug = true;
        #else
        bool isDebug = false;
        #endif
        if (config.Debug || isDebug)
        {
            cnext.CommandErrored += async (e, error) =>
            {
                if (error.Exception is CommandNotFoundException) return;
                if (error.Exception is BadRequestException badRequestException)
                {
                    await error.Context.Channel.SendMessageAsync(badRequestException.JsonMessage);
                    return;
                }

                if (error.Exception is ChecksFailedException checksFailedException)
                {
                    foreach (var failedCheck in checksFailedException.FailedChecks)
                    {
                        foreach (var moduleBase in modules)
                        {
                            if (moduleBase.GetType().Assembly == failedCheck.GetType().Assembly)
                            {
                                await moduleBase.HandleFailedChecks(failedCheck.GetType(), e, error);

                                break;
                            }
                        }
                    }
                }
                
                Log.Error(error.Exception, "Command error");
                
                await error.Context.RespondAsync("An error occured while executing this command :(\n" +
                                             $"```{error.Exception}```");
            };
        }
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