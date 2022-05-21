using System.Text;
using DSharpPlus.Entities;

namespace Friday.Modules.ChannelStats.Entities.Vars;

public class MemberCountTypeVariable : Variable
{
    public override string Name { get; } = "memberCountType";
    public override string Description { get; } = "Member count by type";
    public override string[] Parameters { get; } = { "(bots, humans)" };
    public override Task<string?> Process(ChannelStatsModule module, DiscordGuild guild, string[] parameters)
    {
        if (parameters.Length == 0)
        {
            return Task.FromResult<string?>(null);
        }
        
        var type = parameters[0];
        if (type == "humans")
        {
            return Task.FromResult(guild.Members.Count(x => !x.Value.IsBot).ToString())!;
        }else if (type == "bots")
        {
            return Task.FromResult(guild.Members.Count(x => x.Value.IsBot).ToString())!;
        }
        
        return Task.FromResult<string?>(null);
    }

    public override Task<string> Example(ChannelStatsModule module, DiscordGuild guild)
    {
        if (DateTime.UtcNow.Ticks % 2 == 0)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("```");
            stringBuilder.Append("{");
            stringBuilder.Append(Name);
            stringBuilder.AppendLine($"[bots]}} Bots");
            stringBuilder.AppendLine("```");
            stringBuilder.AppendLine($"Shows the number of bots in the server");
            return Task.FromResult(stringBuilder.ToString());
        }
        else
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("```");
            stringBuilder.Append("{");
            stringBuilder.Append(Name);
            stringBuilder.AppendLine($"[humans]}} Users");
            stringBuilder.AppendLine("```");
            stringBuilder.AppendLine($"Shows the number of members in the server without counting bots");
            
            return Task.FromResult(stringBuilder.ToString());
        }
    }
}