
namespace Friday.Modules.AntiRaid.Entities;

public class GuildAntiRaid
{
    private AntiRaidModule _antiRaidModule;
    public ulong GuildId { get; private set; }
    public AntiRaidSettings? Settings { get; private set; }
    internal GuildAntiRaid(AntiRaidModule module, ulong guildId)
    {
        GuildId = guildId;
        _antiRaidModule = module;
    }

    internal async Task LoadAsync()
    {
        var db = _antiRaidModule.AntiRaidDatabase;
        if (await db.SettingsExistsForGuild(GuildId))
        {
            Settings = await db.GetSettingsForGuild(GuildId);
        }else
        {
            Settings = new AntiRaidSettings();
            await db.InsertSettingsForGuild(GuildId, Settings);
        }
    }

    public async Task SaveSettingsAsync()
    {
        var db = _antiRaidModule.AntiRaidDatabase;
        await db.UpdateSettingsForGuild(GuildId, Settings!);
    }
}