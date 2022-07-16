using DSharpPlus;
using DSharpPlus.Entities;

namespace Friday.Common;

public static class DiscordEmojiUtils
{
    public static DiscordEmoji? FromGeneric(string generic, DiscordClient client)
    {
        if (DiscordEmoji.TryFromUnicode(generic, out var unicode))
        {
            return unicode;
        }

        if (DiscordEmoji.TryFromName(client, generic, out var emoji))
        {
            return emoji;
        }

        try
        {
            var name = generic.Substring(2, generic.IndexOf(':', 2) - 2);
            var id = ulong.Parse(generic.Substring(generic.IndexOf(':', 2) + 1,
                generic.Length - generic.IndexOf(':', 2) - 2));
            if (DiscordEmoji.TryFromGuildEmote(client, id, out var guildEmoji))
            {
                return guildEmoji;
            }
        }catch { /* ignored */ }

        return null;
    }
}