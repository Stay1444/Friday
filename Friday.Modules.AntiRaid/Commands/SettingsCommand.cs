using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.UI;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.AntiRaid.Commands;

public partial class Commands
{
    [Command("antiraid")]
    [Description("AntiRaid settings")]
    public async Task AntiRaidCommand(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder().OnRender(x =>
        {
            x.OnCancelledAsync(async (_, message) =>
            {
                await message.ModifyAsync(new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithDescription("AntiRaid settings saved!")));
            });
            
            x.Embed.Title = "AntiRaid Settings";
            x.Embed.Transparent();
            
            x.AddButton(button =>
            {
                button.Label = "Nothing";
            });
            
            x.AddSubPage("channel-settings",channelSettings =>
            {
                channelSettings.Embed.Title = "AntiRaid - Channel Settings";
                channelSettings.AddButton(button =>
                {
                    button.Label = "Back";
                    button.Style = ButtonStyle.Secondary;
                    
                    button.OnClick(() =>
                    {
                        x.SubPage = null;
                    });
                });
                
                channelSettings.AddSubPage("voice-settings", voiceSettings =>
                {
                    voiceSettings.Embed.Title = "AntiRaid - Voice Settings";
                    
                    voiceSettings.AddButton(button =>
                    {
                        button.Label = "Back";
                        button.Style = ButtonStyle.Secondary;
                        
                        button.OnClick(() =>
                        {
                            channelSettings.SubPage = null;
                        });
                    }); 
                });
                
                channelSettings.AddButton(button =>
                {
                    button.Label = "Voice Settings";
                    button.Style = ButtonStyle.Primary;
                    
                    button.OnClick(() =>
                    {
                        channelSettings.SubPage = "voice-settings";
                    });
                });
                
            });
                
            x.AddButton(button => 
            {
                button.Label = "Channel Settings"; 
                button.Style = ButtonStyle.Primary;
                button.OnClick(() => 
                { 
                    x.SubPage = "channel-settings";
                });
            });
        });

        await ctx.SendUIAsync(uiBuilder);
    }
}