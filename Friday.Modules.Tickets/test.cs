using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Modules.EmbedCreator;

namespace Friday.Modules.Tickets;

public class test : BaseCommandModule
{
    private EmbedCreatorModule _embedCreator;

    public test(EmbedCreatorModule embedCreator)
    {
        _embedCreator = embedCreator;
    }

    [Command("testembed")]
    public async Task TestEmbed(CommandContext ctx)
    {
        var result = await _embedCreator.ExecuteEmbedCreatorFor(ctx.Member, ctx.Channel);
        await ctx.RespondAsync(embed: result);
    }
}