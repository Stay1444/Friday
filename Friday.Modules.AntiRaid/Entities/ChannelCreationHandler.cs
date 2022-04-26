using DSharpPlus.Entities;
using Friday.Common;

namespace Friday.Modules.AntiRaid.Entities;

internal class ChannelCreationHandler
{
    private GuildAntiRaid _guildAntiRaid;
    private DiscordGuild _guild;
    private DiscordUser _user;

    private List<DiscordChannel> _channels = new List<DiscordChannel>();
    private DateTime? _lastUpdate;
    public ChannelCreationHandler(GuildAntiRaid guildAntiRaid, DiscordGuild guild, DiscordUser user)
    {
        _guildAntiRaid = guildAntiRaid;
        _guild = guild;
        _user = user;
    }

    public async Task Handle(DiscordChannel channel)
    {
        
        var member = await _guild.GetMemberAsync(_user.Id);
        
        if (member is null) return;
        
        if (_lastUpdate is not null && _lastUpdate + _guildAntiRaid.Settings!.CreateChannels.Time < DateTime.UtcNow)
        {
            _channels.Clear();
            _lastUpdate = DateTime.UtcNow;
            _channels.Add(channel);
            return;
        }
        
        _channels.Add(channel);
        
        if (_lastUpdate is null) _lastUpdate = DateTime.UtcNow;

        if (_channels.Count > _guildAntiRaid.Settings!.CreateChannels.Count)
        {
            if (_guildAntiRaid.ShouldLog(_guild, out var logChannel))
            {
                await logChannel!.SendMessageAsync(new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.IndianRed)
                        .WithTitle("AntiRaid - Channel Creation")
                        .WithDescription($"{_user.Mention} created {_guildAntiRaid.Settings!.CreateChannels.Count} channels in {_guildAntiRaid.Settings!.CreateChannels.Time.ToHumanTimeSpan().Humanize()}")
                        .WithTimestamp(DateTime.UtcNow)
                        .Build()));
            }
            
            if (_guildAntiRaid.Settings.CreateChannels.Ban)
            {
                try
                {
                    await member.BanAsync(0, "AntiRaid - Channel Creation");
                }catch
                {
                    // ignored
                }
            }else
            {
                try
                {
                    await member.RemoveAsync("AntiRaid - Channel Creation");
                }catch
                {
                    // ignored
                }
            }
            
            if (_guildAntiRaid.Settings!.CreateChannels.Restore)
            {
                foreach (var createdChannel in _channels)
                {
                    try
                    {
                        await createdChannel.DeleteAsync("AntiRaid - Channel Creation");
                    }catch
                    {
                        // ignored
                    }
                }
            }
            
            _channels.Clear();
            _lastUpdate = null;
            
            return;
        }
        
        if (_guildAntiRaid.ShouldLog(_guild, out var logChannelf))
        {
            await logChannelf!.SendMessageAsync(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Orange)
                    .WithTitle("AntiRaid - Channel Creation")
                    .WithDescription($"{_user.Mention} created {channel.Mention}.")
                    .AddField("Count", _channels.Count + "/" + _guildAntiRaid.Settings.CreateChannels.Count)
                    .WithTimestamp(DateTime.UtcNow)
                    .Build()));
        }
        
    }
}