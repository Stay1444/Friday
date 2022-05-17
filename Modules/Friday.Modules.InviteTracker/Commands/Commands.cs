using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Attributes;
using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Modules.InviteTracker.Services;

namespace Friday.Modules.InviteTracker.Commands;
[FridayRequirePermission(Permissions.Administrator), Group("invitetracker")]

public partial class Commands : FridayCommandModule
{
    private InviteTrackerModule _module;
    private LanguageProvider _languageProvider;

    public Commands(InviteTrackerModule module, LanguageProvider languageProvider)
    {
        _module = module;
        _languageProvider = languageProvider;
    }
}