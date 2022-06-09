using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.Birthday.Services;

namespace Friday.Modules.Birthday;

public class BirthdayModule : ModuleBase
{
    internal DatabaseService DatabaseService { get; }

    public BirthdayModule(DatabaseProvider provider)
    {
        this.DatabaseService = new DatabaseService(provider);
    }

    public override Task OnLoad()
    {
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }

    public Task<bool> DoesUserHaveBirthdayAsync(ulong userId)
    {
        return this.DatabaseService.HasBirthdayAsync(userId);
    }

    public Task<bool> DoesUserHaveBirthdayAsync(DiscordUser user)
    {
        return this.DatabaseService.HasBirthdayAsync(user.Id);
    }

    public Task<bool> DoesUserHaveBirthdayAsync(DiscordMember member)
    {
        return this.DatabaseService.HasBirthdayAsync(member.Id);
    }

    public async Task SetBirthdayAsync(Entities.Birthday birthday)
    {
        if (await DoesUserHaveBirthdayAsync(birthday.Id))
        {
            await this.DatabaseService.UpdateBirthdayAsync(birthday);
        }else
        {
            await this.DatabaseService.InsertBirthdayAsync(birthday);
        }
    }
    
    public async Task<Entities.Birthday?> GetBirthdayAsync(ulong userId)
    {
        if (!(await DoesUserHaveBirthdayAsync(userId)))
        {
            return null;
        }
        return await this.DatabaseService.GetBirthdayAsync(userId);
    }
}