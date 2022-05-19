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
public class Commands : FridayCommandModule
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
        if (ctx.Member is not null)
        {
            config = await _module.Database.GetGuildConfigAsync(ctx.Member.Guild.Id);
        }
        
        var uiBuilder = new FridayUIBuilder();
        uiBuilder.OnRender(x =>
        {
            var backingUp = x.GetState("backingUp", false);
            
            x.Embed.Transparent();
            x.Embed.Title = "Backups";
            x.Embed.AddField("Count", backups.Count.ToString() + "/" + BackupsModule.BackupsPerUser, true);
            
            if (backups.Count == 0)
            {
                x.Embed.AddField("Last Backup", "Never", true);
            }else if ((DateTime.UtcNow - backups.Last().backup.Date) < TimeSpan.FromSeconds(5))
            {
                x.Embed.AddField("Last Backup", "Just Now", true);
            }
            else
            {
                x.Embed.AddField("Last Backup", new HumanTimeSpan(DateTime.UtcNow - backups.Last().backup.Date).Humanize(2) + " ago", true);
            }
            x.Embed.WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl);
            
            x.AddSubPage("backups-list", backupsList =>
            {
                x.AddButton(list =>
                {
                    list.Label = "Your Backups";
                    list.Style = ButtonStyle.Secondary;
                    list.Emoji = DiscordEmoji.FromName(ctx.Client, ":scroll:");
                    list.Disabled = backups.Count == 0;
                    list.OnClick(() =>
                    {
                        x.SubPage = "backups-list";
                    });
                });
                
                if (backups.Count > 0)
                {
                    var index = x.GetState("index", 0);
                    var queryString = x.GetState<string?>("queryString", null);
                    var queriedBackups = queryString.Value is null ? backups : backups.Where(b => b.code == queryString.Value || b.backup.Name.ToLower().Contains(queryString.Value.ToLower())).ToList();

                    backupsList.Embed.Transparent();
                    
                    if (queriedBackups.Count > 0)
                    {
                        backupsList.Embed.Title = queriedBackups[index.Value].backup.Name;
                        backupsList.Embed.WithThumbnail(queriedBackups[index.Value].backup.Icon);
                        backupsList.Embed.WithFooter($"{queriedBackups[index.Value].code} ({index.Value + 1}/{queriedBackups.Count})");
                        backupsList.Embed.WithTimestamp(queriedBackups[index.Value].backup.Date);
                        backupsList.Embed.AddField("Channels", queriedBackups[index.Value].backup.Channels.Count.ToString(), true);
                        backupsList.Embed.AddField("Roles", queriedBackups[index.Value].backup.Roles.Count.ToString(), true);
                        backupsList.Embed.AddField("Backup Date", new HumanTimeSpan(DateTime.UtcNow - queriedBackups[index.Value].backup.Date).Humanize(2) + " ago");
                    }
                    else
                    {
                        backupsList.Embed.WithTitle("No backups found");
                        backupsList.Embed.Description = $"No backups found for \n```{queryString.Value}```";
                    }

                    backupsList.AddButton(previous =>
                    {
                        previous.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");
                        previous.Style = ButtonStyle.Secondary;
                        previous.Disabled = queriedBackups.Count < 2;
                        previous.OnClick(() =>
                        {
                            index.Value--;
                            if (index.Value < 0)
                            {
                                index.Value = queriedBackups.Count - 1;
                            }
                        });
                    });
                    
                    backupsList.AddModal(query =>
                    {
                        query.ButtonEmoji = DiscordEmoji.FromName(ctx.Client, ":mag:");
                        query.ButtonStyle = queryString.Value == null ? ButtonStyle.Secondary : ButtonStyle.Success;
                        query.Title = "Query";
                        query.ButtonDisabled = backups.Count < 2;
                        query.AddField("query", field =>
                        {
                            field.Placeholder = "Search for a backup or code";
                            field.Value = queryString.Value;
                            field.Style = TextInputStyle.Short;
                        });
                    
                        query.OnSubmit(result =>
                        {
                            queryString.Value = result["query"] == "" ? null : result["query"];
                            index.Value = 0;
                            x.ForceRender();
                        });
                    });
                    
                    backupsList.AddButton(next =>
                    {
                        next.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
                        next.Style = ButtonStyle.Secondary;
                        next.Disabled = queriedBackups.Count < 2;
                        next.OnClick(() =>
                        {
                            index.Value++;
                            if (index.Value >= queriedBackups.Count)
                                index.Value = 0;
                        });
                    });
                    
                    backupsList.NewLine();
                    
                    backupsList.AddButton(delete =>
                    {
                        delete.Emoji = DiscordEmoji.FromName(ctx.Client, ":wastebasket:");
                        delete.Style = ButtonStyle.Danger;
                        delete.Label = "Delete";
                        delete.Disabled = queriedBackups.Count == 0;
                        delete.OnClick(() =>
                        {
                            backupsList.SubPage = "deleteConfirm";
                        });
                    });
                    
                    backupsList.AddButton(empty =>
                    {
                        empty.Emoji = DiscordEmoji.FromGuildEmote(ctx.Client, _fridayConfiguration.Emojis.Transparent);
                        empty.Style = ButtonStyle.Secondary;
                        empty.Disabled = true;
                    });
                    
                    backupsList.AddButton(load =>
                    {
                        load.Emoji = DiscordEmoji.FromName(ctx.Client, ":outbox_tray:");
                        load.Style = ButtonStyle.Success;
                        load.Label = "Load";
                        load.Disabled = queriedBackups.Count == 0;
                        
                        load.OnClick(() =>
                        {
                            backupsList.SubPage = "loadConfirm";
                        });
                    });

                    backupsList.NewLine();
                    
                    backupsList.AddButton(back =>
                    {
                        back.Label = "Back";
                        back.Style = ButtonStyle.Secondary;
                        back.OnClick(() =>
                        {
                            x.SubPage = null;
                        });
                    });
                    
                    backupsList.AddSubPage("deleteConfirm", deleteConfirm =>
                    {
                        deleteConfirm.Embed.Transparent();
                        deleteConfirm.Embed.Description = "Are you sure you want to delete this backup?";
                        deleteConfirm.Embed.WithFooter("This action cannot be undone.");

                        deleteConfirm.AddButton(no =>
                        {
                            no.Label = "No";
                            no.Style = ButtonStyle.Secondary;
                        
                            no.OnClick(() =>
                            {
                                backupsList.SubPage = null;
                            });
                        });
                    
                        deleteConfirm.AddButton(yes =>
                        {
                            yes.Label = "Yes";
                            yes.Style = ButtonStyle.Danger;
                        
                            yes.OnClick(async () =>
                            {
                                await _module.Database.DeleteBackupAsync(queriedBackups[index.Value].id);
                                backups = backups.Where(b => b.id != queriedBackups[index.Value].id).ToList();
                                if (backups.Count == 0)
                                {
                                    backupsList.SubPage = null;
                                    x.SubPage = null;
                                    index.Value = 0;
                                    return;
                                }
                                index.Value = Math.Clamp(index.Value - 1, 0, queriedBackups.Count - 1);
                                backupsList.SubPage = null;
                            });
                        });
                    });
                    
                    backupsList.AddSubPage("loadConfirm", loadConfirm =>
                    {
                        var loadTime = x.GetState("loadConfirm-time", 5);
                        loadConfirm.Embed.Transparent();
                        loadConfirm.Embed.Description = "Are you sure you want to load this backup?";
                        loadConfirm.Embed.WithFooter("This will wipe the current server.");

                        loadConfirm.AddButton(no =>
                        {
                            no.Label = "No";
                            no.Style = ButtonStyle.Secondary;
                        
                            no.OnClick(() =>
                            {
                                backupsList.SubPage = null;
                                loadTime.Value = 5;
                            });
                        });
                    
                        loadConfirm.AddButton(yes =>
                        {
                            yes.Label = loadTime.Value == 0 ? "Yes" : loadTime.Value.ToString();
                            yes.Style = ButtonStyle.Danger;
                            yes.Disabled = loadTime.Value != 0;

                            if (loadTime.Value != 0 && backupsList.SubPage == "loadConfirm")
                            {
                                _ = Task.Run(async () =>
                                {
                                    await Task.Delay(1000);
                                    loadTime.Value--;
                                    Console.WriteLine(loadTime.Value);
                                    x.ForceRender();
                                });
                            }
                            
                            yes.OnClick(async () =>
                            {
                                loadTime.Value = 5;
                                backupsList.SubPage = null;
                            });
                        });
                    });
                }
            });

            
            x.AddButton(create =>
            {
                create.Label = backingUp.Value ? "Backing Up..." : "Backup";
                create.Style = ButtonStyle.Success;
                create.Emoji = DiscordEmoji.FromName(ctx.Client, ":inbox_tray:");
                
                create.Disabled = config is null;
                
                if (config is not null && !ctx.Member!.IsOwner)
                {
                    if (config.AdminsCanBackup && ctx.Member.Permissions.HasFlag(Permissions.Administrator))
                    {
                        create.Disabled = false;
                    }
                    else
                    {
                        create.Disabled = true;
                    }
                }
                
                if (backingUp.Value)
                {
                    create.Disabled = true;
                }

                if (backups.Count >= BackupsModule.BackupsPerUser)
                {
                    create.Disabled = true;
                }
                
                create.OnClick(async () =>
                {
                    backups = await _module.Database.GetBackupsAsync(ctx.User.Id);
                    if (backups.Count >= BackupsModule.BackupsPerUser)
                    {
                        return;
                    }
                    backingUp.Value = true;
                    _ = Task.Run(async () =>
                    {
                        await _module.BackupService.CreateBackupAsync(ctx.Guild, ctx.User);
                        backingUp.Value = false;
                        backups = await _module.Database.GetBackupsAsync(ctx.User.Id);
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