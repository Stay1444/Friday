using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.MiniGames.Games;
using Friday.Modules.MiniGames.Images;
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
        uiBuilder.OnRender(x =>
        { 
            x.OnCancelledAsync(async (_, message) =>
            {
                await message.DeleteAsync();
            });
            
            x.Embed.Title = "2048";
            x.Embed.AddField("Leaderboard", "Coming Soon");
            x.Embed.Transparent();

            x.AddButton(play =>
            {
                play.OnClick(() =>
                {
                    x.SubPage = "game";
                    var game = x.GetState("game", new _2048Game());
                    game.Value.Start();
                });
                play.Label = "Play";
                play.Emoji = DiscordEmoji.FromName(ctx.Client, ":game_die:");
            });
                        
            x.AddSubPage("win",  winPage =>
            {
                winPage.Embed.Title = "You Win!";
                winPage.Embed.Description = "You won 2048!";
            });

            x.AddSubPage("lose", losePage =>
            {
                losePage.Embed.Title = "You Lose!";
                losePage.Embed.Description = "You have lost the game!";
                losePage.Embed.Transparent();

            });
            
            x.AddSubPageAsync("game", async gamePage =>
            {
                var game = x.GetState("game", new _2048Game());
                var imgUrl = x.GetState<string?>("imgUrl", null);
                var thinking = x.GetState("thinking", false);
                if (imgUrl.Value is null)
                {
                    var r = await _module.SimpleCdnClient.UploadAsync("2048.png", await _2080Renderer.Render(game.Value), false, DateTime.UtcNow + TimeSpan.FromDays(1));
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
                            
                            return;
                        }

                        if (game.Value.IsGameOver())
                        {
                            x.SubPage = "lose";
                            
                            return;
                        }

                        

                        var render = await _2080Renderer.Render(game.Value);
                        var r = await _module.SimpleCdnClient.UploadAsync("2048.png", render, false, DateTime.UtcNow + TimeSpan.FromDays(1));
                        imgUrl.Value = new Uri(new Uri(_module.SimpleCdnClient.Host), r.ToString()).ToString();
                        
                        
                    }catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
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