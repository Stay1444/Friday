using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Attributes;
using Friday.Common.Entities;

namespace Friday.Modules.CustomCommands.Commands;

public class Commands : FridayCommandModule
{
    [Command("customcommands"), RequireGuild]
    [RequireGuild]
    [FridayRequirePermission(Permissions.Administrator)]
    public async Task CustomCommandsCommand(CommandContext ctx)
    {
        
    }
}