using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Entities;

namespace Friday.Modules.Misc.Commands;

public partial class Commands
{
    [Command("botinfo")]
    public async Task BotInfoCommand(CommandContext ctx)
    {
        var embedBuilder = new DiscordEmbedBuilder();
        embedBuilder.Transparent();
        embedBuilder.WithTitle(ctx.Client.CurrentUser.Username);
        embedBuilder.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

        embedBuilder.AddField("Version", $"```{Constants.Version}```", true);
        embedBuilder.AddField("Shard", $"```{ctx.Client.ShardId + 1}/{ctx.Client.ShardCount}```", true);
        embedBuilder.AddField("Uptime", $"```{new HumanTimeSpan(DateTime.UtcNow - Constants.ProcessStartTimeUtc).Humanize(2)}```", true);
        embedBuilder.AddField("Host", $"```{Environment.OSVersion}```", true);
        Process proc = Process.GetCurrentProcess();
        var memory = proc.PrivateMemorySize64;
        var memoryString = "";
        
        if (memory < 1024)
        {
            memoryString = $"{memory} bytes";
        }
        else if (memory < 1048576)
        {
            memoryString = $"{memory / 1024} KB";
        }
        else if (memory < 1073741824)
        {
            memoryString = $"{memory / 1048576} MB";
        }
        else
        {
            memoryString = $"{memory / 1073741824} GB";
        }
        
        embedBuilder.AddField("Memory", $"```{memoryString}```", true);
        embedBuilder.AddField("Ping", $"```{ctx.Client.Ping}ms```", true);
        embedBuilder.AddField("Guilds", $"```{ctx.Client.Guilds.Count}```", true);
        embedBuilder.AddField("Users", $"```{ctx.Client.Guilds.Values.Sum(x => x.MemberCount)}```", true);
        
        
        await using var connection = _databaseProvider.GetConnection();
        var stopWatch = Stopwatch.StartNew();
        await connection.OpenAsync();
        connection.Ping();
        await connection.CloseAsync();
        stopWatch.Stop();
        embedBuilder.AddField("DB Ping", $"```{stopWatch.ElapsedMilliseconds}ms```", true);
        
        await ctx.RespondAsync(embed: embedBuilder);
    }
}