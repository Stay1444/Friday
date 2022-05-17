using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Modules.AntiRaid.Attributes;

namespace Friday.Modules.AntiRaid.Commands;

[RequireAntiRaidPermission, Group("antiraid")]
public partial class Commands : FridayCommandModule
{
    private AntiRaidModule _module;
    private LanguageProvider _languageProvider;
    public Commands(AntiRaidModule module, LanguageProvider languageProvider)
    {
        _module = module;
        _languageProvider = languageProvider;
    }

}