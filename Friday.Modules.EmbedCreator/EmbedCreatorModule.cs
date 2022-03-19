using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Services;
using Serilog;

namespace Friday.Modules.EmbedCreator;

public class EmbedCreatorModule : IModule
{
    private readonly LanguageProvider _languageProvider;
    private readonly DiscordShardedClient  _client;
    public EmbedCreatorModule(LanguageProvider languageProvider, DiscordShardedClient client)
    {
        _languageProvider = languageProvider;
        _client = client;
    }

    public Task OnLoad()
    {
        Log.Information("[EmbedCreator] Module loaded");
        return Task.CompletedTask;
    }

    public async Task<DiscordEmbedBuilder?> ExecuteEmbedCreatorFor(DiscordMember member, DiscordChannel channel)
    {
        var embedCreator = new EmbedCreator(await _languageProvider.GetDesiredLanguage(member),
            member, channel, _languageProvider, _client.GetClient(channel.Guild));
        return await embedCreator.RunAsync();
    }
}