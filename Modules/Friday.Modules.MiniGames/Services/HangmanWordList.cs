using Friday.Common.Entities;

namespace Friday.Modules.MiniGames.Services;

public class HangmanWordList
{
    private List<Resource> _wordsLists;

    public HangmanWordList()
    {
        _wordsLists = Resource.LoadDirectory("Resources/Hangman").ToList();
    }
    
    public bool DoesLanguageExist(string language)
    {
        foreach (var wordsList in _wordsLists)
        {
            if (Path.GetFileNameWithoutExtension(wordsList.FileName) == language)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public async Task<string> GetRandomWord(string language)
    {
        var wordsList = _wordsLists.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.FileName) == language);
        if (wordsList == null)
        {
            throw new Exception("Language not found");
        }

        var words = await wordsList.ReadStringAsync();
        var wordsArray = words.Split('\n');
        var randomIndex = new Random().Next(0, wordsArray.Length);
        return wordsArray[randomIndex];
    }
}