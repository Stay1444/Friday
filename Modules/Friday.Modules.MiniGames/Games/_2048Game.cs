namespace Friday.Modules.MiniGames.Games;

public class _2048Game
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    public const int BOARD_SIZE = 4;
    public const int MAX_VALUE = 2048;
    
    public int[,] Board { get; private set; }
    public int Score { get; private set; }
    
    private Random _random = new Random();
    
    public _2048Game()
    {
        Board = new int[BOARD_SIZE, BOARD_SIZE];
        Score = 0;
    }
    
    public void Reset()
    {
        Board = new int[BOARD_SIZE, BOARD_SIZE];
        Score = 0;
    }

    public bool IsGameOver()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (Board[i, j] == 0)
                {
                    return false;
                }
            }
        }
        
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (i > 0 && Board[i, j] == Board[i - 1, j])
                {
                    return false;
                }
                if (j > 0 && Board[i, j] == Board[i, j - 1])
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    public bool HasWon()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (Board[i, j] == MAX_VALUE)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private bool IsThereEmptySpace()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (Board[i, j] == 0)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private void AddRandomTile()
    {
        // Find an empty cell
        int x, y;
        do
        {
            x = _random.Next(0, BOARD_SIZE);
            y = _random.Next(0, BOARD_SIZE);
        } while (Board[x, y] != 0);
        
        // Add a random tile
        
        int value = _random.Next(1, 10) == 1 ? 4 : 2;
        
        Board[x, y] = value;
    }

    public void Start()
    {
        AddRandomTile();
        AddRandomTile();
    }
    
    public void Move(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                MoveLeft();
                break;
            case Direction.Down:
                MoveDown();
                break;
            case Direction.Right:
                MoveRight();
                break;
            case Direction.Up:
                MoveUp();
                break;
        }
        
        // score is the sum of all the tiles
        Score = 0;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                Score += Board[i, j];
            }
        }

        if (IsThereEmptySpace())
        {
            AddRandomTile();
        }
    }
    
    private void MoveLeft()
    {
        for (var y = 0; y < BOARD_SIZE; y++)
        {
            for (var x = 0; x < BOARD_SIZE; x++)
            {
                if (Board[x, y] == 0) continue; // skip empty cells
                if (x == 0) continue; // skip first column
                
                // move left until we find a cell that is empty or has the same value
                // if we find a cell with the same value, we add them together

                while (true)
                {
                    if (x == 0) break; // we reached the first column
                    
                    if (Board[x - 1, y] == 0)
                    {
                        Board[x - 1, y] = Board[x, y];
                        Board[x, y] = 0;
                        if (x > 0)
                        {
                            x--;
                        }
                        else
                        {
                            break;
                        }
                        continue;
                    }
                    
                    if (Board[x - 1, y] == Board[x, y])
                    {
                        Board[x - 1, y] += Board[x, y];
                        Board[x, y] = 0;
                    }
                    
                    break;
                }
            }
        }
    }
    
    private void MoveRight()
    {
        // -->
        
        for (var y = 0; y < BOARD_SIZE; y++)
        {
            for (var x = BOARD_SIZE - 1; x >= 0; x--)
            {
                if (Board[x, y] == 0) continue; // skip empty cells
                if (x == BOARD_SIZE - 1) continue; // skip last column
                
                // move right until we find a cell that is empty or has the same value
                // if we find a cell with the same value, we add them together
                
                while (true)
                {
                    if (x == BOARD_SIZE - 1) break; // we reached the last column
                    
                    if (Board[x + 1, y] == 0)
                    {
                        Board[x + 1, y] = Board[x, y];
                        Board[x, y] = 0;
                        if (x < BOARD_SIZE - 1)
                        {
                            x++;
                        }
                        else
                        {
                            break;
                        }
                        continue;
                    }
                    
                    if (Board[x + 1, y] == Board[x, y])
                    {
                        Board[x + 1, y] += Board[x, y];
                        Board[x, y] = 0;
                    }
                    
                    break;
                }
            }
        }
    }
    
    private void MoveUp()
    {
        // ^
        
        for (var x = 0; x < BOARD_SIZE; x++)
        {
            for (var y = 0; y < BOARD_SIZE; y++)
            {
                if (Board[x, y] == 0) continue; // skip empty cells
                if (y == 0) continue; // skip first row
                
                // move up until we find a cell that is empty or has the same value
                // if we find a cell with the same value, we add them together
                
                while (true)
                {
                    if (y == 0) break; // we reached the first row
                    
                    if (Board[x, y - 1] == 0)
                    {
                        Board[x, y - 1] = Board[x, y];
                        Board[x, y] = 0;
                        if (y > 0)
                        {
                            y--;
                        }
                        else
                        {
                            break;
                        }
                        continue;
                    }
                    
                    if (Board[x, y - 1] == Board[x, y])
                    {
                        Board[x, y - 1] += Board[x, y];
                        Board[x, y] = 0;
                    }
                    
                    break;
                }
            }
        }
    }
    
    private void MoveDown()
    {
        // v
        
        for (var x = 0; x < BOARD_SIZE; x++)
        {
            for (var y = BOARD_SIZE - 1; y >= 0; y--)
            {
                if (Board[x, y] == 0) continue; // skip empty cells
                if (y == BOARD_SIZE - 1) continue; // skip last row
                
                // move down until we find a cell that is empty or has the same value
                // if we find a cell with the same value, we add them together
                
                while (true)
                {
                    if (y == BOARD_SIZE - 1) break; // we reached the last row
                    
                    if (Board[x, y + 1] == 0)
                    {
                        Board[x, y + 1] = Board[x, y];
                        Board[x, y] = 0;
                        if (y < BOARD_SIZE - 1)
                        {
                            y++;
                        }
                        else
                        {
                            break;
                        }
                        continue;
                    }
                    
                    if (Board[x, y + 1] == Board[x, y])
                    {
                        Board[x, y + 1] += Board[x, y];
                        Board[x, y] = 0;
                    }
                    
                    break;
                }
            }
        }
    }
}