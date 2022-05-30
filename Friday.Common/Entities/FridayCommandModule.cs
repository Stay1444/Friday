using DSharpPlus.CommandsNext;

namespace Friday.Common.Entities;

public class FridayCommandModule : BaseCommandModule
{
    public override Task BeforeExecutionAsync(CommandContext ctx)
    {
        return Task.CompletedTask;
    }
}