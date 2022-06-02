namespace Friday.Modules.MiniGames.Games;

public class HangmanGame
{
    public const int MAX_TRIES = 10;
    
    public string Word { get; }
    public char[] ConstructedWord { get; private set; }
    public List<char> LettersGuessed { get; private set; }
    public int Tries { get; private set; } = MAX_TRIES;
    
    public HangmanGame(string word)
    {
        this.Word = word.ToUpper();
        
        this.ConstructedWord = new char[word.Length];
        
        for (int i = 0; i < word.Length; i++)
        {
            this.ConstructedWord[i] = '_';
        }
        
        this.LettersGuessed = new List<char>();
    }
    
    public bool IsWordCompleted()
    {
        for (int i = 0; i < this.ConstructedWord.Length; i++)
        {
            if (this.ConstructedWord[i] == '_')
            {
                return false;
            }
        }

        return true;
    }
    
    
    public void Solve(char letter)
    {
        if (this.Tries <= 0)
        {
            return;
        }
        
        bool found = false;
        
        for (int i = 0; i < this.Word.Length; i++)
        {
            if (this.Word[i] == letter)
            {
                this.ConstructedWord[i] = letter;
                found = true;
            }
        }
        
        if (!found)
        {
            this.Tries--;
        }
        
        this.LettersGuessed.Add(letter);
    }
    
    public bool HasLost()
    {
        return this.Tries <= 0;
    }
    
}