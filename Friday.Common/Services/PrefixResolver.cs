using DSharpPlus.Entities;

namespace Friday.Common.Services;

public class PrefixResolver
{
    private DatabaseProvider _databaseProvider;
    private GuildConfigurationProvider _guildConfigurationProvider;
    private UserConfigurationProvider _userConfigurationProvider;
    public PrefixResolver(DatabaseProvider databaseProvider, GuildConfigurationProvider guildConfigurationProvider, UserConfigurationProvider userConfigurationProvider)
    {
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
            int r =  message.Content.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length;
            return r;
        }
        else
        {
            var userConfig = await _userConfigurationProvider.GetConfiguration(message.Author);
            string prefix = "f";
            if (userConfig.PrefixOverride is not null)
            {
                prefix = userConfig.PrefixOverride;
            }
            // return the index of the prefix end (if it doens't exist, return -1)
            int r = message.Content.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length;
            return r;
        }
    }
}