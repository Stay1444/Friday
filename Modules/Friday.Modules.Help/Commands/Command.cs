using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.Help.Commands;

public class Command : FridayCommandModule
{
    private FridayConfiguration _configuration;
    public Command(FridayConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Command("help")]
    public async Task HelpCommand(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder();
        uiBuilder.OnRender(x =>
        {
            x.Embed.WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl);
            x.Embed.WithTitle("Friday - Help");
            x.Embed.WithDescription(
                $"[Join the official Friday Discord server]({_configuration.Discord.OfficialServer})\n\n" +
                $"[Invite Friday to your server]({_configuration.Discord.BotInvite})");
            x.Embed.WithColor(new DiscordColor(_configuration.Discord.Color));

            x.AddSelect(select =>
            {
                select.Placeholder = "Select a module";
                
                select.AddOption(option =>
                {
                    option.Label = "Moderation";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":shield:");
                    option.Value = "moderation";
                });

                select.AddOption(option =>
                {
                    option.Label = "Backups";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":package:");
                    option.Value = "backups";
                });
                
                select.AddOption(option =>
                {
                    option.Label = "Music";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":musical_note:");
                    option.Value = "music";
                });

                select.AddOption(option =>
                {
                    option.Label = "Reaction Roles";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":robot:");
                    option.Value = "reactionroles";
                });

                select.AddOption(option =>
                {
                    option.Label = "Anti Raid";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
                    option.Value = "antiraid";
                });

                select.AddOption(option =>
                {
                    option.Label = "Channel Stats";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":chart_with_upwards_trend:");
                    option.Value = "channelstats";
                });
                
                select.AddOption(option =>
                {
                    option.Label = "Utilities";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":wrench:");
                    option.Value = "utilities";
                });
                
                select.AddOption(option =>
                {
                    option.Label = "Developer";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":tools:");
                    option.Value = "developer";
                });

                select.AddOption(option =>
                {
                    option.Label = "Miscellaneous";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":question:");
                    option.Value = "miscellaneous";
                });
                
                select.AddOption(option =>
                {
                    option.Label = "Games";
                    option.Emoji = DiscordEmoji.FromName(ctx.Client, ":game_die:");
                    option.Value = "games";
                });
            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }
}