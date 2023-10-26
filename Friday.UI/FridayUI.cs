using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Friday.UI.Entities;

namespace Friday.UI;

public static class FridayUI
{
    public static Task SendUIAsync(this CommandContext ctx, FridayUIBuilder builder)
    {
        return SendUIAsync(ctx.Client, ctx.Channel, builder, ctx.User);
    }

    public static Task SendUIAsync(this InteractionContext ctx, FridayUIBuilder builder)
    {
        return SendUIAsync(ctx.Client, ctx.Channel, builder, ctx.User);
    }

    public static async Task SendUIAsync(DiscordClient client, DiscordChannel channel, FridayUIBuilder builder, DiscordUser user)
    {
        var renderer = new FridayUIRenderer(builder);
        await renderer.RenderAsync(client, channel, user);
    }

    public static async Task SendUIAsync(this DiscordChannel channel, DiscordClient client, FridayUIBuilder builder)
    {
        var renderer = new FridayUIRenderer(builder);
        await renderer.RenderAsync(client, channel, null);
    }
}