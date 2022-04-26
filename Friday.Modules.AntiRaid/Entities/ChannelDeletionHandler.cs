using DSharpPlus.Entities;
using Friday.Common;

namespace Friday.Modules.AntiRaid.Entities;

public class ChannelDeletionHandler
{
    private GuildAntiRaid _guildAntiRaid;
    private DiscordGuild _guild;
    private DiscordUser _user;
    private List<DiscordChannel> _deletedChannels = new List<DiscordChannel>();
    private Dictionary<ulong, List<ulong>> _channelChildren = new Dictionary<ulong, List<ulong>>();
    private DateTime? _lastUpdate;
    public ChannelDeletionHandler(GuildAntiRaid guildAntiRaid, DiscordGuild guild, DiscordUser user)
    {
        _guildAntiRaid = guildAntiRaid;
        _guild = guild;
        _user = user;
    }

    public async Task Handle(DiscordChannel channel)
    {
        var member = await _guild.GetMemberAsync(_user.Id);

        if (member is null) return;

        if (_guildAntiRaid.GuildChannelChildren.ContainsKey(channel.Id) && _guildAntiRaid.GuildChannelChildren[channel.Id].Count > 0)
        {
            _channelChildren.Add(channel.Id, _guildAntiRaid.GuildChannelChildren[channel.Id].ToList());
        }
        
        if (_lastUpdate is not null && _lastUpdate + _guildAntiRaid.Settings!.DeleteChannels.Time < DateTime.UtcNow)
        {
            _deletedChannels.Clear();
            _channelChildren = _channelChildren.Where(x => x.Key == channel.Id).ToDictionary(x => x.Key, x => x.Value);
            _lastUpdate = DateTime.UtcNow;
            _deletedChannels.Add(channel);

            return;
        }
        
        _deletedChannels.Add(channel);
        
        if (_lastUpdate is null) _lastUpdate = DateTime.UtcNow;

        if (_deletedChannels.Count > _guildAntiRaid.Settings!.DeleteChannels.Count)
        {
            if (_guildAntiRaid.ShouldLog(_guild, out var logChannel))
            {
                await logChannel!.SendMessageAsync(new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.IndianRed)
                        .WithTitle("AntiRaid - Channel Deletion")
                        .WithDescription($"{_user.Mention} deleted {_guildAntiRaid.Settings!.DeleteChannels.Count} channels in {_guildAntiRaid.Settings!.DeleteChannels.Time.ToHumanTimeSpan().Humanize()}")
                        .WithTimestamp(DateTime.UtcNow)
                        .Build()));
            }
            
            if (_guildAntiRaid.Settings.DeleteChannels.Ban)
            {
                try
                {
                    await member.BanAsync(0, "AntiRaid - Channel Deletion");
                }catch
                {
                    // ignored
                }
            }else
            {
                try
                {
                    await member.RemoveAsync("AntiRaid - Channel Deletion");
                }catch
                {
                    // ignored
                }
            }
            
            if (_guildAntiRaid.Settings!.DeleteChannels.Restore)
            {
                var mapping = new Dictionary<ulong, ulong>();
                foreach (var deletedChannel in _deletedChannels.OrderBy(x => x.Type))
                {
                    try
                    {
                        DiscordChannel? parent;
                        if (deletedChannel.Parent is not null && mapping.ContainsKey(deletedChannel.Parent.Id))
                        {
                            parent = _guild.GetChannel(mapping[deletedChannel.Parent.Id]);
                        }else
                        {
                            parent = deletedChannel.Parent;
                        }
                        
                        var newChannel = await _guild.CreateChannelAsync(deletedChannel.Name, deletedChannel.Type, parent, deletedChannel.Topic, deletedChannel.Bitrate, deletedChannel.UserLimit, null, deletedChannel.IsNSFW, deletedChannel.PerUserRateLimit, deletedChannel.QualityMode, deletedChannel.Position);
                        mapping.Add(deletedChannel.Id, newChannel.Id);

                        if (_channelChildren.ContainsKey(deletedChannel.Id))
                        {
                            foreach (var channelChild in _channelChildren[deletedChannel.Id])
                            {
                                var newChild = _guild.GetChannel(channelChild);
                                if (newChild is not null)
                                {
                                    await newChild.ModifyAsync(x =>
                                    {
                                        x.Parent = newChannel;
                                    });
                                }
                            }
                        }

                        if (_guildAntiRaid.GuildChannelMessages.ContainsKey(deletedChannel.Id))
                        {
                            var webHook = await newChannel.CreateWebhookAsync("AntiRaid - Channel Deletion", null, "AntiRaid - Channel Deletion");
                            if (webHook is not null)
                            {
                                foreach (var message in _guildAntiRaid.GuildChannelMessages[deletedChannel.Id])
                                {
                                    await webHook.ExecuteAsync(new DiscordWebhookBuilder()
                                        .WithUsername($"{message.Author.Username}#{message.Author.Discriminator}")
                                        .WithAvatarUrl(message.Author.AvatarUrl)
                                        .WithContent(message.Content)
                                        .AddEmbeds(message.Embeds));
                                }
                            }
                            _guildAntiRaid.GuildChannelMessages.Remove(deletedChannel.Id);
                        }
                    }catch
                    {
                        // ignored
                    }
                }
            }

            _deletedChannels.Clear();
            _channelChildren.Clear();
            _lastUpdate = DateTime.UtcNow;
            
            return;
        }
        
        if (_guildAntiRaid.ShouldLog(_guild, out var logChannelf))
        {
            await logChannelf!.SendMessageAsync(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Orange)
                    .WithTitle("AntiRaid - Channel Deletion")
                    .WithDescription($"{_user.Mention} deleted #{channel.Name}.")
                    .AddField("Count", _deletedChannels.Count + "/" + _guildAntiRaid.Settings.DeleteChannels.Count)
                    .WithTimestamp(DateTime.UtcNow)
                    .Build()));
        }
    }
}