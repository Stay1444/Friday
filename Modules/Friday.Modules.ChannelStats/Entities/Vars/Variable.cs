using DSharpPlus.Entities;

namespace Friday.Modules.ChannelStats.Entities.Vars;

public abstract class Variable
{
    public abstract string Name { get; }
    public string CurlyBracedName => $"{{{Name}}}";
    public abstract string Description { get; }
    public abstract string[] Parameters { get; }
    public abstract Task<string?> Process(ChannelStatsModule module, DiscordGuild guild, string[] parameters);
    public abstract Task<string> Example(ChannelStatsModule module, DiscordGuild guild);
}