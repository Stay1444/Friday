using System.Data;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;

namespace Friday.Modules.Misc.Commands;

public partial class Commands
{
    [Command("calc")]
    [Description("Calculates a math expression")]
    public async Task cmd_Calc(CommandContext ctx, [RemainingText] string expression)
    {
        try
        {
            await ctx.RespondAsync(new DataTable().Compute(expression, null).ToString() ?? "Error");
        }
        catch (Exception e)
        {
            await ctx.RespondAsync(e.Message);
        }
    }
}