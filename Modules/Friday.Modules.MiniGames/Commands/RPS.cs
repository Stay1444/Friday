using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Friday.Common;

namespace Friday.Modules.MiniGames.Commands;

public partial class Commands
{
    [Command("rps"), RequireGuild]
    public async Task cmd_RockPaperScissors(CommandContext ctx, DiscordMember member)
    {
        var msgBuilder = new DiscordMessageBuilder();
        var embedBuilder = new DiscordEmbedBuilder();
        embedBuilder.Transparent();
        var hourglass = DiscordEmoji.FromName(ctx.Client, ":hourglass:");
        var ready = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
        var rock = DiscordEmoji.FromName(ctx.Client, ":rock:");
        var paper = DiscordEmoji.FromName(ctx.Client, ":newspaper:");
        var scissors = DiscordEmoji.FromName(ctx.Client, ":scissors:");
        
        embedBuilder.AddField(ctx.User.Username, hourglass, true);
        embedBuilder.AddField(member.Username, hourglass, true);
        embedBuilder.Title = "Rock Paper Scissors";
        msgBuilder.WithEmbed(embedBuilder);
        msgBuilder.AddComponents(
                new DiscordButtonComponent(ButtonStyle.Primary, "rock", null, false, new DiscordComponentEmoji(rock)),
                new DiscordButtonComponent(ButtonStyle.Primary, "paper", null, false, new DiscordComponentEmoji(paper)),
                new DiscordButtonComponent(ButtonStyle.Primary, "scissors", null, false, new DiscordComponentEmoji(scissors))
            );
        
        var user1Choice = "wait";
        var user2Choice = "wait";
        
        var message = await ctx.RespondAsync(msgBuilder);
        
        while (true)
        {
            var result = await ctx.Client.GetInteractivity().WaitForButtonAsync(message, TimeSpan.FromMinutes(1));
            if (result.TimedOut)
            {
                await message.DeleteAsync();
                return;
            }
            
            if (result.Result.User.Id != ctx.User.Id && result.Result.User.Id != member.Id) continue;

            var choice = result.Result.Id;

            if (result.Result.User.Id == ctx.User.Id && user1Choice == "wait")
            {
                user1Choice = choice;
            }
            
            if (result.Result.User.Id == member.Id && user2Choice == "wait")
            {
                user2Choice = choice;
            }

            embedBuilder.ClearFields();
            if (user1Choice == "wait" || user2Choice == "wait")
            {
                embedBuilder.AddField(ctx.User.Username, user1Choice == "wait" ? hourglass : ready, true);
                embedBuilder.AddField(member.Username, user2Choice == "wait" ? hourglass : ready, true);
                
                msgBuilder.WithEmbed(embedBuilder);
                await result.Ack();
                await message.ModifyAsync(msgBuilder);
            }else
            {
                switch (user1Choice)
                {
                    case "rock": 
                        embedBuilder.AddField(ctx.User.Username, rock, true);
                        break;
                    case "paper":
                        embedBuilder.AddField(ctx.User.Username, paper, true);
                        break;
                    case "scissors":
                        embedBuilder.AddField(ctx.User.Username, scissors, true);
                        break;
                }
                
                switch (user2Choice)
                {
                    case "rock": 
                        embedBuilder.AddField(member.Username, rock, true);
                        break;
                    case "paper":
                        embedBuilder.AddField(member.Username, paper, true);
                        break;
                    case "scissors":
                        embedBuilder.AddField(member.Username, scissors, true);
                        break;
                }

                if (user1Choice == user2Choice)
                {
                    embedBuilder.Title = "Draw";
                }
                else if (user1Choice == "rock" && user2Choice == "scissors" || user1Choice == "paper" && user2Choice == "rock" || user1Choice == "scissors" && user2Choice == "paper")
                {
                    embedBuilder.Title = ctx.User.Username + " wins!";
                }
                else
                {
                    embedBuilder.Title = member.Username + " wins!";
                }
                msgBuilder.ClearComponents();
                msgBuilder.WithEmbed(embedBuilder);
                await result.Ack();
                await message.ModifyAsync(msgBuilder);
                
                break;
            }
        }
    }
}