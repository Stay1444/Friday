using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Minesprout.UI;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;
using Serilog;

namespace Friday.Modules.Minesprout.Commands;

public partial class Commands
{
    [Command("server")]
    public async Task cmd_ServerCommand(CommandContext ctx, ulong? serverId = null)
    {
        if (serverId is null)
        {
            await ctx.RespondAsync($"Usage: {ctx.Prefix}server <id>");
            return;
        }

        var server = await _minesproutClient.GetServerAsync(serverId.Value);
        if (server is null)
        {
            await ctx.RespondAsync($"Server with id `{serverId.Value}` not found!");
            return;
        }

        var ui = new FridayUIBuilder()
        {
            Duration = TimeSpan.FromMinutes(5)
        };

        ui.OnRenderAsync(async x =>
        {
            await ServerUI.RenderAsync(server, x, _module, _minesproutClient);
            
            x.AddButton(delete =>
            {
                delete.Style = ButtonStyle.Danger;
                delete.Emoji = DiscordEmoji.FromName(ctx.Client, ":wastebasket:");
                delete.OnClick(() => x.SubPage = "deleteConfirm");
            });
            
            x.AddSubPage("deleteConfirm", deleteConfirm =>
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
                            x.SubPage = null;
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
                            x.SubPage = null;
                            x.OnCancelledAsync(async (_, msg) =>
                            {
                                try
                                {
                                    await msg.DeleteAsync();
                                } catch { /* ignored */ }
                            });
                            x.Stop();
                        });
                    });
                });
        });

        await ctx.SendUIAsync(ui);
    }
}