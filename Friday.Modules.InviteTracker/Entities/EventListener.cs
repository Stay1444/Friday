using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Friday.Common;

namespace Friday.Modules.InviteTracker.Entities;

public class EventListener
{
    private InviteTrackerModule _module;
    internal EventListener(InviteTrackerModule module)
    {
        this._module = module;
    }

    public void Register()
    {
        _module.Client.GuildMemberAdded += Client_GuildMemberAdded;
        _module.Client.GuildCreated += Client_GuildCreated;
        _module.Client.GuildDeleted += Client_GuildDeleted;
    }

    public void Unregister()
    {
        _module.Client.GuildMemberAdded -= Client_GuildMemberAdded;
        _module.Client.GuildCreated -= Client_GuildCreated;
        _module.Client.GuildDeleted -= Client_GuildDeleted;
    }

    private async Task Client_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        var config = await _module.GetConfiguration(e.Guild.Id);
        if (!config.Enabled) return;
        if (config.JoinLogChannel == 0) return;
        DiscordChannel? channel = null;
        foreach (var guild in _module.Client.GetGuilds())
        {
            if (guild.Id == e.Guild.Id)
            {
                channel = guild.GetChannel(config.JoinLogChannel);
                break;
            }
        }
        if (channel == null) return;
        var langProvider = _module.LanguageProvider;
        var pastInvites = _module.States[e.Guild.Id];
        var newInvites = await e.Guild.GetInvitesAsync();
        var unchangedInvites = pastInvites.Where(x => newInvites.Any(y => y.Code == x.Code && x.Uses == y.Uses)).ToList();
        var changedInvites = newInvites.Where(x => !unchangedInvites.Any(y => y.Code == x.Code && x.Uses == y.Uses)).ToList();

        foreach (var pastInvite in pastInvites)
        {
            if (pastInvite.MaxUses - pastInvite.Uses == 1 && changedInvites.All(x => x.Code != pastInvite.Code))
            {
                changedInvites.Add(pastInvite);
            }
        }
        if (changedInvites.Count == 0) return;

        var discordEmbedBuilder = new DiscordEmbedBuilder();
        discordEmbedBuilder.Transparent();
        discordEmbedBuilder.WithTitle(await langProvider.GetString(e.Guild, "it.event.join.title"));
        discordEmbedBuilder.Description = e.Member.Mention;
        discordEmbedBuilder.WithAuthor(e.Member.Username, null, e.Member.AvatarUrl);
        discordEmbedBuilder.AddField(await langProvider.GetString(e.Guild, "it.event.join.f1"), $"`{changedInvites.First().Code}`", true);
        discordEmbedBuilder.AddField(await langProvider.GetString(e.Guild, "it.event.join.f2"), changedInvites.First().Inviter.Mention, true);
        discordEmbedBuilder.WithTimestamp(DateTime.UtcNow);
        
        await channel.SendMessageAsync(embed: discordEmbedBuilder);

        _module.States[e.Guild.Id] = newInvites.ToList();
    }

    private async Task Client_GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        if (_module.States.ContainsKey(e.Guild.Id)) return;
        
        _module.States.Add(e.Guild.Id, new List<DiscordInvite>(await e.Guild.GetInvitesAsync()));
    }

    private Task Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
    {
        _module.States.Remove(e.Guild.Id);
        return Task.CompletedTask;
    }
}