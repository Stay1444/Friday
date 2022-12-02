using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.ReactionRoles.Entities;
using Friday.Modules.ReactionRoles.Services;

namespace Friday.Modules.ReactionRoles;

public class ReactionRoles : ModuleBase
{
    internal DatabaseHelper DatabaseHelper { get; }
    internal DiscordShardedClient Client;
    private LanguageProvider _languageProvider;
    public ReactionRoles(DatabaseProvider provider, DiscordShardedClient client, LanguageProvider languageProvider)
    {
        Client = client;
        _languageProvider = languageProvider;
        DatabaseHelper = new DatabaseHelper(provider);
    }
    
    public override Task OnLoad()
    {
        Client.MessageReactionAdded += ClientOnMessageReactionAdded;
        Client.MessageReactionRemoved += ClientOnMessageReactionRemoved;
        return Task.CompletedTask;
    }

    

    private async Task ClientOnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        if (e.User.IsBot) return;
        
        var reactionRoles = await GetReactionRolesAsync(e.Message);
        
        foreach (var reactionRole in reactionRoles)
        {
            if (reactionRole.Emoji is not null && reactionRole.Emoji == e.Emoji.GetDiscordName())
            {
                var member = await e.Guild.GetMemberAsync(e.User.Id);
                
                if (member is null)
                    return;
                
                foreach (var roleId in reactionRole.RoleIds)
                {
                    var role = e.Guild.GetRole(roleId);
                    
                    if (role is null)
                        continue;

                    if (reactionRole.Behaviour == ReactionRoleBehaviour.Add || reactionRole.Behaviour == ReactionRoleBehaviour.Toggle)
                    {
                        if (member.Roles.Contains(role)) continue;

                        try
                        {
                            await member.GrantRoleAsync(role, "Reaction Role");
                        }
                        catch (UnauthorizedException)
                        {
                            reactionRole.Warning = await _languageProvider.GetString(member, "rr.warns.not-enough-permissions");
                            await DatabaseHelper.UpdateReactionRoleAsync(reactionRole);
                        }
                        catch
                        {
                            reactionRole.Warning = await _languageProvider.GetString(member, "rr.warns.unknown");
                            await DatabaseHelper.UpdateReactionRoleAsync(reactionRole);
                        }
                    }
                    
                    if (reactionRole.Behaviour == ReactionRoleBehaviour.Remove)
                    {
                        if (!member.Roles.Contains(role)) continue;
                        
                        try
                        {
                            await member.RevokeRoleAsync(role, "Reaction Role");
                        }catch
                        {
                            // ignored
                        }
                    }
                }
                
                break;
            }
        }
    }
    
    private async Task ClientOnMessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        if (e.User.IsBot) return;

        var reactionRoles = await GetReactionRolesAsync(e.Message);
        
        foreach (var reactionRole in reactionRoles)
        {
            if (reactionRole.Emoji is not null && reactionRole.Emoji == e.Emoji.GetDiscordName())
            {
                var member = await e.Guild.GetMemberAsync(e.User.Id);
                
                if (member is null)
                    return;
                
                foreach (var roleId in reactionRole.RoleIds)
                {
                    var role = e.Guild.GetRole(roleId);
                    
                    if (role is null)
                        continue;

                    if (reactionRole.Behaviour == ReactionRoleBehaviour.Toggle)
                    {
                        if (!member.Roles.Contains(role)) continue;
                        
                        try
                        {
                            await member.RevokeRoleAsync(role, "Reaction Role");
                        }catch
                        {
                            // ignored
                        }
                    }
                }
                
                break;
            }
        }
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ReactionRole>> GetReactionRolesAsync(ulong guildId)
        => DatabaseHelper.GetReactionRolesAsync(guildId);

    public Task<IEnumerable<ReactionRole>> GetReactionRolesAsync(ulong guildId, ulong channelId, ulong messageId)
        => DatabaseHelper.GetReactionRolesAsync(guildId, channelId, messageId);

    public Task DeleteReactionRoleAsync(ReactionRole reactionRole)
        => DatabaseHelper.DeleteReactionRoleAsync(reactionRole.Id);

    public Task InsertReactionRole(DiscordMessage message, ReactionRole reactionRole)
    {
        return DatabaseHelper.InsertReactionRoleAsync(message.Channel.Guild.Id, message.ChannelId, message.Id,
            reactionRole);
    }

    public Task UpdateReactionRole(ReactionRole role)
    {
        return DatabaseHelper.UpdateReactionRoleAsync(role);
    }
    
    public Task<IEnumerable<ReactionRole>> GetReactionRolesAsync(DiscordMessage message)
        => GetReactionRolesAsync(message.Channel.Guild.Id, message.Channel.Id, message.Id);
    
    public Task<IEnumerable<ReactionRole>> GetReactionRolesAsync(DiscordGuild guild)
        => GetReactionRolesAsync(guild.Id);
}