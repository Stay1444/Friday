using Friday.Common.Entities;
using Friday.Common.Services;
using Friday.Modules.MiniGames.Services;

namespace Friday.Modules.MiniGames.Commands;

public partial class Commands : FridayCommandModule
{
    private MiniGamesModule _module;
    private LanguageProvider _languageProvider;
    private HangmanWordList _hangmanWordList;
    public Commands(MiniGamesModule module, LanguageProvider languageProvider)
    {
        _module = module;
        _languageProvider = languageProvider;
        _hangmanWordList = new HangmanWordList();
    }
}