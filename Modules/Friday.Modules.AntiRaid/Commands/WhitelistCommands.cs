using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common.Attributes;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.AntiRaid.Commands;

public partial class Commands
{
    [Command("whitelist"), RequireGuild, FridayRequireGuildOwner]
    public async Task Whitelist(CommandContext ctx, [Description("User to whitelist")] DiscordMember member)
    {
        var db = _module.AntiRaidDatabase;
        var uiBuilder = new FridayUIBuilder().OnRenderAsync(async x =>
        {
            var isWhiteListed = await db.IsUserInWhitelist(member.Guild.Id, member.Id);
            x.Embed.Title = $"{member.Username} Whitelist";
            x.Embed.Color = isWhiteListed ? DiscordColor.SpringGreen : DiscordColor.IndianRed;
            x.Embed.Description = $"{member.Username} is {(isWhiteListed ? "" : "not")} whitelisted";

            x.AddButton( button =>
            {
                button.Label = isWhiteListed ? "Remove from whitelist" : "Add to whitelist";
                button.Style = isWhiteListed ? ButtonStyle.Danger : ButtonStyle.Success;
                
                button.OnClick(async () =>
                {
                    if (isWhiteListed)
                    {
                        await db.RemoveUserFromWhitelist(member.Guild.Id, member.Id);
                    }
                    else
                    {
                        await db.AddUserToWhitelist(member.Guild.Id, member.Id);
                    }
                });
                
            });
        });
        await ctx.SendUIAsync(uiBuilder);
    }
}