using System.Text;
using DSharpPlus.Entities;

namespace Friday.Modules.ChannelStats.Entities.Vars;

public class MemberCountVariable : Variable
{
    public override string Name { get; } = "memberCount";
    public override string Description { get; } = "Total member count in the server";
    public override string[] Parameters { get; } = Array.Empty<string>();

    public override Task<string?> Process(ChannelStatsModule module, DiscordGuild guild, string[] parameters)
        => Task.FromResult(guild.MemberCount.ToString())!;

    public override Task<string> Example(ChannelStatsModule module, DiscordGuild guild)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("```");
        stringBuilder.Append("{");
        stringBuilder.AppendLine(Name + "} Members");
        stringBuilder.AppendLine("```");
        stringBuilder.AppendLine($"Shows total member count in the server");
        return Task.FromResult(stringBuilder.ToString());
    }
}