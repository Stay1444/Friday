using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Attributes;

namespace Friday.Modules.Misc.Commands;

public partial class Commands
{
    [Command("addemotes"), Aliases("addemote"), RequireGuild, FridayRequirePermission(Permissions.Administrator),
     Cooldown(5, 10, CooldownBucketType.Guild), Priority(5)]
    public async Task AddEmotesCommand(CommandContext ctx, DiscordEmoji emoji, string? name = null)
    {
        var success = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
        var failure = DiscordEmoji.FromName(ctx.Client, ":x:");

        try
        {
            await ctx.Guild.CreateEmojiAsync(name ?? emoji.Name, await emoji.DownloadAsync());
            await ctx.Message.CreateReactionAsync(success);
        }catch
        {
            await ctx.Message.CreateReactionAsync(failure);
        }
    }

    [Command("addemotes"), RequireGuild, FridayRequirePermission(Permissions.Administrator),
     Cooldown(5, 10, CooldownBucketType.Guild), Priority(1)]
    public async Task AddMultipleEmotesCommand(CommandContext ctx, params DiscordEmoji[] emojis)
    {
        var success = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
        var failure = DiscordEmoji.FromName(ctx.Client, ":x:");
        if (emojis.Length > 0)
        {
            try
            {
                foreach (var emoji in emojis)
                {
                    await ctx.Guild.CreateEmojiAsync(emoji.Name, await emoji.DownloadAsync());
                }
            
                await ctx.Message.CreateReactionAsync(success);
            }catch
            {
                await ctx.Message.CreateReactionAsync(failure);
            }

            return;
        }

        if (ctx.Message.Attachments.Count > 0)
        {
            var files = ctx.Message.Attachments;
            
            var imageMediaTypes = new[]
            {
                "image/png",
                "image/jpeg",
                "image/jpg",
                "image/gif",
                "image/webp"
            };

            var allowedCharactersInName = new char[Constants.AlphaNumeric.Length + 1];
            allowedCharactersInName[0] = '_';
            Array.Copy(Constants.AlphaNumeric, 0, allowedCharactersInName, 1, Constants.AlphaNumeric.Length);
            
            try
            {
                foreach (var file in files)
                {
                    if (!imageMediaTypes.Contains(file.MediaType))
                    {
                        continue;
                    }
                    
                    var name = file.FileName;
                    
                    name = name.Split('.')[0];
                    
                    if (name.Length > 20)
                    {
                        name = name.Substring(0, 20);
                    }
                    
                    name = name.Replace(" ", "_");
                    
                    for (var i = 0; i < name.Length; i++)
                    {
                        if (!allowedCharactersInName.Contains(name[i]))
                        {
                            name = name.Replace(name[i], '_');
                        }
                    }
            
                    await ctx.Guild.CreateEmojiAsync(name, await file.DownloadAsync());
                }
            
                await ctx.Message.CreateReactionAsync(success);
            }catch(Exception error)
            {
                await ctx.Message.CreateReactionAsync(failure);
                await ctx.RespondAsync(error.Message);
            }
            
            return;
        }
        
        await ctx.Message.CreateReactionAsync(failure);
    }
}