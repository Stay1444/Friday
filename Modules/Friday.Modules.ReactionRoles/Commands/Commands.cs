using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Attributes;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Modules.ReactionRoles.Entities;
using Friday.Modules.ReactionRoles.UI;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.ReactionRoles.Commands;

[Group("rrole")]
[FridayRequirePermission(Permissions.Administrator)]
[RequireGuild]
[Aliases("reactionrole")]
public class Commands : FridayCommandModule
{
    private readonly FridayConfiguration _configuration;
    private readonly ReactionRoles _module;
    
    public Commands(ReactionRoles module, FridayConfiguration configuration)
    {
        _module = module;
        _configuration = configuration;
    }

    [GroupCommand]
    [Priority(0)]
    public async Task ReactionRoleUI(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder
        {
            Duration = TimeSpan.FromMinutes(5)
        };
        var rrolesEnumerable = await _module.GetReactionRolesAsync(ctx.Guild);
        uiBuilder.OnRenderAsync(async x =>
        {
            var reactionRoles = x.GetState("rroles", rrolesEnumerable.ToList());
            var index = x.GetState("pageIndex", 0);
            x.Embed.Transparent();
            x.Embed.WithTitle($"Reaction Roles - {ctx.Guild.Name}");

            index.Value = Math.Clamp(index.Value, 0, Math.Max(reactionRoles.Value.Count() - 1, 0));

            if (reactionRoles.Value.IsEmpty())
            {
                x.Embed.WithDescription(await ctx.GetString("common.empty"));
            }
            else
            {
                var selected = reactionRoles.Value[index.Value];

                if (!ctx.Guild.Channels.ContainsKey(selected.ChannelId))
                {
                    x.Embed.WithDescription(":warning: " + await ctx.GetString("rr.msg.channel-not-found"));
                }
                else
                {
                    foreach (var roleId in selected.RoleIds)
                    {
                        if (!ctx.Guild.Roles.ContainsKey(roleId))
                        {
                            x.Embed.WithDescription(":warning: " + await ctx.GetString("rr.msg.role-not-found"));
                            break;
                        }
                    }
                    
                    var lastCheckIdResult = x.GetState("lastcheckidresult", (true, (ulong)0));

                    if (lastCheckIdResult.Value.Item2 != selected.Id)
                    {
                        try
                        {
                            await ctx.Guild.Channels[selected.ChannelId].GetMessageAsync(selected.MessageId);
                                
                            lastCheckIdResult.Value = (true, selected.Id);
                        }
                        catch
                        {
                            lastCheckIdResult.Value = (false, selected.Id);
                        } 
                    }

                    if (!lastCheckIdResult.Value.Item1)
                    {
                        x.Embed.WithDescription(":warning: " + await ctx.GetString("rr.msg.message-not-found"));
                    }
                }

                x.Embed.AddField("Channel", $"<#{selected.ChannelId}>", true);
                x.Embed.AddField("Message Id", "`" + selected.MessageId + "`", true);
                if (selected.Emoji is not null) x.Embed.AddField("Emote", selected.Emoji);

                x.Embed.AddField("Behaviour", "`" + selected.Behaviour + "`");
                x.Embed.AddField("Send Message", selected.SendMessage.ToString());

                if (!string.IsNullOrEmpty(selected.Warning))
                {
                    x.Embed.AddField(":warning: " + await ctx.GetString("common.warning"), await ctx.GetString(selected.Warning));
                }
                
                x.Embed.WithFooter($"{index.Value + 1}/{reactionRoles.Value.Count}");
                if (!selected.RoleIds.IsEmpty())
                {
                    x.Embed.AddField("Roles", string.Join(", ", selected.RoleIds.Select(r => $"<@&{r}>")).MaxLength(1000));
                }
            }

            x.AddButton(left =>
            {
                left.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");
                left.Disabled = reactionRoles.Value.Count < 2;
                left.OnClick(() =>
                {
                    index.Value--;
                    if (index.Value < 0) index.Value = reactionRoles.Value.Count - 1;

                    index.Value = Math.Clamp(index.Value, 0, Math.Max(reactionRoles.Value.Count - 1, 0));
                });
            });

            x.AddButton(empty =>
            {
                empty.Disabled = true;
                empty.Emoji = DiscordEmoji.FromGuildEmote(ctx.Client, _configuration.Emojis.Transparent);
            });

            x.AddButton(right =>
            {
                right.Emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
                right.Disabled = reactionRoles.Value.Count < 2;
                right.OnClick(() =>
                {
                    index.Value++;

                    if (index.Value > reactionRoles.Value.Count - 1) index.Value = 0;

                    index.Value = Math.Clamp(index.Value, 0, Math.Max(reactionRoles.Value.Count() - 1, 0));
                });
            });

            x.NewLine();

            await x.AddButton(async edit =>
            {
                edit.Label = await ctx.GetString("common.edit");
                edit.Disabled = reactionRoles.Value.Count < 1;
                edit.Style = ButtonStyle.Primary;
                edit.Emoji = DiscordEmoji.FromName(ctx.Client, ":pencil2:");
                
                edit.OnClick(() => x.SubPage = "edit");
            });

            await x.AddButton(async delete =>
            {
                delete.Label = await ctx.GetString("common.delete");
                delete.Emoji = DiscordEmoji.FromName(ctx.Client, ":wastebasket:");
                delete.Style = ButtonStyle.Danger;
                delete.Disabled = reactionRoles.Value.Count < 1;
                
                delete.OnClick(() =>
                {
                    x.SubPage = "delete";
                });
            });

            await x.AddButton(async @new =>
            {
                @new.Label = await ctx.GetString("common.new");
                @new.Style = ButtonStyle.Success;
                @new.OnClick(() => x.SubPage = "new");
            });
            
            x.AddSubPageAsync("delete", async deletePage =>
            {
                deletePage.Embed.WithTitle(await ctx.GetString("rr.embeds.delete-confirm.title"));
                deletePage.Embed.Description = await ctx.GetString("rr.embeds.delete-confirm.description",
                        reactionRoles.Value[index.Value].Emoji);
                deletePage.Embed.Color = DiscordColor.IndianRed;

                await deletePage.AddButton(async cancel =>
                {
                    cancel.Label = await ctx.GetString("common.cancel");
                    cancel.OnClick(() => x.SubPage = null);
                });
                
                await deletePage.AddButton(async delete =>
                {
                    delete.Label = await ctx.GetString("rr.embeds.delete-confirm.no");
                    delete.Style = ButtonStyle.Danger;
                    delete.OnClick(async () =>
                    {
                        await _module.DeleteReactionRoleAsync(reactionRoles.Value[index.Value]);
                        x.SubPage = null;
                        
                        rrolesEnumerable = await _module.GetReactionRolesAsync(ctx.Guild);
                        reactionRoles.Value = rrolesEnumerable.ToList();
                    });
                });
                
                await deletePage.AddButton(async remove =>
                {
                    remove.Label = await ctx.GetString("rr.embeds.delete-confirm.yes");
                    remove.Style = ButtonStyle.Success;
                    remove.OnClick(async () =>
                    {
                        await _module.DeleteReactionRoleAsync(reactionRoles.Value[index.Value]);
                        x.SubPage = null;
                        try
                        {
                            var message = await ctx.Guild.Channels[reactionRoles.Value[index.Value].ChannelId]
                                .GetMessageAsync(reactionRoles.Value[index.Value].MessageId);
                            await message.DeleteReactionsEmojiAsync(Utils.FromGeneric(ctx.Client, reactionRoles.Value[index.Value].Emoji));
                        }
                        catch(Exception er)
                        {
                            Console.WriteLine(er);
                        }
                        rrolesEnumerable = await _module.GetReactionRolesAsync(ctx.Guild);
                        reactionRoles.Value = rrolesEnumerable.ToList();
                    });
                });
            });
            
            x.AddSubPageAsync("new", async newPage =>
            {
                newPage.Embed.Transparent();
                newPage.Embed.WithDescription(await ctx.GetString("rr.embeds.new.description",
                    $"```\n{ctx.Prefix}rrole <#channel> <messageId> <emoji> <roles>\n```"));
                if (ctx.Member!.Roles.IsEmpty())
                {
                    newPage.Embed.AddField(await ctx.GetString("common.example"), $"{ctx.Prefix}rrole {ctx.Channel.Mention} {ctx.Message.Id} :earth_africa: @example");
                }
                else
                {
                    newPage.Embed.AddField(await ctx.GetString("common.example"), $"{ctx.Prefix}rrole {ctx.Channel.Mention} {ctx.Message.Id} :earth_africa: @{ctx.Member.Roles.First().Name}");
                }

                x.Stop();
            });
            
            x.AddSubPageAsync("edit", async editPage =>
            {
                await editPage.AddButton(async back =>
                {
                    back.Label = await ctx.GetString("common.back");
                    back.OnClick(() =>
                    {
                        x.SubPage = null;
                    });
                });
                await editPage.ReactionRolesEdit(ctx, reactionRoles.Value[index.Value], _module);
            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }

    [GroupCommand]
    [Priority(5)]
    public async Task ReactionRoleAdd(CommandContext ctx, DiscordChannel channel, ulong messageId, DiscordEmoji emoji,
        params DiscordRole[] roles)
    {

        if (roles.Any(x => x.IsManaged))
        {
            await ctx.RespondAsync(await ctx.GetString("rr.msg.managed-roles"));
            return;
        }

        if (roles.Any(x => x.Id == ctx.Guild.EveryoneRole.Id))
        {
            await ctx.RespondAsync(await ctx.GetString("rr.msg.everyone-role"));
            return;
        }
        
        var rr = new ReactionRole
        {
            ChannelId = channel.Id,
            MessageId = messageId,
            Emoji = emoji.GetDiscordName(),
            RoleIds = roles.Select(x => x.Id).ToList(),
            Behaviour = ReactionRoleBehaviour.Toggle
        };
        
        var message = await channel.GetMessageAsync(messageId);

        if (message == null)
        {
            await ctx.RespondAsync(await ctx.GetString("rr.msg.message-not-found"));
            return;
        }

        try
        {
            await message.CreateReactionAsync(emoji);
        }
        catch
        {
            await ctx.RespondAsync(await ctx.GetString("rr.msg.emoji-failed"));
            return;
        }

        await _module.InsertReactionRole(message, rr);
        await ctx.RespondAsync(await ctx.GetString("rr.msg.done"));
    }
}