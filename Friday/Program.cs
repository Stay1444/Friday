using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Friday.Common;
using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Helpers;
using Friday.Modules.Backups;
using Friday.Modules.Birthday;
using Friday.Modules.Help;
using Friday.Modules.Misc;
using Friday.Modules.Music;
using Friday.Modules.ReactionRoles;
using Friday.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using SimpleCDN.Wrapper;
using Zaroz.Modules.ZarozMinecraft;

try
{
    _ = Constants.ProcessStartTimeUtc.AddMilliseconds(0);
    Startup.CleanTempFiles();

    Startup.LoggerStartup();

    Log.Information("Starting up");

    Log.Information("Reading config...");

    if (!Startup.DoesConfigurationExist())
    {
        Startup.LoadConfiguration();
        
        Log.Information("Configuration file created. Please edit it and restart the container.");
        
        await Task.Delay(-1);
    }
    
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

    var moduleManager = new ModuleManager();
    
    // Load modules | Module.dll
    moduleManager.LoadModules();

    
    
    // Load this project as a module
    // In the future we could add commands here, but this is currently used because this contains basic language files.
    moduleManager.LoadModule<Program>();
    
    // Load Friday modules
    moduleManager.LoadModule<BackupsModule>();
    moduleManager.LoadModule<MiscModule>();
    moduleManager.LoadModule<BirthdayModule>();
    services.AddSingleton<IModuleManager>(moduleManager);
    
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
    
    services.AddSingleton(new LanguageProvider(userConfigProvider, guildConfigProvider,
        moduleManager));
    
    services.AddSingleton(new SimpleCdnClient(config.SimpleCdn.Host, Guid.Parse(config.SimpleCdn.ApiKey)));

    Log.Information("Loading modules");

    moduleManager.CreateInstances(services);
    
    if (!moduleManager.Modules.Any())
        Log.Information("No modules found");
    else
        Log.Information("Loaded {0} modules", moduleManager.Modules.Count);

    foreach (var module in moduleManager.Modules)
        try
        {
            var resource = Resource.Load(module.Assembly, "Resources/required.sql");
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

    foreach (var module in moduleManager.Modules) commandsNextExtensions.RegisterCommands(module.Assembly);

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
            }

            if (error.Exception is ChecksFailedException checksFailedException)
            {
                foreach (var failedCheck in checksFailedException.FailedChecks)
                foreach (var module in moduleManager.Modules)
                    if (module.Assembly == failedCheck.GetType().Assembly)
                    {
                        await module.Instance!.HandleFailedChecks(failedCheck.GetType(), e, error);
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


    var slashCommands = await client.UseSlashCommandsAsync(new SlashCommandsConfiguration()
    {
        Services = serviceProvider
    });

    foreach (var ex in slashCommands.Values)
    {
        foreach (var module in moduleManager.Modules)
        {
            module.Instance!.RegisterSlashCommands(ex);
        }
        
        ex.SlashCommandErrored += async (_, e) =>
        {
            // check if the exception is a checks failed exception (i.e. a permission check failed)
            if (e.Exception is SlashExecutionChecksFailedException checksFailedException)
            {
                var embed = new DiscordEmbedBuilder();
                embed.WithTitle("Execution checks failed");
                embed.WithColor(DiscordColor.Red);
            
                // iterate over all failed checks
                foreach (var failedCheck in checksFailedException.FailedChecks)
                {
                    if (failedCheck is SlashRequirePermissionsAttribute)
                    {
                        embed.Description += ("Not enough permissions.");
                    }
                    
                    if (failedCheck is SlashRequireBotPermissionsAttribute requireBotPermissionsAttribute)
                    {
                        embed.Description +=  ("Bot does not have enough permissions. Required: " + requireBotPermissionsAttribute.Permissions.ToPermissionString());
                    }
                    
                    if (failedCheck is SlashRequireUserPermissionsAttribute requireUserPermissionsAttribute)
                    {
                        embed.Description += ("You do not have enough permissions. Required: \n```" + requireUserPermissionsAttribute.Permissions.ToPermissionString() + "```");
                    }
                    
                    if (failedCheck is SlashRequireOwnerAttribute)
                    {
                        embed.Description += ("Only the bot owner can use this command.");
                    }
                    
                    if (failedCheck is SlashRequireGuildAttribute)
                    {
                        embed.Description += ("This command can only be used in a guild.");
                    }
                    
                    if (failedCheck is SlashRequireDirectMessageAttribute)
                    {
                        embed.Description += ("This command can only be used in a DM channel.");
                    }

                    if (string.IsNullOrEmpty(embed.Description))
                    {
                        embed.Description = "You cannot execute that command.";
                    }
                }
                
                await e.Context.CreateResponseAsync(embed: embed, true);
            }
            else
            {
                #if DEBUG
                
                DiscordEmbedBuilder exceptionEmbed = new DiscordEmbedBuilder();
                exceptionEmbed.WithTitle("Exception");
                exceptionEmbed.WithColor(DiscordColor.Red);
                exceptionEmbed.AddField("Message", e.Exception.Message);
                await e.Context.Channel.SendMessageAsync(exceptionEmbed);
                
                #endif
                Console.WriteLine(e.Exception);
            }
            
            
        };
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
            foreach (var module in moduleManager.Modules)
            {
                await module.Instance!.OnLoad();
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