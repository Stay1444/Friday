using DSharpPlus.Entities;
using Friday.Common.Models;

namespace Friday.Common.Services;

public class PrefixResolver
{
    private DatabaseProvider _databaseProvider;
    private GuildConfigurationProvider _guildConfigurationProvider;
    private UserConfigurationProvider _userConfigurationProvider;
    private FridayConfiguration _fridayConfiguration;
    public PrefixResolver(FridayConfiguration configuration, DatabaseProvider databaseProvider, GuildConfigurationProvider guildConfigurationProvider, UserConfigurationProvider userConfigurationProvider)
    {
        this._fridayConfiguration = configuration;
        _databaseProvider = databaseProvider;
        _guildConfigurationProvider = guildConfigurationProvider;
        _userConfigurationProvider = userConfigurationProvider;
    }

    public async Task<int> ResolvePrefixAsync(DiscordMessage message)
    {
        if (message.Channel.Guild is not null)
        {
            var userConfig = await _userConfigurationProvider.GetConfiguration(message.Author);
            var guildConfig = await _guildConfigurationProvider.GetConfiguration(message.Channel.Guild);
            string prefix = guildConfig.Prefix;
            if (userConfig.PrefixOverride is not null)
            {
                prefix = userConfig.PrefixOverride;
            }
            // return the index of the prefix end (if it doens't exist, return -1)
            
            if (message.Content.StartsWith(prefix))
            {
                return prefix.Length;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            var userConfig = await _userConfigurationProvider.GetConfiguration(message.Author);
            string prefix = _fridayConfiguration.Discord.DefaultPrefix;
            if (userConfig.PrefixOverride is not null)
            {
                prefix = userConfig.PrefixOverride;
            }
            // return the index of the prefix end (if it doens't exist, return -1)
            if (message.Content.StartsWith(prefix))
            {
                return prefix.Length;
            }
            else
            {
                return -1;
            }
        }
    }

    public async Task<string> GetPrefixAsync(DiscordMember user)
    {
        var guildSettings = await _guildConfigurationProvider.GetConfiguration(user.Guild);
        var userSettings = await _userConfigurationProvider.GetConfiguration(user);
        var prefix = guildSettings.Prefix;
        if (userSettings.PrefixOverride is not null)
        {
            prefix = userSettings.PrefixOverride;
        }
        
        return prefix;
    }

    public async Task<string> GetPrefixAsync(DiscordUser user)
    {
        var userSettings = await _userConfigurationProvider.GetConfiguration(user);
        var prefix = "f";
        if (userSettings.PrefixOverride is not null)
        {
            prefix = userSettings.PrefixOverride;
        }
        return prefix;
    }
}