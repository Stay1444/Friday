using System.Text;
using DSharpPlus.Entities;

namespace Friday.Modules.ChannelStats.Entities.Vars;

public class RoleCountVariable : Variable
{
    public override string Name { get; } = "roleCount";
    public override string Description { get; } = "The amount of members in a specified role with a specified state.";
    public override string[] Parameters { get; } = { "roleId", "(online, offline)" };
    public override Task<string?> Process(ChannelStatsModule module, DiscordGuild guild, string[] parameters)
    {
        var members = guild.Members.Values.ToList();
        
        if (parameters.Length != 2) return Task.FromResult<string?>(null);
        
        var role = guild.Roles.Values.FirstOrDefault(x => x.Id.ToString() == parameters[0].ToLower());
        if (role == null) return Task.FromResult<string?>(null);
        
        var state = parameters[1].ToLower();
        if (state != "online" && state != "idle" && state != "dnd") return Task.FromResult<string?>(null);
        
        var count = 0;
        foreach (var member in members)
        {
            if (member.Roles.Contains(role))
            {
                if (state == "online" && member.Presence.Status != UserStatus.Offline)
                {
                    count++;
                }           
                
                if (state == "offline" && member.Presence.Status == UserStatus.Offline)
                {
                    count++;
                }
            }
        }
        
        return Task.FromResult(count.ToString())!;
    }

    public override Task<string> Example(ChannelStatsModule module, DiscordGuild guild)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("```");
        stringBuilder.Append("{");
        stringBuilder.Append(Name);
        stringBuilder.AppendLine($"[{guild.Roles.Values.Last().Id}, online]}} Staffs Online");
        stringBuilder.AppendLine("```");
        stringBuilder.AppendLine($"Shows members **online** with the role specified (@{guild.Roles.Values.Last().Name})");
        return Task.FromResult(stringBuilder.ToString());
    }
}