using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Services;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.Misc.Commands;

public partial class Commands
{
    [Command("settings"), Aliases("config", "cfg")]
    public async Task cmd_SettingsCommand(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder
        {
            Duration = TimeSpan.FromMinutes(5)
        };

        uiBuilder.OnRender(x =>
        {
            x.Embed.Title = "Settings";
            x.Embed.Description = "Manage Friday Settings on a Personal & Guild Level";
            x.Embed.Transparent();
            x.AddButton(guild =>
            {
                guild.Style = ButtonStyle.Primary;
                guild.Label = "Guild";
                guild.Disabled = ctx.Member is null;

                if (ctx.Member is not null && !ctx.Member.Permissions.HasPermission(Permissions.Administrator))
                {
                    guild.Disabled = true;
                }
                
                guild.OnClick(() => x.SubPage = "guild");
            });

            x.AddButton(personal =>
            {
                personal.Style = ButtonStyle.Primary;
                personal.Label = "Personal";
                
                personal.OnClick(() => x.SubPage = "personal");
            });
            
            x.AddSubPageAsync("personal", async personal =>
            {
                var userSettings = x.GetState("userSettings",
                    await _userConfigurationProvider.GetConfiguration(ctx.User.Id));

                personal.Embed.Title = "Settings - Personal";
                personal.Embed.Transparent();
                personal.Embed.WithDescription("Manage your Personal Settings");


                personal.Embed.AddField("Language Override", userSettings.Value.LanguageOverride == null
                    ? "None"
                    : $"{LanguageProvider.LanguageList[userSettings.Value.LanguageOverride].emote} {LanguageProvider.LanguageList[userSettings.Value.LanguageOverride].name}", true);

                personal.Embed.AddField("Prefix Override", userSettings.Value.PrefixOverride ?? "None", true);
                personal.AddButton(back =>
                {
                    back.Style = ButtonStyle.Secondary;
                    back.Label = "Back";
                    back.OnClick(() => x.SubPage = null);
                });

                personal.AddButton(lang =>
                {
                    lang.Style = ButtonStyle.Primary;
                    
                    lang.Label = userSettings.Value.LanguageOverride == null ? "Language" :
                        LanguageProvider.LanguageList[userSettings.Value.LanguageOverride].name;
                    lang.Emoji = userSettings.Value.LanguageOverride == null
                        ? null
                        : DiscordEmoji.FromName(ctx.Client,
                            LanguageProvider.LanguageList[userSettings.Value.LanguageOverride].emote);
                    
                    lang.OnClick(() => personal.SubPage = "lang");
                });

                personal.AddButton(prefix =>
                {
                    prefix.Style = ButtonStyle.Primary;
                    prefix.OnClick(() => personal.SubPage = "prefix");
                    prefix.Label = "Prefix";
                });
                
                personal.AddSubPage("lang", languagePage =>
                {
                    languagePage.Embed.Transparent();
                    languagePage.Embed.Title = "Personal - Language Override";
                    languagePage.Embed.WithDescription(
                        "This setting allows you to set your custom language preferences, bypassing the Guild configuration.");
                    
                    languagePage.Embed.AddField("Language Override", userSettings.Value.LanguageOverride == null
                        ? "None"
                        : $"{LanguageProvider.LanguageList[userSettings.Value.LanguageOverride].emote} {LanguageProvider.LanguageList[userSettings.Value.LanguageOverride].name}", true);
                    
                    languagePage.AddButton(back =>
                    {
                        back.Style = ButtonStyle.Secondary;
                        back.Label = "Back";
                        back.OnClick(() => personal.SubPage = null);
                    });

                    languagePage.AddButton(clear =>
                    {
                        clear.Style = ButtonStyle.Primary;
                        clear.Label = "Clear";
                        clear.Disabled = userSettings.Value.LanguageOverride is null;

                        clear.OnClick(async () =>
                        {
                            userSettings.Value.LanguageOverride = null;
                            await _userConfigurationProvider.SaveConfiguration(ctx.User.Id, userSettings.Value);
                        });
                    });
                    
                    languagePage.NewLine();

                    foreach (var lang in LanguageProvider.LanguageList)
                    {
                        languagePage.AddButton(l =>
                        {
                            l.Label = lang.Value.name;
                            l.Emoji = DiscordEmoji.FromName(ctx.Client, lang.Value.emote);
                            l.Style = userSettings.Value.LanguageOverride == lang.Key
                                ? ButtonStyle.Success
                                : ButtonStyle.Secondary;
                            l.Disabled = userSettings.Value.LanguageOverride == lang.Key;

                            l.OnClick(async () =>
                            {
                                userSettings.Value.LanguageOverride = lang.Key;
                                await _userConfigurationProvider.SaveConfiguration(ctx.User.Id, userSettings.Value);
                            });
                        });
                    }
                });
                
                personal.AddSubPage("prefix", prefix =>
                {
                    prefix.Embed.Transparent();
                    prefix.Embed.Title = "Personal - Prefix";
                    prefix.Embed.WithDescription(
                        "This setting allows you to set your custom prefix, bypassing the Guild configuration.");
                    
                    prefix.Embed.AddField("Prefix Override", userSettings.Value.PrefixOverride ?? "None");

                    prefix.AddButton(back =>
                    {
                        back.Style = ButtonStyle.Secondary;
                        back.Label = "Back";
                        back.OnClick(() => personal.SubPage = null);
                    });
                    
                    prefix.AddButton(clear =>
                    {
                        clear.Style = ButtonStyle.Primary;
                        clear.Label = "Clear";
                        clear.Disabled = userSettings.Value.PrefixOverride is null;

                        clear.OnClick(async () =>
                        {
                            userSettings.Value.PrefixOverride = null;
                            await _userConfigurationProvider.SaveConfiguration(ctx.User.Id, userSettings.Value);
                        });
                    });
                    
                    prefix.AddModal(change =>
                    {
                        change.ButtonStyle = ButtonStyle.Primary;
                        change.ButtonLabel = "Change";
                        change.Title = "Change Prefix";
                        change.AddField("prefix", field =>
                        {
                            field.Required = true;
                            field.Placeholder = "Your Prefix";
                            field.Value = userSettings.Value.PrefixOverride;
                            field.MinimumLength = 1;
                            field.MaximumLength = 4;
                        });
                        
                        change.OnSubmit(async result =>
                        {
                            if (string.IsNullOrEmpty(result["prefix"]) ||
                                string.IsNullOrWhiteSpace(result["prefix"]))
                            {
                                return;
                            }
                            
                            if (string.IsNullOrEmpty(result["prefix"].Trim()) ||
                                string.IsNullOrWhiteSpace(result["prefix"].Trim()))
                            {
                                return;
                            }
                            
                            userSettings.Value.PrefixOverride = result["prefix"].Replace(" ", "");
                            
                            await _userConfigurationProvider.SaveConfiguration(ctx.User.Id, userSettings.Value);
                            
                            x.ForceRender();
                        });
                    });
                });
            });
            
            x.AddSubPageAsync("guild", async guild =>
            {
                var guildSettings = x.GetState("guildSettings", await _guildConfigurationProvider.GetConfiguration(ctx.Guild.Id));
                
                guild.Embed.Transparent();
                guild.Embed.Title = "Settings - Guild";
                guild.Embed.WithDescription($"Manage {ctx.Guild.Name} Settings");
                
                guild.Embed.AddField("Language", $"{LanguageProvider.LanguageList[guildSettings.Value.Language].emote} {LanguageProvider.LanguageList[guildSettings.Value.Language].name}", true);
                guild.Embed.AddField("Prefix", guildSettings.Value.Prefix, true);

                guild.AddButton(back =>
                {
                    back.Style = ButtonStyle.Secondary;
                    back.Label = "Back";
                    back.OnClick(() => x.SubPage = null);
                });
                
                guild.AddButton(lang =>
                {
                    lang.Style = ButtonStyle.Primary;
                    lang.Label = LanguageProvider.LanguageList[guildSettings.Value.Language].name;
                    lang.Emoji = DiscordEmoji.FromName(ctx.Client, LanguageProvider.LanguageList[guildSettings.Value.Language].emote);
                    
                    lang.OnClick(() => guild.SubPage = "lang");
                });
                
                guild.AddButton(prefix =>
                {
                    prefix.Style = ButtonStyle.Primary;
                    prefix.OnClick(() => guild.SubPage = "prefix");
                    prefix.Label = "Prefix";
                });
                
                guild.AddSubPage("lang", languagePage =>
                {
                    languagePage.Embed.Transparent();
                    languagePage.Embed.Title = "Guild - Language";
                    languagePage.Embed.WithDescription(
                        "The language setting allows you to set the language for this server.");
                    
                    languagePage.Embed.AddField("Language", $"{LanguageProvider.LanguageList[guildSettings.Value.Language].emote} {LanguageProvider.LanguageList[guildSettings.Value.Language].name}", true);
                    
                    languagePage.AddButton(back =>
                    {
                        back.Style = ButtonStyle.Secondary;
                        back.Label = "Back";
                        back.OnClick(() => guild.SubPage = null);
                    });
                    
                    languagePage.NewLine();

                    foreach (var lang in LanguageProvider.LanguageList)
                    {
                        languagePage.AddButton(l =>
                        {
                            l.Label = lang.Value.name;
                            l.Emoji = DiscordEmoji.FromName(ctx.Client, lang.Value.emote);
                            l.Style = guildSettings.Value.Language == lang.Key
                                ? ButtonStyle.Success
                                : ButtonStyle.Secondary;
                            l.Disabled = guildSettings.Value.Language == lang.Key;

                            l.OnClick(async () =>
                            {
                                guildSettings.Value.Language = lang.Key;
                                await _guildConfigurationProvider.SaveConfiguration(ctx.Guild.Id, guildSettings.Value);
                            });
                        });
                    }
                });
                
                guild.AddSubPage("prefix", prefix =>
                {
                    prefix.Embed.Transparent();
                    prefix.Embed.Title = "Guild - Prefix";
                    prefix.Embed.WithDescription(
                        "This setting allows you to set the prefix for this server.");
                    
                    prefix.Embed.AddField("Prefix", guildSettings.Value.Prefix);

                    prefix.AddButton(back =>
                    {
                        back.Style = ButtonStyle.Secondary;
                        back.Label = "Back";
                        back.OnClick(() => guild.SubPage = null);
                    });
                    
                    prefix.AddModal(change =>
                    {
                        change.ButtonStyle = ButtonStyle.Primary;
                        change.ButtonLabel = "Change";
                        change.Title = "Change Prefix";
                        change.AddField("prefix", field =>
                        {
                            field.Required = true;
                            field.Placeholder = "New Prefix";
                            field.Value = guildSettings.Value.Prefix;
                            field.MinimumLength = 1;
                            field.MaximumLength = 4;
                        });
                        
                        change.OnSubmit(async result =>
                        {
                            if (string.IsNullOrEmpty(result["prefix"]) ||
                                string.IsNullOrWhiteSpace(result["prefix"]))
                            {
                                return;
                            }
                            
                            if (string.IsNullOrEmpty(result["prefix"].Trim()) ||
                                string.IsNullOrWhiteSpace(result["prefix"].Trim()))
                            {
                                return;
                            }
                            
                            guildSettings.Value.Prefix = result["prefix"].Replace(" ", "");

                            await _guildConfigurationProvider.SaveConfiguration(ctx.Guild.Id, guildSettings.Value);
                            
                            x.ForceRender();
                        });
                    });
                });
                
            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }
}