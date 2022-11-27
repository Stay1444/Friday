using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.Misc.UI;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.Misc.Commands;

public partial class Commands
{
    [Command("settings"), Aliases("config", "cfg")]
    public async Task cmd_SettingsCommand(CommandContext ctx)
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