using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Backups.Entities;

namespace Friday.Modules.Backups.Services;

public class BackupService
{
    private BackupsModule _module;
    internal BackupService(BackupsModule module)
    {
        this._module = module;   
    }

    public async Task<(long id, string code, Backup backup)?> CreateBackupAsync(DiscordGuild guild, DiscordUser owner)
    {
        var code = new char[8].RandomAlphanumeric();

        while (await _module.Database.GetBackupAsync(code) is not null)
        {
            code = new char[8].RandomAlphanumeric();
        }

        var backup = new Backup();
        await backup.From(guild);
        
        var id = DateTime.UtcNow.Ticks;
        await _module.Database.InsertBackupAsync(id, backup, code, owner.Id);
        
        return (id, code, backup);
    }
}