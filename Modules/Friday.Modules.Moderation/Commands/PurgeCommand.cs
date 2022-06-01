using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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

        var messagesList = messages.ToList();
        // REMOVE ALL MESSAGES OLDER THAN 2 WEEKS FROM THE LIST

        var olderThanTwoWeeks = messagesList.Count(x => x.CreationTimestamp < DateTimeOffset.Now.AddDays(-14));
        
        messagesList.RemoveAll(x => x.Timestamp < DateTimeOffset.Now.AddDays(-14));
                
        await ctx.Channel.DeleteMessagesAsync(messagesList);
        
        if (olderThanTwoWeeks == 0)
        {
            await ctx.RespondAsync("Done.");
        }else
        {
            await ctx.RespondAsync($"Done. {olderThanTwoWeeks} messages older than 2 weeks were not removed.");
        }
    }
}