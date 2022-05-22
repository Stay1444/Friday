using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;

namespace Friday.Modules.Misc.Commands;

public partial class Commands 
{
    
    [Command("serverinfo"), RequireGuild]
    public async Task ServerInfoCommand(CommandContext ctx)
    {
        
        var embedBuilder = new DiscordEmbedBuilder();
        embedBuilder.WithTitle($"{ctx.Guild.Name} Info");
        embedBuilder.WithThumbnail(ctx.Guild.IconUrl);
        embedBuilder.AddField("ID", "`"+ ctx.Guild.Id + "`");
        
        
        embedBuilder.AddField("Created At", ctx.Guild.CreationTimestamp.DateTime.ToLongDateString() + ", " + ctx.Guild.CreationTimestamp.DateTime.ToShortTimeString(), true);
        embedBuilder.AddField("Partnered", (ctx.Guild.BannerUrl != null).ToString(), true);
        embedBuilder.AddField("Boosts", ctx.Guild.PremiumSubscriptionCount.ToString(), true);
        
        
        embedBuilder.AddField("Owner", $"{ctx.Guild.Owner.Mention}, {ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}", true);
        embedBuilder.AddField("Members", ctx.Guild.Members.Count.ToString(), true);
        embedBuilder.AddField("Role Count", ctx.Guild.Roles.Count.ToString(), true);

        if (await _fridayVerifiedServer.IsVerified(ctx.Guild.Id))
        {
            embedBuilder.WithColor(DiscordColor.Azure);
            var moderatorEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Verified);
            embedBuilder.WithFooter($"{ctx.Client.CurrentUser.Username} Verified Server", moderatorEmoji.Url);
            
            
            
        }
        else
        {
            embedBuilder.Transparent();
        }
        
        await ctx.RespondAsync(embedBuilder.Build());
        
    }
}