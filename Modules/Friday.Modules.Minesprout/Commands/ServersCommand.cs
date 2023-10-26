using System.Security.Cryptography;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Minesprout.UI;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;
using ReverseMarkdown;
using Serilog;
using Serilog.Core;

namespace Friday.Modules.Minesprout.Commands;

public partial class Commands
{
    [Command("servers")]
    public async Task cmd_ServersCommand(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder
        {
            Duration = TimeSpan.FromSeconds(60)
        };

        var serverCount = await _minesproutClient.GetServerCountAsync();

        uiBuilder.OnRender(x =>
        {
            x.OnCancelledAsync(async (_, message) => { await message.DeleteAsync(); });

            var newest = x.GetState("filter.newest", false);

            x.Embed.Title = "Minesprout - Servers";

            x.Embed.AddField("Server Count", serverCount.ToString());
            x.Embed.AddField("Newest", newest.Value.ToString());
            x.Embed.Transparent();

            x.AddButton(toggleNewest =>
            {
                toggleNewest.Label = "Newest: " + newest.Value;
                toggleNewest.Style = newest.Value ? ButtonStyle.Success : ButtonStyle.Secondary;

                toggleNewest.OnClick(() => { newest.Value = !newest.Value; });
            });

            x.AddButton(listButton =>
            {
                listButton.Label = "View";
                listButton.Style = ButtonStyle.Primary;
                listButton.Disabled = serverCount < 1;
                listButton.OnClick(() => { x.SubPage = "list"; });
            });

            x.AddSubPageAsync("list", async listPage =>
            {
                var currentIndex = x.GetState("list.index", 0);
                var servers = x.GetState("list.servers", await _minesproutClient.GetServersAsync());

                if (servers.Value is null)
                {
                    listPage.Embed.Description = "Unknown error while querying servers";
                    return;
                }

                var server = servers.Value.Servers[currentIndex.Value];

                await ServerUI.RenderAsync(server, listPage, _module, _minesproutClient);
                
                listPage.AddButton(prev =>
                {
                    prev.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");
                    prev.Style = ButtonStyle.Secondary;
                    prev.Disabled = serverCount <= 1;
                    prev.OnClick(() =>
                    {
                        currentIndex.Value = (currentIndex.Value - 1) % serverCount;
                        if (currentIndex.Value < 0) currentIndex.Value = serverCount - 1;
                    });
                });

                listPage.AddButton(currentBtn =>
                {
                    currentBtn.Disabled = true;
                    currentBtn.Label = $"{currentIndex.Value + 1} / {serverCount}";
                });

                listPage.AddButton(next =>
                {
                    next.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
                    next.Style = ButtonStyle.Secondary;
                    next.Disabled = serverCount <= 1;
                    next.OnClick(() => { currentIndex.Value = (currentIndex.Value + 1) % serverCount; });
                });

                listPage.NewLine();

                listPage.AddButton(delete =>
                {
                    delete.Style = ButtonStyle.Danger;
                    delete.Emoji = DiscordEmoji.FromName(ctx.Client, ":wastebasket:");
                    delete.OnClick(() => listPage.SubPage = "deleteConfirm");
                });

                listPage.AddButton(refresh =>
                {
                    refresh.Emoji = DiscordEmoji.FromName(ctx.Client, ":repeat:");
                    refresh.OnClick(async () =>
                    {
                        serverCount = await _minesproutClient.GetServerCountAsync();
                        servers.Value = await _minesproutClient.GetServersAsync();
                        currentIndex.Value = 0;
                        if (servers?.Value?.Servers.Count < 1 || servers?.Value is null)
                        {
                            x.SubPage = null;
                        }
                    });
                });
/*
                listPage.AddModal(edit =>
                {
                    edit.ButtonEmoji = DiscordEmoji.FromName(ctx.Client, ":pencil:");

                    edit.Title = $"{server.Name?.MaxLength(100)} - Edit";
                    
                    edit.AddField("name", name =>
                    {
                        name.Title = "Name";
                        name.Value = server.Name ?? "";
                        name.Placeholder = server.Name ?? "Name";
                        name.Style = TextInputStyle.Short;
                        name.MinimumLength = 1;
                        name.Required = true;
                    });
                    
                    edit.AddField("description", description =>
                    {
                        description.Title = "Description";
                        description.Value = server.Description ?? "";
                        description.Placeholder = server.Description ?? "Description";
                        description.Style = TextInputStyle.Paragraph;
                        description.Required = true;
                    });
                    
                    edit.OnSubmit(async result =>
                    {
                        var name = result["name"];
                        var description = result["description"];

                        if (name != server.Name && !string.IsNullOrWhiteSpace(name))
                        {
                            await _minesproutClient.SetServerNameAsync(server.Id, name);
                            server.Name = name;
                        }

                        if (description != server.Description && !string.IsNullOrWhiteSpace(description))
                        {
                            await _minesproutClient.SetServerDescriptionAsync(server.Id, description);
                            server.Description = description;
                        }
                    });
                });
                */
                listPage.AddSubPage("deleteConfirm", deleteConfirm =>
                {
                    var loadTime = x.GetState("deleteConfirm-time", 5);

                    deleteConfirm.Embed.Transparent();
                    deleteConfirm.Embed.Description = "Are you sure you want to delete this server?";

                    deleteConfirm.AddButton(no =>
                    {
                        no.Label = "No";
                        no.Style = ButtonStyle.Secondary;

                        no.OnClick(() =>
                        {
                            listPage.SubPage = null;
                            loadTime.Value = 5;
                        });
                    });

                    deleteConfirm.AddButton(yes =>
                    {
                        yes.Label = loadTime.Value == 0 ? "Yes" : loadTime.Value.ToString();
                        yes.Style = ButtonStyle.Danger;
                        yes.Disabled = loadTime.Value != 0;

                        if (loadTime.Value != 0)
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(1000);
                                loadTime.Value--;
                                x.ForceRender();
                            });

                        yes.OnClick(async () =>
                        {
                            try
                            {
                                Log.Information("Deleting server {id}", server.Id);
                                await _minesproutClient.DeleteServerAsync(server.Id);
                            }
                            catch (Exception exception)
                            {
                                Log.Error("Error deleting server {exception}", exception);
                            }
                            
                            loadTime.Value = 5;
                            listPage.SubPage = null;
                            servers.Value = await _minesproutClient.GetServersAsync();
                            serverCount = await _minesproutClient.GetServerCountAsync();
                            currentIndex.Value = 0;
                            if (serverCount < 1)
                            {
                                x.SubPage = null;
                            }
                        });
                    });
                });
            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }
}