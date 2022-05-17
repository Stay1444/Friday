using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.Tickets.Enums;

namespace Friday.Modules.Tickets.Commands;

[Group("ticket")]
public partial class Commands
{
    private TicketModuleBase _module;
    private LanguageProvider _languageProvider;
    public Commands(TicketModuleBase module, LanguageProvider languageProvider)
    {
        _module = module;
        _languageProvider = languageProvider;
    }

    [Command("create"), RequireUserPermissions(Permissions.Administrator), RequireGuild]
    public async Task CreateTicketPanel(CommandContext ctx)
    {
        var interactivity = ctx.Client.GetInteractivity();
        TicketPanelType ticketPanelType;

        #region Selecting Type
        {
            var interactivityTicketTypeResult = await interactivity.WaitForButtonAsync(
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Create Ticket Panel")
                        .Transparent()
                        .WithDescription("Please select a ticket type"))
                    .AddComponents(
                        new DiscordButtonComponent(ButtonStyle.Secondary, "button", "Button"),
                        new DiscordButtonComponent(ButtonStyle.Secondary, "selector", "Selector")
                    )), ctx.User, TimeSpan.FromSeconds(30));
            if (interactivityTicketTypeResult.TimedOut)
            {
                await ctx.Channel.SendMessageAsync("Timed out");
                return;
            }

