using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.MiniGames.Games;
using Friday.Modules.MiniGames.Services;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.MiniGames.Commands;

public partial class Commands
{
    
    [Command("hangman")]
    [Description("Hangman game")]
    public async Task cmd_HangmanCommand(CommandContext ctx)
    {
        string userLang;

        if (ctx.Member is not null)
        {
            userLang = await _languageProvider.GetDesiredLanguage(ctx.Member);
        }
        else
        {
            userLang = await _languageProvider.GetDesiredLanguage(ctx.User);
        }

        if (!_hangmanWordList.DoesLanguageExist(userLang))
        {
            userLang = "en";
        }
        
        var word = await _hangmanWordList.GetRandomWord(userLang);
        word = word.RemoveDiacritics();
        
        var uiBuilder = new FridayUIBuilder();
        uiBuilder.OnRenderAsync(async x =>
        { 
            var leaderBoardOrderType = x.GetState("leaderBoardOrderType", DatabaseService.HangmanLeaderBoardOrderBy.Wins);
            var startPlayDate = x.GetState("startPlayDate", DateTime.MinValue);
            x.OnCancelledAsync(async (_, message) =>
            {
                await message.DeleteAsync();
            });
            
            x.Embed.Title = "Hangman";
            var leaderBoard =
                await _module.DatabaseService.GetHangmanLeaderboard(leaderBoardOrderType.Value, 10);
            
            var leaderBoardString = "";
            
            for (var i = 0; i < leaderBoard.Count; i++)
            {
                switch (leaderBoardOrderType.Value)
                {
                    case DatabaseService.HangmanLeaderBoardOrderBy.PlayTime:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].playTime.ToHumanTimeSpan().Humanize(2)}\n";
                        break;
                    case DatabaseService.HangmanLeaderBoardOrderBy.Wins:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].wonCount}\n";
                        break;
                    case DatabaseService.HangmanLeaderBoardOrderBy.Loses:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].lostCount}\n";
                        break;
                    case DatabaseService.HangmanLeaderBoardOrderBy.Played:
                        leaderBoardString += $"{i + 1}. {leaderBoard[i].username} - {leaderBoard[i].played}\n";
                        break;
                }
            }

            var playerPosition =
                await _module.DatabaseService.GetHangmanLeaderboardPosition(leaderBoardOrderType.Value, ctx.User.Id);

            if (playerPosition != -1 && playerPosition > 10)
            {
                var playerStats = await _module.DatabaseService.GetHangmanStats(ctx.User.Id);
                
                switch (leaderBoardOrderType.Value)
                {
                    case DatabaseService.HangmanLeaderBoardOrderBy.PlayTime:
                        leaderBoardString += $"\n{playerPosition + 1}. {ctx.User.Username} - {playerStats.playTime.ToHumanTimeSpan().Humanize(2)}";
                        break;
                    case DatabaseService.HangmanLeaderBoardOrderBy.Wins:
                        leaderBoardString += $"\n{playerPosition + 1}. {ctx.User.Username} - {playerStats.wonCount}";
                        break;
                    case DatabaseService.HangmanLeaderBoardOrderBy.Loses:
                        leaderBoardString += $"\n{playerPosition + 1}. {ctx.User.Username} - {playerStats.lostCount}";
                        break;
                    case DatabaseService.HangmanLeaderBoardOrderBy.Played:
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
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService.HangmanLeaderBoardOrderBy.PlayTime;
                });
                
                ld.AddOption(option =>
                {
                    option.Description = "Order by Wins";
                    option.Label = "Wins";
                    option.Value = "wins";
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService.HangmanLeaderBoardOrderBy.Wins;
                });
                
                ld.AddOption(option =>
                {
                    option.Description = "Order by Loses";
                    option.Label = "Loses";
                    option.Value = "loses";
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService.HangmanLeaderBoardOrderBy.Loses;
                });
                
                ld.AddOption(option =>
                {
                    option.Description = "Order by the matches played";
                    option.Label = "Played";
                    option.Value = "played";
                    option.IsDefault = leaderBoardOrderType.Value == DatabaseService.HangmanLeaderBoardOrderBy.Played;
                });
                
                ld.OnSelect(selections =>
                {
                    var selection = selections.FirstOrDefault();
                    
                    if (selection == null)
                        return;

                    switch (selection)
                    {
                        case "playTime":
                            leaderBoardOrderType.Value = DatabaseService.HangmanLeaderBoardOrderBy.PlayTime;
                            break;
                        case "wins":
                            leaderBoardOrderType.Value = DatabaseService.HangmanLeaderBoardOrderBy.Wins;
                            break;
                        case "loses":
                            leaderBoardOrderType.Value = DatabaseService.HangmanLeaderBoardOrderBy.Loses;
                            break;
                        case "played":
                            leaderBoardOrderType.Value = DatabaseService.HangmanLeaderBoardOrderBy.Played;
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
                    startPlayDate.Value = DateTime.UtcNow;
                });
                play.Label = "Play";
                play.Emoji = DiscordEmoji.FromName(ctx.Client, ":game_die:");
            });
                        
            x.AddSubPage("win",  winPage =>
            {
                winPage.Embed.Title = "You Win!";
                winPage.Embed.Description = "You won!\n The word was: " + word;
                
                x.OnCancelled((_, _) => {});
                x.OnCancelledAsync(async (_, _) => {});
                
                x.Stop();
            });

            x.AddSubPage("lose", losePage =>
            {
                losePage.Embed.Title = "You Lose!";
                losePage.Embed.Description = "You have lost the game! :(\n\nThe word was: " +
                                             word.ToUpper();
                losePage.Embed.Transparent();
                
                x.OnCancelled((_, _) => {});
                x.OnCancelledAsync(async (_, _) => {});
                
                x.Stop();
            });
            
            x.AddSubPageAsync("game", async gamePage =>
            {
                var game = x.GetState("game", new HangmanGame(word));
                
                x.OnCancelledAsync(async (_, msg) =>
                {
                    var currentStats = await _module.DatabaseService.GetHangmanStats(ctx.User.Id);
                            
                    var newTotalPlaytime = currentStats.playTime + (DateTime.UtcNow - startPlayDate.Value);
                            
                    await _module.DatabaseService.SetHangmanStats(ctx.User.Id, currentStats.wonCount,
                        currentStats.lostCount, currentStats.played + 1, newTotalPlaytime,
                        ctx.User.GetName());

                    try
                    {
                        await msg.DeleteAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                });
                
                gamePage.Embed.Transparent();
                gamePage.Embed.Title = "Hangman";

                gamePage.Embed.Description = $"\n```\n{new string(game.Value.ConstructedWord)}\n```";
                var guessedLetters = string.Join(' ', game.Value.LettersGuessed.OrderBy(x => x));
                if (guessedLetters == "")
                {
                    guessedLetters = "None";
                }
                gamePage.Embed.AddField("Letters Used", guessedLetters);
                gamePage.Embed.WithFooter($"Remaining Lives: {game.Value.Tries}");

                gamePage.AddModal(guess =>
                {
                    guess.Title = "Guess a letter";
                    guess.ButtonLabel = "Guess";
                    guess.AddField("guess", field =>
                    {
                        field.Title = "Letter";
                        field.Value = "";
                        field.Placeholder = "Guess a letter";
                        field.Required = true;
                        field.MinimumLength = 1;
                        field.MaximumLength = 1;
                    });
                    
                    guess.OnSubmit(async result =>
                    {
                        var letter = result["guess"];
                        
                        if (string.IsNullOrEmpty(letter)) return;
                        var charLetter = letter.ToUpper()[0];
                        if (game.Value.LettersGuessed.Contains(charLetter))
                        {
                            return;
                        }
                        
                        game.Value.Solve(charLetter);

                        if (game.Value.HasLost())
                        {
                            x.SubPage = "lose";
                            
                            var currentStats = await _module.DatabaseService.GetHangmanStats(ctx.User.Id);
                            
                            var newTotalPlaytime = currentStats.playTime + (DateTime.UtcNow - startPlayDate.Value);
                            
                            await _module.DatabaseService.SetHangmanStats(ctx.User.Id, currentStats.wonCount,
                                currentStats.lostCount + 1, currentStats.played + 1, newTotalPlaytime,
                                ctx.User.GetName());
                            
                            x.ForceRender();
                            return;
                        }

                        if (game.Value.IsWordCompleted())
                        {
                            x.SubPage = "win";
                            
                            var currentStats = await _module.DatabaseService.GetHangmanStats(ctx.User.Id);
                            
                            var newTotalPlaytime = currentStats.playTime + (DateTime.UtcNow - startPlayDate.Value);
                            
                            await _module.DatabaseService.SetHangmanStats(ctx.User.Id, currentStats.wonCount + 1,
                                currentStats.lostCount, currentStats.played + 1, newTotalPlaytime,
                                ctx.User.GetName());
                            
                            x.ForceRender();
                            
                            return;
                        }
                        
                        x.ForceRender();
                    });
                });
                
            });

        });
        uiBuilder.Duration = TimeSpan.FromMinutes(5);
        await ctx.SendUIAsync(uiBuilder);
    }
}