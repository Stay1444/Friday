using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Friday.UI.Entities;

namespace Friday.UI;

public static class FridayUI
{
    public static Task SendUIAsync(this CommandContext ctx, FridayUIBuilder builder)
    {
        return SendUIAsync(ctx.Client, ctx.Channel, builder, ctx.User);
    }

    public static async Task SendUIAsync(DiscordClient client, DiscordChannel channel, FridayUIBuilder builder, DiscordUser user)
    {
        var renderer = new FridayUIRenderer(builder);
        await renderer.RenderAsync(client, channel, user);
    }
}