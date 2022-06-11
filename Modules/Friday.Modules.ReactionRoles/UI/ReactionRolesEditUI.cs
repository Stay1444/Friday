using DSharpPlus;
using DSharpPlus.CommandsNext;
using Friday.Common;
using Friday.Modules.ReactionRoles.Entities;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.ReactionRoles.UI;

internal static class ReactionRolesEditUI
{
    public static async Task ReactionRolesEdit(this FridayUIPage page, CommandContext ctx, ReactionRole reactionRole, ReactionRolesModule module)
    {
        page.Embed.Transparent();
        page.Embed.WithTitle("Reaction Roles - Edit");
        page.Embed.AddField("Channel", $"<#{reactionRole.ChannelId}>", true);
        page.Embed.AddField("Message Id", $"`{reactionRole.MessageId}`", true);
        page.Embed.AddField("Roles", string.Join(", ", reactionRole.RoleIds.Select(r => $"<@&{r}>")).MaxLength(1000));
        page.Embed.AddField("Behaviour", reactionRole.Behaviour.ToString(), true);
        page.Embed.AddField("Send Message", reactionRole.SendMessage.ToString(), true);
        
        page.NewLine();
        
        page.AddButton(editMessage =>
        {
            editMessage.Label = "Message";
            editMessage.Style = ButtonStyle.Primary;
            editMessage.OnClick(() => page.SubPage = "edit-message");
        });

        page.AddButton(editRoles =>
        {
            editRoles.Label = "Roles";
            editRoles.Style = ButtonStyle.Primary;
            editRoles.OnClick(() => page.SubPage = "edit-roles");
        });
        
        page.AddButton(editOthers =>
        {
            editOthers.Label = "Others";
            editOthers.Style = ButtonStyle.Primary;
            editOthers.OnClick(() => page.SubPage = "edit-others");
        });
        
        page.AddSubPage("edit-message", editMessagePage =>
        {
            editMessagePage.Embed.Transparent();
            editMessagePage.Embed.WithTitle("Reaction Roles - Edit");
            editMessagePage.Embed.WithDescription("Edit the message location (channel & message id).");


            editMessagePage.AddButton(back =>
            {
                back.Label = "Back";
                back.OnClick(() => page.SubPage = null);
            });
            
            editMessagePage.NewLine();

            editMessagePage.AddSelect(ch =>
            {
                ch.Placeholder = "Select a channel";
                ch.Disabled = ctx.Guild.Channels.All(x => x.Value.Type != ChannelType.Text);
                foreach (var discordChannel in ctx.Guild.Channels.Values)
                {
                    if (discordChannel.Type != ChannelType.Text) continue;

                    ch.AddOption(option =>
                    {
                        option.Label = $"#{discordChannel.Name}";
                        option.Description = discordChannel.Parent?.Name ?? null;
                        option.Value = discordChannel.Id.ToString();
                        option.IsDefault = discordChannel.Id == reactionRole.ChannelId;
                    });
                }
                
                ch.OnSelect(async selections =>
                {
                    if (selections.IsEmpty()) return;
                    
                    reactionRole.ChannelId = ulong.Parse(selections.First());
                    
                    await module.UpdateReactionRole(reactionRole);
                });
            });
            
            editMessagePage.NewLine();
            
            editMessagePage.AddModal(mid =>
            {
                mid.Title = "Edit Message Id";
                mid.ButtonLabel = "Message Id";
                mid.ButtonStyle = ButtonStyle.Primary;
                
                mid.AddField("message-id", field =>
                {
                    field.Placeholder = "Message Id";
                    field.Required = true;
                    field.MinimumLength = 1;
                    field.MaximumLength = 100;
                    field.Title = "Message Id";
                    field.Value = reactionRole.MessageId.ToString();
                });
                
                
                mid.OnSubmit(async fields =>
                {
                    var messageIdString = fields["message-id"];
                    
                    if (!ulong.TryParse(messageIdString, out var messageId))
                    {
                        return;
                    }
                    
                    reactionRole.MessageId = messageId;

                    await module.UpdateReactionRole(reactionRole);
                    
                    editMessagePage.ForceRender();
                });
            });
        });

        page.AddSubPage("edit-roles", editRolesPage =>
        {
            editRolesPage.Embed.Transparent();
            editRolesPage.Embed.WithTitle("Reaction Roles - Edit");
            editRolesPage.Embed.WithDescription("Edit the roles to be assigned.");

            editRolesPage.AddSelect(roles =>
            {
                roles.Placeholder = "Select roles";
                roles.MaxOptions = Math.Min(ctx.Guild.Roles.Count(x => !x.Value.IsManaged && x.Value.Id != ctx.Guild.EveryoneRole.Id), 25);
                roles.Disabled = ctx.Guild.Roles.Count == 0;
                roles.MinOptions = 1;
                if (roles.MaxOptions < 1) roles.MaxOptions = 1;
                foreach (var discordRole in ctx.Guild.Roles.Values)
                {
                    if (ctx.Guild.EveryoneRole.Id == discordRole.Id) continue;
                    if (discordRole.IsManaged) continue;
                    roles.AddOption(option =>
                    {
                        option.Label = "@" + discordRole.Name;
                        option.Value = discordRole.Id.ToString();
                        option.IsDefault = reactionRole.RoleIds.Contains(discordRole.Id);

                        if (discordRole.Permissions.HasPermission(Permissions.Administrator))
                        {
                            option.Description = "Administrator";
                        }
                    });
                }
                
                if (roles.Options.Count == 0) {
                    roles.AddOption(option => {
                        option.Label = "No Roles Found";
                        option.Value = "1";
                    })
                }

                roles.OnSelect(async selections =>
                {
                    if (selections.IsEmpty()) return;
                    
                    reactionRole.RoleIds = selections.Select(x => ulong.Parse(x)).ToList();
                    
                    await module.UpdateReactionRole(reactionRole);
                });
            });
            
            editRolesPage.NewLine();
            
            editRolesPage.AddButton(back =>
            {
                back.Label = "Back";
                back.OnClick(() => page.SubPage = null);
            });
        });

        page.AddSubPage("edit-others", editOthersPage =>
        {
            editOthersPage.Embed.Transparent();
            editOthersPage.Embed.WithTitle("Reaction Roles - Edit");
            editOthersPage.Embed.WithDescription("Edit the behaviour and send message settings.");

            editOthersPage.AddSelect(behaviour =>
            {
                behaviour.Placeholder = "Select behaviour";

                behaviour.AddOption(add =>
                {
                    add.Value = ReactionRoleBehaviour.Add.ToString();
                    add.Label = "Add";
                    add.Description = "Add the role(s) to the user";
                    add.IsDefault = reactionRole.Behaviour == ReactionRoleBehaviour.Add;
                });
                
                behaviour.AddOption(remove =>
                {
                    remove.Value = ReactionRoleBehaviour.Remove.ToString();
                    remove.Label = "Remove";
                    remove.Description = "Remove the role(s) from the user";
                    remove.IsDefault = reactionRole.Behaviour == ReactionRoleBehaviour.Remove;
                });
                
                behaviour.AddOption(toggle =>
                {
                    toggle.Value = ReactionRoleBehaviour.Toggle.ToString();
                    toggle.Label = "Toggle";
                    toggle.Description = "Toggle the role(s) on the user";
                    toggle.IsDefault = reactionRole.Behaviour == ReactionRoleBehaviour.Toggle;
                });
                
                
                behaviour.OnSelect(async selections =>
                {
                    if (selections.IsEmpty()) return;
                    
                    reactionRole.Behaviour = (ReactionRoleBehaviour) Enum.Parse(typeof(ReactionRoleBehaviour), selections.First());
                    
                    await module.UpdateReactionRole(reactionRole);
                });
            });
            
            editOthersPage.NewLine();

            editOthersPage.AddButton(back =>
            {
                back.Label = "Back";
                back.OnClick(() => page.SubPage = null);
            });
        });
    }
}