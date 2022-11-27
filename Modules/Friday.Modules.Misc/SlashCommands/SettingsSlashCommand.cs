using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Friday.Modules.Misc.UI;
using Friday.UI;

namespace Friday.Modules.Misc.SlashCommands;

public partial class SlashCommands
{
    [SlashCommand("settings", "Manage Guild & User settings")]
    public async Task scmd_Settings(InteractionContext ctx)
    {
        await ctx.SendUIAsync(SettingsUI.Get(
            ctx.User,
            ctx.Member,
            _userConfigurationProvider,
            _guildConfigurationProvider,
            ctx.Client
        ));
    }
}