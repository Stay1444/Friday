using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.Help.Commands;

public class Command : FridayCommandModule
{
    private FridayConfiguration _configuration;
    private IModuleManager _moduleManager;
    private HelpModule _module;
    public Command(FridayConfiguration configuration, IModuleManager moduleManager, HelpModule module)
    {
        _configuration = configuration;
        _moduleManager = moduleManager;
        _module = module;
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
                $"Select a module below to view its commands!\n\n" +
                $"You can also use `{ctx.Prefix}help <command>`");
            x.Embed.WithColor(new DiscordColor(_configuration.Discord.Color));

            x.AddSelect(select =>
            {
                select.Placeholder = "Select a module";

                foreach (var module in _moduleManager.Modules)
                {
                    var commands = _module.Scanner.ResolveCommandsAsync(module, ctx);
                    if (commands.Count < 1) continue;
                    select.AddOption(option =>
                    {
                        option.Label = module.Name;
                        option.Value = module.Assembly.GetName().Name;
                        option.Description = $"{commands.Count} commands";
                        if (module.Icon is not null)
                        {
                            var emoji = DiscordEmojiUtils.FromGeneric(module.Icon, ctx.Client);
                            if (emoji is not null)
                            {
                                option.Emoji = emoji;
                            }
                        }
                    });
                }
            });

            x.NewLine();
            x.AddButtonUrl(btn =>
            {
                btn.Label = "Official Server";
                btn.Url = _configuration.Discord.OfficialServer;
            });
            x.AddButtonUrl(btn =>
            {
                btn.Label = "Invite";
                btn.Url = _configuration.Discord.BotInvite;
            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }
}