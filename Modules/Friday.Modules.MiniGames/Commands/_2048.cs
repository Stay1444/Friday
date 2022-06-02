using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.MiniGames.Games;
using Friday.Modules.MiniGames.Images;
using Friday.Modules.MiniGames.Services;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.MiniGames.Commands;

public partial class Commands
{
    [Command("2048")]
    [Description("Play 2048")]
    public async Task TwentyFortyEight(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder();
        uiBuilder.OnRenderAsync(async x =>
        { 
            var leaderBoardOrderType = x.GetState("leaderBoardOrderType", DatabaseService._2048LeaderBoardOrderBy.TotalScore);
            var startPlayDate = x.GetState("startPlayDate", DateTime.MinValue);
            x.OnCancelledAsync(async (_, message) =>
            {
                await message.DeleteAsync();
            });
            
            x.Embed.Title = "2048";
            var leaderBoard =
                await _module.DatabaseService.Get2048Leaderboard(leaderBoardOrderType.Value, 10);
            
            var leaderBoardString = "";
            
            for (var i = 0; i < leaderBoard.Count; i++)
            {
                switch (leaderBoardOrderType.Value)
                {
                    case DatabaseService._2048LeaderBoardOrderBy.PlayTime:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].playTime.ToHumanTimeSpan().Humanize(2)}\n";
                        break;
                    case DatabaseService._2048LeaderBoardOrderBy.MaxScore:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].maxScore}\n";
                        break;
                    case DatabaseService._2048LeaderBoardOrderBy.TotalScore:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].totalScore}\n";
                        break;
                    case DatabaseService._2048LeaderBoardOrderBy.Played:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].played}\n";
                        break;
                }
            }

            var playerPosition =
                await _module.DatabaseService.Get2048LeaderboardPosition(leaderBoardOrderType.Value, ctx.User.Id);

            if (playerPosition != -1 && playerPosition > 10)
            {
                var playerStats = await _module.DatabaseService.Get2048Stats(ctx.User.Id);
                
                switch (leaderBoardOrderType.Value)
                {
                    case DatabaseService._2048LeaderBoardOrderBy.PlayTime:
                        leaderBoardString += $"\n{playerPosition + 1}. {ctx.User.Username} - {playerStats.playTime.ToHumanTimeSpan().Humanize(2)}";
                        break;
                    case DatabaseService._2048LeaderBoardOrderBy.MaxScore:
                        leaderBoardString += $"\n{playerPosition + 1}. {ctx.User.Username} - {playerStats.maxScore}";
                        break;
                    case DatabaseService._2048LeaderBoardOrderBy.TotalScore:
                        leaderBoardString += $"\n{playerPosition + 1}. {ctx.User.Username} - {playerStats.totalScore}";
                        break;
                    case DatabaseService._2048LeaderBoardOrderBy.Played:
                        leaderBoardString += $"\n{playerPosition + 1}. {ctx.User.Username} - {playerStats.played}";
                        break;
                }
            }
            
            x.Embed.AddField("Leaderboard", leaderBoardString == "" ? "No one has played yet!" : leaderBoardString);
            
            x.Embed.Transparent();
            x.AddSelect(ld =>
            {
                ld.Placeholder = "Leaderboard Order";

                ld.AddOption(option =>
                {
                    option.Description = "Order by Total Play Time";
                    option.Label = "Play Time";
                    option.Value = "playTime";
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService._2048LeaderBoardOrderBy.PlayTime;
                });
                
                ld.AddOption(option =>
                {
                    option.Description = "Order by Highest Score";
                    option.Label = "Max Score";
                    option.Value = "maxScore";
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService._2048LeaderBoardOrderBy.MaxScore;
                });
                
                ld.AddOption(option =>
                {
                    option.Description = "Order by Total Score";
                    option.Label = "Total Score";
                    option.Value = "totalScore";
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService._2048LeaderBoardOrderBy.TotalScore;
                });
                
                ld.AddOption(option =>
                {
                    option.Description = "Order by the matches played";
                    option.Label = "Played";
                    option.Value = "played";
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService._2048LeaderBoardOrderBy.Played;
                });
                
                ld.OnSelect(selections =>
                {
                    var selection = selections.FirstOrDefault();
                    
                    if (selection == null)
                        return;

                    switch (selection)
                    {
                        case "playTime":
                            leaderBoardOrderType.Value = DatabaseService._2048LeaderBoardOrderBy.PlayTime;
                            break;
                        case "maxScore":
                            leaderBoardOrderType.Value = DatabaseService._2048LeaderBoardOrderBy.MaxScore;
                            break;
                        case "totalScore":
                            leaderBoardOrderType.Value = DatabaseService._2048LeaderBoardOrderBy.TotalScore;
                            break;
                        case "played":
                            leaderBoardOrderType.Value = DatabaseService._2048LeaderBoardOrderBy.Played;
                            break;
                    }
                });
            });
            x.NewLine();

            x.AddButton(play =>
            {
                play.OnClick(() =>
                {
                    x.SubPage = "game";
                    var game = x.GetState("game", new _2048Game());
                    startPlayDate.Value = DateTime.UtcNow;
                    game.Value.Start();
                });
                play.Label = "Play";
                play.Emoji = DiscordEmoji.FromName(ctx.Client, ":game_die:");
            });
                        
            x.AddSubPage("win",  winPage =>
            {
                winPage.Embed.Title = "You Win!";
                winPage.Embed.Description = "You won 2048!";
                
                x.OnCancelled((_, _) => {});
                x.OnCancelledAsync(async (_, _) => {});
                
                x.Stop();
            });

            x.AddSubPage("lose", losePage =>
            {
                losePage.Embed.Title = "You Lose!";
                losePage.Embed.Description = "You have lost the game!";
                losePage.Embed.Transparent();
                
                x.OnCancelled((_, _) => {});
                x.OnCancelledAsync(async (_, _) => {});
                
                x.Stop();
            });
            
            x.AddSubPageAsync("game", async gamePage =>
            {
                var game = x.GetState("game", new _2048Game());
                var imgUrl = x.GetState<string?>("imgUrl", null);
                var thinking = x.GetState("thinking", false);
                if (imgUrl.Value is null)
                {
                    var r = await _module.SimpleCdnClient.UploadAsync("2048.png", await _2080Renderer.Render(game.Value), false, DateTime.UtcNow + TimeSpan.FromHours(1));
                    imgUrl.Value = new Uri(new Uri(_module.SimpleCdnClient.Host), r.ToString()).ToString();
                }

                async Task Move(_2048Game.Direction direction)
                {
                    try
                    {
                        game.Value.Move(direction);
                        
                        if (game.Value.HasWon())
                        {
                            x.SubPage = "win";
                            
                            var currentStats = await _module.DatabaseService.Get2048Stats(ctx.User.Id);
                            
                            var maxScore = currentStats.maxScore > game.Value.Score ? currentStats.maxScore : game.Value.Score;

                            var newTotalPlaytime = currentStats.playTime + (DateTime.UtcNow - startPlayDate.Value);
                            
                            await _module.DatabaseService.Set2048Stats(ctx.User.Id, maxScore,
                                currentStats.totalScore + game.Value.Score, currentStats.played + 1, newTotalPlaytime,
                                ctx.User.GetName());
                            
                            return;
                        }

                        if (game.Value.IsGameOver())
                        {
                            x.SubPage = "lose";
                            
                            var currentStats = await _module.DatabaseService.Get2048Stats(ctx.User.Id);
                            
                            var maxScore = currentStats.maxScore > game.Value.Score ? currentStats.maxScore : game.Value.Score;

                            var newTotalPlaytime = currentStats.playTime + (DateTime.UtcNow - startPlayDate.Value);
                            
                            await _module.DatabaseService.Set2048Stats(ctx.User.Id, maxScore,
                                currentStats.totalScore + game.Value.Score, currentStats.played + 1, newTotalPlaytime,
                                ctx.User.GetName());
                            
                            return;
                        }

                        var render = await _2080Renderer.Render(game.Value);
                        var r = await _module.SimpleCdnClient.UploadAsync("2048.png", render, false, DateTime.UtcNow + TimeSpan.FromHours(1));
                        imgUrl.Value = new Uri(new Uri(_module.SimpleCdnClient.Host), r.ToString()).ToString();
                        
                        
                    }catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                
                
                x.OnCancelledAsync(async (_, _) =>
                {
                    var currentStats = await _module.DatabaseService.Get2048Stats(ctx.User.Id);
                            
                    var maxScore = currentStats.maxScore > game.Value.Score ? currentStats.maxScore : game.Value.Score;

                    var newTotalPlaytime = currentStats.playTime + (DateTime.UtcNow - startPlayDate.Value);
                            
                    await _module.DatabaseService.Set2048Stats(ctx.User.Id, maxScore,
                        currentStats.totalScore + game.Value.Score, currentStats.played + 1, newTotalPlaytime,
                        ctx.User.GetName());
                });
                
                gamePage.Embed.Transparent();
                gamePage.Embed.Title = "2048";
                gamePage.Embed.ImageUrl = imgUrl.Value;
                gamePage.Embed.WithFooter("Score: " + game.Value.Score.ToString());
                gamePage.AddButton(left =>
                {
                    left.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");
                    left.Disabled = thinking.Value;
                    left.OnClick(async () =>
                    {
                        await Move(_2048Game.Direction.Left);
                    });
                });
                
                gamePage.AddButton(right =>
                {
                    right.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
                    right.Disabled = thinking.Value;
                    right.OnClick(async () =>
                    {
                        await Move(_2048Game.Direction.Right);
                    });
                });
                
                gamePage.AddButton(up =>
                {
                    up.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_up:");
                    up.Disabled = thinking.Value;
                    up.OnClick(async  () =>
                    {
                        await Move(_2048Game.Direction.Up);
                    });
                });
                
                gamePage.AddButton(down =>
                {
                    down.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_down:");
                    down.Disabled = thinking.Value;
                    down.OnClick(async () =>
                    {
                        await Move(_2048Game.Direction.Down);
                    });
                });

                
            });

        });
        uiBuilder.Duration = TimeSpan.FromMinutes(5);
        await ctx.SendUIAsync(uiBuilder);
    }
}