            ticketPanelType = interactivityTicketTypeResult.Result.Id == "button" ? TicketPanelType.Button : TicketPanelType.Select;
            await interactivityTicketTypeResult.Ack();
        }
        #endregion

        DiscordChannel channel;
        
        #region Selecting Channel
        {
            await ctx.Channel.SendMessageAsync("Ping the channel you want to create the ticket panel in");
            var interactivityChannelResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
            
            if (interactivityChannelResult.TimedOut)
            {
                await ctx.Channel.SendMessageAsync("Timed out");
                return;
            }
            
            if (!interactivityChannelResult.Result.MentionedChannels.Any())
            {
                await ctx.Channel.SendMessageAsync("Cancelled: Please ping a channel");
                return;
            }
            
            channel = interactivityChannelResult.Result.MentionedChannels.First();
            
            if (channel.Type != ChannelType.Text)
            {
                await ctx.Channel.SendMessageAsync("Cancelled: Please ping a text channel");
                return;
            }
            
        }
        #endregion
        
        if (ticketPanelType == TicketPanelType.Button)
        {
            var discordEmbedBuilder = new DiscordEmbedBuilder();
            
            #region Embed Building
            {
                await ctx.Channel.SendMessageAsync("Enter the embed title");
                var interactivityTitleResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityTitleResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                discordEmbedBuilder.WithTitle(interactivityTitleResult.Result.Content);
                
            }

            {
                await ctx.Channel.SendMessageAsync("Enter the embed description");
                var interactivityDescriptionResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityDescriptionResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                discordEmbedBuilder.WithDescription(interactivityDescriptionResult.Result.Content);
                
            }

            {
                await ctx.Channel.SendMessageAsync("Enter the embed color in hexadecimal format.\nExample: #FFFFFF (white)\nEnter `transparent` for no color");
                var interactivityColorResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityColorResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (interactivityColorResult.Result.Content.ToLower() == "transparent")
                {
                    discordEmbedBuilder.Transparent();
                }
                else
                {
                    discordEmbedBuilder.WithColor(new DiscordColor(interactivityColorResult.Result.Content));
                }
            }
            #endregion

            ButtonStyle style;
            
            #region Button Style

            {
                await ctx.Channel.SendMessageAsync("Enter the button style.\n`primary` `secondary` `succeess` `danger`");    
                
                var interactivityButtonStyleResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityButtonStyleResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (interactivityButtonStyleResult.Result.Content.ToLower() == "primary")
                {
                    style = ButtonStyle.Primary;
                }
                else if (interactivityButtonStyleResult.Result.Content.ToLower() == "secondary")
                {
                    style = ButtonStyle.Secondary;
                }
                else if (interactivityButtonStyleResult.Result.Content.ToLower() == "success")
                {
                    style = ButtonStyle.Success;
                }
                else if (interactivityButtonStyleResult.Result.Content.ToLower() == "danger")
                {
                    style = ButtonStyle.Danger;
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Invalid button style");
                    return;
                }
                
            }
            
            #endregion

            string? buttonText;

            #region Button Text

            {
                await ctx.Channel.SendMessageAsync("Enter the button text\nEnter `none` for no text.");
                
                var interactivityButtonTextResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityButtonTextResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (interactivityButtonTextResult.Result.Content.ToLower() == "none")
                {
                    buttonText = null;
                }
                else
                {
                    buttonText = interactivityButtonTextResult.Result.Content;
                }
            }

            #endregion

            DiscordEmoji? buttonEmoji;
            
            #region Button Emoji

            {

                if (buttonText is null)
                {
                    await ctx.Channel.SendMessageAsync("Enter the button emoji\n**REQUIRED**");
                }else
                {
                    await ctx.Channel.SendMessageAsync("Enter the button emoji\nEnter `none` for no emoji.");
                }
                
                var interactivityButtonEmojiResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityButtonEmojiResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (interactivityButtonEmojiResult.Result.Content.ToLower() == "none")
                {
                    if (buttonText is null)
                    {
                        await ctx.Channel.SendMessageAsync("Button emoji required.");
                        return;
                    }
                    
                    buttonEmoji = null;
                }
                else
                {
                    if (DiscordEmoji.TryFromUnicode(ctx.Client, interactivityButtonEmojiResult.Result.Content, out var emoji))
                    {
                        buttonEmoji = emoji;
                    }
                    else if (DiscordEmoji.TryFromName(ctx.Client, interactivityButtonEmojiResult.Result.Content, out emoji))
                    {
                        buttonEmoji = emoji;
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync("Invalid button emoji");
                        return;
                    }
                    
                }
            }

            #endregion

            DiscordChannel? category, closedCategory, archivedCategory;
            
            #region Category Select

            {
                await ctx.Channel.SendMessageAsync("Select the category for the ticket (`<#id>`)\n**REQUIRED**\nYou can also specify `closed` and `archived categories` by pinging them in this order:\n" +
                                                   "Open Category - Closed Category (optional) - Archived Category (optional)");
                
                var interactivityCategoryResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityCategoryResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (interactivityCategoryResult.Result.MentionedChannels.Count == 0)
                {
                    await ctx.Channel.SendMessageAsync("Invalid category");
                    return;
                }

                foreach (var mentionedChannel in interactivityCategoryResult.Result.MentionedChannels)
                {
                    if (mentionedChannel.Type != ChannelType.Category)
                    {
                        await ctx.Channel.SendMessageAsync($"{mentionedChannel.Mention} is not a category");
                        return;
                    }
                }
                
                if (interactivityCategoryResult.Result.MentionedChannels.Count == 1)
                {
                    category = interactivityCategoryResult.Result.MentionedChannels.First();
                    closedCategory = null;
                    archivedCategory = null;
                }
                else if (interactivityCategoryResult.Result.MentionedChannels.Count == 2)
                {
                    category = interactivityCategoryResult.Result.MentionedChannels.First();
                    closedCategory = interactivityCategoryResult.Result.MentionedChannels.Last();
                    archivedCategory = null;
                }
                else
                {
                    category = interactivityCategoryResult.Result.MentionedChannels.First();
                    closedCategory = interactivityCategoryResult.Result.MentionedChannels.ElementAt(1);
                    archivedCategory = interactivityCategoryResult.Result.MentionedChannels.Last();
                }
                
            }
            
            #endregion
            DiscordRole[] supportRoles;
            
            #region Role Select

            {
                await ctx.Channel.SendMessageAsync("Ping all the roles that will have access to the ticket");
                
                var interactivityRoleResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityRoleResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (interactivityRoleResult.Result.MentionedRoles.Count == 0)
                {
                    await ctx.Channel.SendMessageAsync("Invalid role");
                    return;
                }
                
                foreach (var mentionedRole in interactivityRoleResult.Result.MentionedRoles)
                {
                    if (ctx.Guild.EveryoneRole == mentionedRole)
                    {
                        await ctx.Channel.SendMessageAsync($"{mentionedRole.Mention} is an everyone role");
                        return;
                    }
                }

                supportRoles = interactivityRoleResult.Result.MentionedRoles.ToArray();
            }

            #endregion
            
            int ticketsPerUser;
            #region Tickets Per User

            {
                await ctx.Channel.SendMessageAsync("How many tickets can each user open?\n" +
                                                   "0 = Unlimited");
                
                var interactivityTicketsResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityTicketsResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (!int.TryParse(interactivityTicketsResult.Result.Content, out ticketsPerUser))
                {
                    await ctx.Channel.SendMessageAsync("Invalid number");
                    return;
                }
            }
            
            #endregion
            
            string namingFormat;

            #region Name Format

            {
                await ctx.Channel.SendMessageAsync("How will the ticket channels be named?\n" +
                                                   "{ticket} = The ticket number\n" +
                                                   "{user} = The user who opened the ticket\n" +
                                                   $"Example: `{{ticket}}-{{user}}` would be `1-{ctx.User.GetName()}`");
                
                var interactivityNameResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityNameResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                namingFormat = interactivityNameResult.Result.Content;
            }

            #endregion

            string cpTitle;

            #region Control Panel Title

            {
                await ctx.Channel.SendMessageAsync("Control Panel Title");
                
                var interactivityTitleResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityTitleResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                cpTitle = interactivityTitleResult.Result.Content;
            }

            #endregion
            
            string cpDescription;

            #region Control Panel Description

            {
                await ctx.Channel.SendMessageAsync("Control Panel Description");
                
                var interactivityDescriptionResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                
                if (interactivityDescriptionResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                cpDescription = interactivityDescriptionResult.Result.Content;
            }

            #endregion
            
            string cpColor;

            #region Control Panel Color

            {
                await ctx.Channel.SendMessageAsync("Control Panel Color in hexadecimal\n`transparent` for no color");
                
                var interactivityColorResult = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                if (interactivityColorResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Timed out");
                    return;
                }
                
                if (interactivityColorResult.Result.Content.ToLower() == "transparent")
                {
                    cpColor = "#2F3136";
                }
                else
                {
                    if (!interactivityColorResult.Result.Content.StartsWith("#"))
                    {
                        await ctx.Channel.SendMessageAsync("Invalid color");
                        return;
                    }
                    
                    cpColor = interactivityColorResult.Result.Content;
                }
            }

            #endregion
            
            var message = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(discordEmbedBuilder)
                .AddComponents(new DiscordButtonComponent(style, "0", buttonText, false,
                    buttonEmoji != null ? new DiscordComponentEmoji(buttonEmoji) : null)));

            await _module.CreateButtonTicketPanel(message, category, closedCategory, archivedCategory, supportRoles, ticketsPerUser, namingFormat, cpTitle, cpDescription, cpColor);

        }
        
    }
}