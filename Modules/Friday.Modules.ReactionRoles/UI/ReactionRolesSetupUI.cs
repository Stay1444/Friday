using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.UI.Entities;
using Friday.UI.Extensions;

namespace Friday.Modules.ReactionRoles.UI;

internal static class ReactionRolesSetupUI
{

    private static Random _random = new Random();
    private static string[] _randomEmojis = new[]
    {
        ":upside_down:",
        ":neutral_face:",
        ":thumbsup:",
        ":sunglasses:",
        ":partying_face:",
        ":grinning:"
    };
    
    public static async Task ReactionRolesSetup(this FridayUIPage x, CommandContext ctx)
    {
        x.Embed.Transparent();
        x.Embed.WithTitle("Reaction Roles - Setup");
        x.NewLine();

        var selectedType = x.GetState("selectedRRoleType", -1);
        if (selectedType.Value == -1)
        {
            x.Embed.WithDescription("Select a Reaction Role type");
        }
        else
        {
            x.Embed.AddField("Selected", selectedType.Value == 0 ? $"{_randomEmojis[_random.Next(0, _randomEmojis.Length)]} Emoji Reaction" : ":mouse_three_button: Button");
        }

        x.AddSelect(typeSelect =>
        {
            typeSelect.Placeholder = "Select a Reaction Role type";
            
            typeSelect.AddOption(emoji =>
            {
                var randomEmoji = _randomEmojis[_random.Next(0, _randomEmojis.Length)];
                emoji.Emoji = DiscordEmoji.FromName(ctx.Client, randomEmoji);
                emoji.Label = "Emoji Reaction";
                emoji.Value = "0";
                emoji.Description = "Compatible with any message";
                emoji.IsDefault = selectedType.Value.ToString() == "0";
            });

            typeSelect.AddOption(button =>
            {
                //button.Emoji = DiscordEmoji.FromName(ctx.Client, ":mouse_three_button:");
                //button.Label = "Button";
                //button.Value = "1";
                //button.Description = "Only compatible with Friday embeds";
                //button.IsDefault = selectedType.Value.ToString() == "1";
                
                button.Emoji = DiscordEmoji.FromName(ctx.Client, ":hourglass:");
                button.Label = "Coming Soon";
                button.Value = "-1";
                //button.Description = "Only compatible with Friday embeds";
                //button.IsDefault = selectedType.Value.ToString() == "1";
            });
            
            typeSelect.OnSelect(result =>
            {
                if (result.IsEmpty()) return;

                if (!int.TryParse(result[0], out var r)) return;

                selectedType.Value = r;
            });
        });
        x.NewLine();
        x.AddButton(cont =>
        {
            cont.Label = "Continue";
            cont.Disabled = selectedType.Value == -1;
            cont.Style = ButtonStyle.Primary;
        });
    }
}