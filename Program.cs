using System.Text;

const int Width = 40;
const int Height = 20;
const int InitialDelayMs = 120;

try { Console.CursorVisible = false; } catch (IOException) { }
try { Console.OutputEncoding = Encoding.UTF8; } catch (IOException) { }

var rng = new Random();
bool playAgain = true;

while (playAgain)
{
    int score = PlayOneGame();

    Console.SetCursorPosition(0, Height + 3);
    Console.WriteLine($"Awww, you lost! Final score: {score}");
    Console.Write("Want another go? (Y/N): ");

    ConsoleKey choice;
    do
    {
        choice = Console.ReadKey(intercept: true).Key;
    } while (choice != ConsoleKey.Y && choice != ConsoleKey.N);

    Console.WriteLine(choice == ConsoleKey.Y ? "Y" : "N");
    playAgain = choice == ConsoleKey.Y;
}

Console.WriteLine("Thanks for playing!");

int PlayOneGame()
{
    var snake = new LinkedList<(int X, int Y)>();
    snake.AddFirst((Width / 2, Height / 2));
    var direction = (X: 1, Y: 0);
    var nextDirection = direction;
    var food = SpawnFood(snake);
    int score = 0;
    int delayMs = InitialDelayMs;
    bool gameOver = false;

    DrawBorder(snake, food, score);

    while (!gameOver)
    {
        var tickStart = DateTime.UtcNow;
        while ((DateTime.UtcNow - tickStart).TotalMilliseconds < delayMs)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;
                nextDirection = key switch
                {
                    ConsoleKey.UpArrow when direction.Y == 0 => (0, -1),
                    ConsoleKey.DownArrow when direction.Y == 0 => (0, 1),
                    ConsoleKey.LeftArrow when direction.X == 0 => (-1, 0),
                    ConsoleKey.RightArrow when direction.X == 0 => (1, 0),
                    _ => nextDirection
                };
            }
            Thread.Sleep(5);
        }

        direction = nextDirection;
        var head = snake.First!.Value;
        var newHead = (X: head.X + direction.X, Y: head.Y + direction.Y);

        if (newHead.X < 0 || newHead.X >= Width || newHead.Y < 0 || newHead.Y >= Height ||
            snake.Contains(newHead))
        {
            gameOver = true;
            continue;
        }

        snake.AddFirst(newHead);
        DrawCell(newHead, '█');

        if (newHead == food)
        {
            score++;
            food = SpawnFood(snake);
            DrawCell(food, '●');
            if (delayMs > 40) delayMs -= 4;
        }
        else
        {
            var tail = snake.Last!.Value;
            snake.RemoveLast();
            DrawCell(tail, ' ');
        }

        DrawScore(score);
    }

    return score;
}

(int X, int Y) SpawnFood(LinkedList<(int X, int Y)> body)
{
    (int X, int Y) candidate;
    do
    {
        candidate = (rng.Next(Width), rng.Next(Height));
    } while (body.Contains(candidate));
    return candidate;
}

void DrawBorder(LinkedList<(int X, int Y)> snake, (int X, int Y) food, int score)
{
    Console.Clear();
    Console.SetCursorPosition(0, 0);
    Console.WriteLine("+" + new string('-', Width) + "+");
    for (int y = 0; y < Height; y++)
        Console.WriteLine("|" + new string(' ', Width) + "|");
    Console.WriteLine("+" + new string('-', Width) + "+");
    DrawCell(snake.First!.Value, '█');
    DrawCell(food, '●');
    DrawScore(score);
}

void DrawCell((int X, int Y) cell, char ch)
{
    Console.SetCursorPosition(cell.X + 1, cell.Y + 1);
    Console.Write(ch);
}

void DrawScore(int value)
{
    Console.SetCursorPosition(0, Height + 2);
    Console.Write($"Score: {value}   ");
}
