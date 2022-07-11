using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Miuex.Modules.Proxmox.Entities;
using Miuex.Modules.Proxmox.Models;

namespace Miuex.Modules.Proxmox;

public static class Extensions
{
    public static void AddDescriptionLine(this DiscordEmbedBuilder builder, string line)
    {
        builder.WithDescription(builder.Description + "\n" + line);
    }

    public static async Task SendPaginatedMessageAsync(this InteractionContext context, List<DiscordEmbed> pages)
    {
        PaginatedMessage paginatedMessage = new PaginatedMessage(context, pages);
        await paginatedMessage.SendAsync(TimeSpan.FromMinutes(5));
    }
    
    public static double Round(this double value, int digits)
    {
        if (digits < 0)
            throw new ArgumentOutOfRangeException("digits", "Value must be 0 or greater");
        return Math.Round(value, digits);
    }

    public static bool IsAdmin(this InteractionContext ctx)
    {
        var configuration = ctx.Services.GetService(typeof(Configuration)) as Configuration;

        if (configuration is null) return false;
        
        return ctx.Member.Roles.Any(x => x.Id == configuration.AdminRole);
    }

    public static async Task Log(this InteractionContext ctx, DiscordEmbedBuilder builder)
    {
        try
        {
            await Log(builder);
        }catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public static string RemoveHttpPrefix(this string input)
    {
        return input.Replace("https://", "").Replace("http://", "").Replace("www.", "").Replace("/", "");
    }

    public static async Task Log(DiscordEmbedBuilder builder)
    {
        try
        {
            if (Constants.Config is null || Constants.Instance is null)
            {
                Console.WriteLine("Config or Instance is null");
                return;
            }

            if (Constants.Config.LogsChannel is null)
            {
                Console.WriteLine("Logs channel is null");
                return;
            }

            var channel = await Constants.Instance.GetChannelAsync(Constants.Config.LogsChannel.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}