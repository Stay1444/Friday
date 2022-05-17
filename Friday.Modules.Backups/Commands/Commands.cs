using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Modules.Backups.Entities;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.Backups.Commands;

[Group("backup"), Aliases("backups", "bkp")]
public partial class Commands : FridayCommandModule
{
    private BackupsModule _module;
    private FridayConfiguration _fridayConfiguration;

    public Commands(BackupsModule module, FridayConfiguration fridayConfiguration)
    {
        _module = module;
        _fridayConfiguration = fridayConfiguration;
    }

    [GroupCommand]
    public async Task Main(CommandContext ctx)
    {
        var backups = await _module.Database.GetBackupsAsync(ctx.User.Id);
        GuildConfig? config = null;
        var backingUp = false;
        if (ctx.Member is not null)
        {
            config = await _module.Database.GetGuildConfigAsync(ctx.Member.Guild.Id);
        }
        
        var uiBuilder = new FridayUIBuilder();
        uiBuilder.OnRender(x =>
        {
            x.Embed.Transparent();
            x.Embed.Title = "Backups";
            x.Embed.AddField("Count", backups.Count.ToString());
            x.Embed.WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl);
            
            x.AddButton(list =>
            {
                list.Label = "Your Backups";
                list.Style = ButtonStyle.Secondary;
                list.Emoji = DiscordEmoji.FromName(ctx.Client, ":scroll:");
                
                list.OnClick(() =>
                {
                    
                });
            });

            x.AddButton(@void =>
            {
                @void.Label = null;
                @void.Disabled = true;
                @void.Emoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Transparent);
            });
            
            x.AddButton(create =>
            {
                create.Label = backingUp ? "Backing Up..." : "Backup";
                create.Style = ButtonStyle.Success;
                create.Emoji = DiscordEmoji.FromName(ctx.Client, ":floppy_disk:");
                
                create.Disabled = config is null;
                
                if (config is not null && !config.AdminsCanBackup)
                {
                    create.Disabled = true;
                }
                
                if (backingUp)
                {
                    create.Disabled = true;
                }
                
                create.OnClick(() =>
                {
                    backingUp = true;
                    _ = Task.Run(async () =>
                    {
                        await _module.BackupService.CreateBackupAsync(ctx.Guild, ctx.User);
                        backingUp = false;
                        x.ForceRender();
                    });
                });
            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }

    private async Task<Action<FridayUIPage>> BuildBackupListUI(ulong owner)
    {
        var backups = await _module.Database.GetBackupsAsync(owner);

        int index = 0;
        return page =>
        {
            page.Embed.Transparent();
            page.Embed.Title = backups[index].backup.Name;
            page.Embed.WithThumbnail(backups[index].backup.Icon);
            page.Embed.ClearFields();
            page.Embed.AddField("Date", backups[index].backup.Date.ToString("yyyy/MM/dd HH:mm:ss"));
            page.Embed.WithFooter("Backup " + (index + 1) + "/" + backups.Count);

            page.AddButton(previous =>
            {
                previous.Style = ButtonStyle.Primary;
                previous.Label = "Previous";
                previous.Disabled = backups.Count == 1;
                previous.OnClick(() =>
                {
                    index--;
                    if (index < 0)
                    {
                        index = backups.Count - 1;
                    }
                });
            });
            
            page.AddButton(next =>
            {
                next.Style = ButtonStyle.Primary;
                next.Label = "Next";
                next.Disabled = backups.Count == 1;
                next.OnClick(() =>
                {
                    index++;
                    if (index >= backups.Count)
                    {
                        index = 0;
                    }
                });
            });
            
        };
    }
    
    [Command("list"), Aliases("ls")]
    [Description("Lists all your backups")]
    public async Task ListCommand(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder();
        uiBuilder.OnRender(await BuildBackupListUI(ctx.User.Id));
        await ctx.SendUIAsync(uiBuilder);
    }

    [Command("create"), Aliases("new"), RequireGuild]
    public async Task CreateCommand(CommandContext ctx)
    {
        await _module.BackupService.CreateBackupAsync(ctx.Guild, ctx.User);
        await ctx.RespondAsync("Backup created");
    }
}