using DSharpPlus.Entities;
using Friday.Modules.Minesprout.Minesprout;
using Friday.Modules.Minesprout.Minesprout.Entities;
using Friday.UI.Entities;
using ReverseMarkdown;

namespace Friday.Modules.Minesprout.UI;

public static class ServerUI
{
    public static async Task RenderAsync(IMinesproutServer server, FridayUIPage page, MinesproutModule module, MinesproutClient apiClient)
    {
        page.Embed.Title = server.Name ?? "Unnamed";
        if (!string.IsNullOrWhiteSpace(server.Description))
        {
            page.Embed.Description = new Converter().Convert(server.Description);
        }

        page.Embed.Color = DiscordColor.SpringGreen;


        try
        {
            var banner =
                await module.BannerResolver.ResolveAsync(apiClient.GetBannerUrl(server.Id), server.Id);
            page.Embed.WithImageUrl(banner);
        }
        catch
        {
            // ignored
        }

        if (server.Status?.Favicon is not null)
        {
            var icon = await module.IconResolver.ResolveAsync(server.Status.Favicon, server.Id);
            page.Embed.WithThumbnail(new Uri(icon));
        }

        page.Embed.AddField("IP", $"`{server.Ip}`", true);
        page.Embed.AddField("Version", $"{server.MinVersion} - {server.MaxVersion}", true);

        if (server.Status?.Players is not null)
            page.Embed.AddField("Players", $"{server.Status.Players.Online} / {server.Status.Players.Max}",
                true);

        page.Embed.AddField("Main Mode", server.MainMode, true);
        page.Embed.AddField("Type", server.Type, true);
        page.Embed.AddField("Country", server.Country, true);

        page.Embed.WithFooter($"{server.Id}", "https://minesprout.net/img/minesproutlogo.png");
    }
}