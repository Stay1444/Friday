
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Zaroz.Modules.ZarozMinecraft;

public sealed class awdwada : BaseCommandModule
{
    [Command("aaa")]
    public async Task cmd_aaa(CommandContext ctx)
    {
        await ctx.RespondAsync("HELLO WORLD");
    }
}