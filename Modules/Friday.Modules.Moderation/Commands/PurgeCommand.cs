using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Attributes;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;

namespace Friday.Modules.Moderation.Commands;

public class PurgeCommand : FridayCommandModule
{
    private ModerationModuleBase _moderationModuleBase;
    private FridayConfiguration _config;
    private LanguageProvider _languageProvider;

    public PurgeCommand(ModerationModuleBase moderationModuleBase, FridayConfiguration config,
        LanguageProvider languageProvider)
    {
        _moderationModuleBase = moderationModuleBase;
        _config = config;
        _languageProvider = languageProvider;
    }

    [Command("purge"), Aliases("clear")]
    [RequireGuild]
    [FridayRequirePermission(Permissions.ManageMessages)]
    [RequireBotPermissions(Permissions.ManageMessages)]
    public async Task Purge(CommandContext ctx, [Description("Number of messages to delete")] int amount)
    {
        var messages = await ctx.Channel.GetMessagesAsync(amount);
        await ctx.Channel.DeleteMessagesAsync(messages);
        await ctx.RespondAsync("Done.");
    }
}