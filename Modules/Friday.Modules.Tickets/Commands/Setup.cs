using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Attributes;
using Friday.Modules.Tickets.Enums;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.Tickets.Commands;

[Group("ticket"), RequireGuild]
public partial class Commands
{
    private TicketModuleBase _module;
    public Commands(TicketModuleBase module)
    {
        _module = module;
    }

    [Command("setup"), FridayRequirePermission(Permissions.Administrator), RequireGuild]
    public async Task CreateTicketPanel(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder();

        uiBuilder.OnRender(x =>
        {
            var ticketPanelType = x.GetState("ticketPanelType", TicketPanelType.Button);
            var ticketPanelChannel = x.GetState<DiscordChannel?>("ticketPanelChannel", null);
            var ticketPanelEmbedBuilder = x.GetState("ticketPanelEmbedBuilder", new DiscordEmbedBuilder().WithTitle("Ticket Panel Embed").WithDescription("This is the embed of your ticket panel. You can modify this embed using the button below. Usually this contains something like `To get support press the button below`").Transparent());
            var ticketCategory = x.GetState<DiscordChannel?>("ticketCategory", null);
            var ticketControlPanelEmbedBuilder = x.GetState("ticketControlPanelEmbedBuilder", new DiscordEmbedBuilder().WithTitle("Ticket Control Panel").WithDescription("This is the embed that will be created at the start of every ticket. It has `close` and `add participant` buttons.").Transparent());
            var supportRoles = x.GetState("ticketSupportRoles", new List<DiscordRole>());
            x.Embed.Transparent();
            x.Embed.Title = "Ticket Panel Setup";
            x.Embed.Description = "Setup a new ticket panel for your server.";

            x.AddButton(setup =>
            {
                setup.Label = "Setup";
                setup.OnClick(() =>
                {
                    x.SubPage = "2";
                });
            });
            /*
            x.AddSubPage("1", typePage =>
            {
                typePage.Embed.Transparent();
                typePage.Embed.Title = "Ticket Panel Setup";
                typePage.Embed.Description = "Select a ticket panel type.";

                typePage.AddSelect(select =>
                {
                    select.Placeholder = "Select a ticket panel type";

                    foreach (var name in Enum.GetNames<TicketPanelType>())
                    {
                        select.AddOption(option =>
                        {
                            option.Label = name;
                            option.Value = name;
                        });
                    }
                    
                    select.OnSelect(selection =>
                    {
                        ticketPanelType.Value = (TicketPanelType) Enum.Parse(typeof(TicketPanelType), selection[0]);
                        x.SubPage = "2";
                    });
                });
            });
            */
            x.AddSubPage("2", channelPage =>
            {
                channelPage.Embed.Transparent();
                channelPage.Embed.Title = "Ticket Panel Setup";
                channelPage.Embed.Description = "Select the channel to use for the ticket panel.";

                channelPage.AddSelect(select =>
                {
                    select.Placeholder = "Select a channel";

                    foreach (var channel in ctx.Guild.Channels.Values.Where(c => c.Type == ChannelType.Text))
                    {
                        select.AddOption(option =>
                        {
                            option.Label = channel.Name;
                            option.Value = channel.Id.ToString();
                            option.Description = channel.Parent?.Name ?? null;
                        });
                    }

                    select.OnSelect(selection =>
                    {
                        ticketPanelChannel.Value = ctx.Guild.GetChannel(ulong.Parse(selection[0]));
                        x.SubPage = "2.5";
                    });
                });
            });
            
            x.AddSubPage("2.5", channelPage =>
            {
                channelPage.Embed.Transparent();
                channelPage.Embed.Title = "Ticket Panel Setup";
                channelPage.Embed.Description = "Select the category to use for the tickets.";

                channelPage.AddSelect(select =>
                {
                    select.Placeholder = "Select a category";

                    foreach (var channel in ctx.Guild.Channels.Values.Where(c => c.Type == ChannelType.Category))
                    {
                        select.AddOption(option =>
                        {
                            option.Label = channel.Name;
                            option.Value = channel.Id.ToString();
                        });
                    }

                    select.OnSelect(selection =>
                    {
                        ticketCategory.Value = ctx.Guild.GetChannel(ulong.Parse(selection[0]));
                        x.SubPage = "2.6";
                    });
                });
            });
            
            x.AddSubPage("2.6", rolePage =>
            {
                rolePage.Embed.Transparent();
                rolePage.Embed.Title = "Ticket Panel Setup";
                rolePage.Embed.Description = "Select the roles that can access the tickets.";
                
                rolePage.AddSelect(select =>
                {
                    select.Placeholder = "Select a role";
                    select.MaxOptions = Math.Min(25, ctx.Guild.Roles.Count);
                    select.MinOptions = 1;
                    foreach (var role in ctx.Guild.Roles.Values)
                    {
                        select.AddOption(option =>
                        {
                            option.Label = role.Name;
                            option.Value = role.Id.ToString();
                        });
                    }

                    select.OnSelect(selection =>
                    {
                        x.SubPage = "3";
                        
                        foreach (var roleId in selection)
                        {
                            var role = ctx.Guild.GetRole(ulong.Parse(roleId));
                            if (role == null) continue;
                            supportRoles.Value.Add(role);
                        }
                    });
                });
            });

            if (ticketPanelType.Value == TicketPanelType.Button)
            {
                var ticketPanelButtonBuilder = x.GetState<(ButtonStyle style, string? text, string ? emoji)>("ticketPanelButtonBuilder", (ButtonStyle.Primary, "Placeholder", null));

                x.AddSubPage("3", embedPage =>
                {
                    embedPage.Message.WithEmbed(ticketPanelEmbedBuilder.Value);
                     
                    embedPage.AddModal(edit =>
                    {
                        edit.ButtonEmoji = DiscordEmoji.FromName(ctx.Client, ":pencil2:");
                        edit.ButtonLabel = "Edit";
                        edit.ButtonStyle = ButtonStyle.Primary;
                        
                        edit.Title = "Edit Ticket Panel Embed";
                        
                        edit.AddField("title", title =>
                        {
                            title.Placeholder = "Title";
                            title.Value = ticketPanelEmbedBuilder.Value.Title;
                            title.Title = "Title";
                            title.MaximumLength = 100;
                        });
                        
                        edit.AddField("description", description =>
                        {
                            description.Placeholder = "Description";
                            description.Value = ticketPanelEmbedBuilder.Value.Description;
                            description.Style = TextInputStyle.Paragraph;
                            description.MaximumLength = 1024;
                            description.Title = "Description";
                        });
                        
                        edit.AddField("color", color =>
                        {
                            color.Placeholder = "Color (hex without #)";
                            color.Value = ticketPanelEmbedBuilder.Value.Color.HasValue ? 
                                $"{ticketPanelEmbedBuilder.Value.Color.Value.R:X2}{ticketPanelEmbedBuilder.Value.Color.Value.G:X2}{ticketPanelEmbedBuilder.Value.Color.Value.B:X2}" : null;
                            color.Title = "Color";
                            color.MaximumLength = 6;
                        });
                        
                        edit.AddField("footer", footer =>
                        {
                            footer.Placeholder = "Footer";
                            footer.Value = ticketPanelEmbedBuilder.Value.Footer?.Text;
                            footer.MaximumLength = 50;
                            footer.Title = "Footer";
                        });

                        edit.AddField("footer-icon", footerIcon =>
                        {
                            footerIcon.Placeholder = "Footer Icon URL";
                            footerIcon.Value = ticketPanelEmbedBuilder.Value.Footer?.IconUrl;
                            footerIcon.Title = "Footer Icon";
                            footerIcon.MaximumLength = 1024;
                        });
                        
                        edit.OnSubmit(fields =>
                        {
                            ticketPanelEmbedBuilder.Value.Title = string.IsNullOrEmpty(fields["title"]) ? null : fields["title"];
                            
                            ticketPanelEmbedBuilder.Value.Description = string.IsNullOrEmpty(fields["description"]) ? null : fields["description"];
                            
                            if (string.IsNullOrEmpty(fields["color"]))
                            {
                                ticketPanelEmbedBuilder.Value.Transparent();
                            }
                            else
                            {
                                ticketPanelEmbedBuilder.Value.Color = new DiscordColor(fields["color"]);
                            }
                            
                            if (string.IsNullOrEmpty(fields["footer"]))
                            {
                                ticketPanelEmbedBuilder.Value.Footer = null;
                            }
                            else
                            {
                                ticketPanelEmbedBuilder.Value.Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = fields["footer"],
                                    IconUrl = string.IsNullOrEmpty(fields["footer-icon"]) ? null : fields["footer-icon"]
                                };
                            }

                            if (string.IsNullOrEmpty(ticketPanelEmbedBuilder.Value.Title) && string.IsNullOrEmpty(ticketPanelEmbedBuilder.Value.Description))
                            {
                                ticketPanelEmbedBuilder.Value.Title = "Placeholder";
                            }
                            
                            x.ForceRender();
                        });
                    });
                    embedPage.AddButton(submit =>
                    {
                        submit.Label = "Submit";
                        submit.OnClick(() =>
                        {
                            x.SubPage = "4";
                        });
                    });
                });
                
                x.AddSubPage("4", buttonPage =>
                {
                    buttonPage.Embed.Transparent();
                    buttonPage.Embed.Title = "Ticket Panel Setup";
                    buttonPage.Embed.Description = "Modify the button for the ticket panel to your liking.";

                    buttonPage.AddModal(edit =>
                    {
                        edit.ButtonLabel = "Edit";
                        
                        edit.Title = "Edit Ticket Panel Button";
                        
                        edit.AddField("label", label =>
                        {
                            label.MaximumLength = 50;
                            label.Placeholder = "Label";
                            label.Value = ticketPanelButtonBuilder.Value.text;
                        });

                        edit.AddField("emoji", emoji =>
                        {
                            emoji.Placeholder = "Emoji (:name: or id)";
                            emoji.Value = ticketPanelButtonBuilder.Value.emoji;
                        });
                        
                        edit.OnSubmit(fields =>
                        {
                            ticketPanelButtonBuilder.Value.text = string.IsNullOrEmpty(fields["label"]) ? null : fields["label"];

                            ticketPanelButtonBuilder.Value.emoji = string.IsNullOrEmpty(fields["emoji"]) ? null : fields["emoji"];
                            
                            x.ForceRender();
                        });
                    });
                    
                    buttonPage.AddButton(example =>
                    {
                        example.Disabled = true;

                        if (ticketPanelButtonBuilder.Value.text is null && ticketPanelButtonBuilder.Value.emoji is null)
                        {
                            example.Label = "Example";
                        }
                        else
                        {
                            example.Label = ticketPanelButtonBuilder.Value.text;

                            if (ticketPanelButtonBuilder.Value.emoji is not null)
                            {
                                if (ulong.TryParse(ticketPanelButtonBuilder.Value.emoji, out var id))
                                {
                                    example.Emoji = DiscordEmoji.FromGuildEmote(ctx.Client, id);
                                }else
                                {
                                    try
                                    {
                                        example.Emoji = DiscordEmoji.FromName(ctx.Client, ticketPanelButtonBuilder.Value.emoji);
                                    }catch
                                    {
                                        example.Emoji = null;
                                    }
                                }
                            }
                        }

                        example.Style = ticketPanelButtonBuilder.Value.style;
                    });
                    
                    buttonPage.AddButton(submit =>
                    {
                        submit.Label = "Submit";
                        submit.OnClick(() =>
                        {
                            x.SubPage = "5";
                        });
                    });
                    
                    buttonPage.NewLine();

                    buttonPage.AddSelect(styleSelect =>
                    {
                        styleSelect.Placeholder = "Style";

                        foreach (var name in Enum.GetNames<ButtonStyle>())
                        {
                            styleSelect.AddOption(style =>
                            {
                                style.Label = name;
                                style.Value = name;
                                style.IsDefault = name == ticketPanelButtonBuilder.Value.style.ToString();
                            });
                        }
                        
                        styleSelect.OnSelect(value =>
                        {
                            if (Enum.TryParse(value[0], out ButtonStyle style))
                            {
                                ticketPanelButtonBuilder.Value.style = style;
                            }
                        });
                    });
                });
                
                x.AddSubPage("5", embedPage =>
                {
                    embedPage.Message.WithEmbed(ticketControlPanelEmbedBuilder.Value);
                     
                    embedPage.AddModal(edit =>
                    {
                        edit.ButtonEmoji = DiscordEmoji.FromName(ctx.Client, ":pencil2:");
                        edit.ButtonLabel = "Edit";
                        edit.ButtonStyle = ButtonStyle.Primary;
                        
                        edit.Title = "Edit Ticket Control Panel Embed";
                        
                        edit.AddField("title", title =>
                        {
                            title.Placeholder = "Title";
                            title.Value = ticketControlPanelEmbedBuilder.Value.Title;
                            title.Title = "Title";
                            title.MaximumLength = 100;
                        });
                        
                        edit.AddField("description", description =>
                        {
                            description.Placeholder = "Description";
                            description.Value = ticketControlPanelEmbedBuilder.Value.Description;
                            description.Style = TextInputStyle.Paragraph;
                            description.MaximumLength = 1024;
                            description.Title = "Description";
                        });
                        
                        edit.AddField("color", color =>
                        {
                            color.Placeholder = "Color (hex without #)";
                            color.Value = ticketControlPanelEmbedBuilder.Value.Color.HasValue ? 
                                $"{ticketControlPanelEmbedBuilder.Value.Color.Value.R:X2}{ticketControlPanelEmbedBuilder.Value.Color.Value.G:X2}{ticketControlPanelEmbedBuilder.Value.Color.Value.B:X2}" : null;
                            color.Title = "Color";
                            color.MaximumLength = 6;
                        });
                        
                        edit.AddField("footer", footer =>
                        {
                            footer.Placeholder = "Footer";
                            footer.Value = ticketControlPanelEmbedBuilder.Value.Footer?.Text;
                            footer.MaximumLength = 50;
                            footer.Title = "Footer";
                        });

                        edit.AddField("footer-icon", footerIcon =>
                        {
                            footerIcon.Placeholder = "Footer Icon URL";
                            footerIcon.Value = ticketControlPanelEmbedBuilder.Value.Footer?.IconUrl;
                            footerIcon.Title = "Footer Icon";
                            footerIcon.MaximumLength = 1024;
                        });
                        
                        edit.OnSubmit(fields =>
                        {
                            ticketControlPanelEmbedBuilder.Value.Title = string.IsNullOrEmpty(fields["title"]) ? null : fields["title"];
                            
                            ticketControlPanelEmbedBuilder.Value.Description = string.IsNullOrEmpty(fields["description"]) ? null : fields["description"];
                            
                            if (string.IsNullOrEmpty(fields["color"]))
                            {
                                ticketControlPanelEmbedBuilder.Value.Transparent();
                            }
                            else
                            {
                                ticketControlPanelEmbedBuilder.Value.Color = new DiscordColor(fields["color"]);
                            }
                            
                            if (string.IsNullOrEmpty(fields["footer"]))
                            {
                                ticketControlPanelEmbedBuilder.Value.Footer = null;
                            }
                            else
                            {
                                ticketControlPanelEmbedBuilder.Value.Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = fields["footer"],
                                    IconUrl = string.IsNullOrEmpty(fields["footer-icon"]) ? null : fields["footer-icon"]
                                };
                            }

                            if (string.IsNullOrEmpty(ticketControlPanelEmbedBuilder.Value.Title) && string.IsNullOrEmpty(ticketControlPanelEmbedBuilder.Value.Description))
                            {
                                ticketControlPanelEmbedBuilder.Value.Title = "Placeholder";
                            }
                            
                            x.ForceRender();
                        });
                    });

                    embedPage.AddButton(submit =>
                    {
                        submit.Label = "Create";
                        submit.OnClick(async () =>
                        {
                            DiscordEmoji? emoji = null;

                            if (ticketPanelButtonBuilder.Value.emoji is not null)
                            {
                                if (ulong.TryParse(ticketPanelButtonBuilder.Value.emoji, out var id))
                                {
                                    emoji = DiscordEmoji.FromGuildEmote(ctx.Client, id);
                                }
                                else
                                {
                                    emoji = DiscordEmoji.FromName(ctx.Client, ticketPanelButtonBuilder.Value.emoji);
                                }
                            }
                            
                            var message = await ticketPanelChannel.Value!.SendMessageAsync(new DiscordMessageBuilder()
                                .WithEmbed(ticketPanelEmbedBuilder.Value)
                                .AddComponents(new DiscordButtonComponent(ticketPanelButtonBuilder.Value.style, "open",
                                    ticketPanelButtonBuilder.Value.text, false, emoji == null ? null : new DiscordComponentEmoji(emoji))));

                            await _module.CreateButtonTicketPanel(message, ticketCategory.Value!, null, null,
                                supportRoles.Value.ToArray(), 0, "ticket-{count}", ticketControlPanelEmbedBuilder.Value.Title,
                                ticketControlPanelEmbedBuilder.Value.Description,
                                ticketControlPanelEmbedBuilder.Value.Color.Value.ToHex());
                        });
                    });
                });
                
            }
        });
        
        uiBuilder.Duration = TimeSpan.FromMinutes(5);

        await ctx.SendUIAsync(uiBuilder);
    }
}