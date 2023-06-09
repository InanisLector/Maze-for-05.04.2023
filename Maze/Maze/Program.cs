using System.Text;

namespace Maze;
class Program
{
    static void Main(string[] args)
    {
        Game maze = new();
        maze.Start();
    }
}

public class Game
{
    private Map map;
    private Action<Vector2>? inputCycle;

    private Vector2 playerPosition = new(1, 1);
    private bool didntWin;

    private int mazeWidth = 31;
    private int mazeHeight = 21;

    private int renderWidth = 21;
    private int renderHeight = 15;

    private bool somethingChanged;

    private HashCode seed;

    #region Game Cycle

    public void Start()
    {
        while (true)
        {
            QuizMenu();
            GameInit();

            GameCycle();

            Console.ReadKey(true);
        }
    }

    private void GameInit()
    {
        playerPosition.x = 1;
        playerPosition.y = 1;

        map = new Map(mazeWidth, mazeHeight);
        map.GenerateMap(); // Я просто не хотел чтобы Generate map можно было вызвать откуда угодно. Если есть способ получше - скажите
        
        didntWin = true;
    }

    private void GameCycle()
    {
        while (didntWin)
        {
            somethingChanged = false;

            Vector2 input = GetInput();
            MovePlayer(input);

            didntWin = !CheckForWin();
            RenderScaledMap();
        }
    }

    private void QuizMenu()
    {
        Console.WriteLine("What is your preferable maze size\nInput like 31 21\nIf you don't want it, just press Enter. It will just generate automatically or take you previous settings");

        string s = Console.ReadLine();

        try
        {
            var wh = s.Split(" ").Select(x => int.Parse(x)).ToArray();

            mazeWidth = wh[0];
            mazeHeight = wh[1];
        }
        catch { }

        Console.WriteLine("What is your preferable render distance\nInput like 21 15\nIf you don't want it, just press Enter. It will just generate automatically or take you previous settings");

        s = Console.ReadLine();

        try
        {
            var wh = s.Split(" ").Select(x => int.Parse(x)).ToArray();

            renderWidth =
                wh[0] < mazeWidth ?
                    wh[0] :
                    mazeWidth + 1;
            renderHeight =
                wh[1] < mazeHeight ?
                    wh[1] :
                    mazeHeight + 1;
        }
        catch { }

    }

    #endregion

    #region Player controls

    private Vector2 GetInput()
    {
        if (Console.KeyAvailable)
            return Vector2.Zero;

        Vector2 input = Console.ReadKey(true)
                .Key switch
        {
            ConsoleKey.W => Vector2.Up,
            ConsoleKey.UpArrow => Vector2.Up,
            ConsoleKey.A => Vector2.Left,
            ConsoleKey.LeftArrow => Vector2.Left,
            ConsoleKey.S => Vector2.Down,
            ConsoleKey.DownArrow => Vector2.Down,
            ConsoleKey.D => Vector2.Right,
            ConsoleKey.RightArrow => Vector2.Right,
            _ => Vector2.Zero
        };

        return input;
    }

    private void MovePlayer(Vector2 input)
    {
        if (map[playerPosition + input] is '#' or '-' or '+' or '|')
            return;

        playerPosition += input;
    }

    private bool CheckForWin()
        => map[playerPosition] == 'E';
    // => playerPosition == new Vector2(map.width - 2, map.height - 2) // Возможно быстрее

    #endregion

    #region Map

    struct Map
    {
        const bool enableDecor = false;

        public readonly int width;
        public readonly int height;

        private char[,] _map;

        private int maxCrossLength = 10;

        public Map(int width, int height)
        {
            width = width >= 5 ? width : 5; // Check for smallest maze
            height = height >= 5 ? height : 5;

            width = width % 2 == 0 ? width + 1 : width; // Check for walls working properly
            height = height % 2 == 0 ? height + 1 : height;

            this.width = width;
            this.height = height;

            _map = new char[width, height];
        }

        public void GenerateMap()
        {
            char[,] map = new char[width, height];

            Vector2 i = new();

            for (i.y = 0; i.y < height; i.y++)
            {
                for (i.x = 0; i.x < width; i.x++)
                {
                    if (enableDecor)
                    {
                        if (i.x == 0 || i.y == 0 || i.y == height - 1 || i.x == width - 1) // Borders
                        {
                            map[i.x, i.y] = '#';
                            continue;
                        }

                        if (i.x % 2 == 0 && i.y % 2 == 0) // Intersections
                        {
                            map[i.x, i.y] = '+';
                            continue;
                        }

                        if (i.x % 2 == 0) // Vertical conections
                        {
                            map[i.x, i.y] = '|';
                            continue;
                        }

                        if (i.y % 2 == 0) // Horizontal conections
                        {
                            map[i.x, i.y] = '-';
                            continue;
                        }
                    }
                    else
                    {
                        if (i.x == 0 || i.y == 0 || i.y == height - 1 || i.x == width - 1) // Borders
                        {
                            map[i.x, i.y] = '#';
                            continue;
                        }

                        if (i.x % 2 == 0 || i.y % 2 == 0) // Intersections
                        {
                            map[i.x, i.y] = '#';
                            continue;
                        }
                    }


                    map[i.x, i.y] = '.';
                }
            }

            char[,] generatedMap;

            while (!GenerateInnerWalls(map, width, height, out generatedMap)) { }

            generatedMap[width - 2, height - 2] = 'E';

            _map = generatedMap;
        }

        private bool GenerateInnerWalls(char[,] map, int width, int height, out char[,] finishedMap)
        {
            int widthOfBool = width >> 1; // Division by 2
            int heightOfBool = height >> 1;

            int lastCross = 0;

            finishedMap = map;

            bool[,] visited = new bool[widthOfBool, heightOfBool];
            int toVisit = (widthOfBool) * (heightOfBool);

            //Vector2 currentPosition = new(0, 0);
            Vector2 currentPosition = new(widthOfBool - 1, heightOfBool - 1);
            Stack<Vector2> path = new Stack<Vector2>();

            path.Push(currentPosition);

            Random rand = new();

            while (toVisit > 0)
            {
                if (!visited[currentPosition.x, currentPosition.y])
                {
                    toVisit--;
                    visited[currentPosition.x, currentPosition.y] = true;
                }

                var neighbors = new Vector2[4];
                int availableNeighbors = 0;

                if (currentPosition.y > 0)
                {
                    (int x, int y) = (currentPosition + Vector2.Up);
                    if (!visited[x, y])
                        neighbors[availableNeighbors++] = currentPosition + Vector2.Up;
                }
                if (currentPosition.y < heightOfBool - 1)
                {
                    (int x, int y) = (currentPosition + Vector2.Down);
                    if (!visited[x, y])
                        neighbors[availableNeighbors++] = currentPosition + Vector2.Down;
                }
                if (currentPosition.x > 0)
                {
                    (int x, int y) = (currentPosition + Vector2.Left);
                    if (!visited[x, y])
                        neighbors[availableNeighbors++] = currentPosition + Vector2.Left;
                }
                if (currentPosition.x < widthOfBool - 1)
                {
                    (int x, int y) = (currentPosition + Vector2.Right);
                    if (!visited[x, y])
                        neighbors[availableNeighbors++] = currentPosition + Vector2.Right;
                }

                bool makeCross = false;

                if (lastCross > maxCrossLength)
                    makeCross = rand.Next(100) > 49;

                if (makeCross)
                {
                    int crossLength = rand.Next(maxCrossLength);

                    for (int i = 0; i < crossLength; i++)
                    {
                        if (path.Count == 0)
                            return false;

                        currentPosition = path.Pop();
                    }

                    lastCross = 0;
                }
                else if (availableNeighbors == 0)
                {
                    if (path.Count == 0)
                        return false;

                    lastCross = 0;
                    currentPosition = path.Pop();
                }
                else
                {
                    lastCross++;

                    path.Push(currentPosition);

                    currentPosition = neighbors[rand.Next(availableNeighbors)];
                    Vector2 addPathPosition = currentPosition + path.Peek() + new Vector2(1, 1); // Расчёты на бумаге были для координат, начиная с (1, 1)

                    map[addPathPosition.x, addPathPosition.y] = '.';
                }

            }

            return true;
        }

        public char this[Vector2 vector]
        {
            get => _map[vector.x, vector.y];
            set => _map[vector.x, vector.y] = value;
        }

        public char this[int x, int y]
        {
            get => _map[x, y];
            set => _map[x, y] = value;
        }

        public char this[(int x, int y) tuple]
        {
            get => _map[tuple.x, tuple.y];
            set => _map[tuple.x, tuple.y] = value;
        }
    }

    private void RenderScaledMap()
    {
        Console.Clear();

        StringBuilder builder = new StringBuilder((renderWidth + 2) * renderHeight);

        Vector2 renderStartPosition =
            playerPosition - new Vector2(renderWidth >> 1, renderHeight >> 1);

        // Below zero

        renderStartPosition.x =
            renderStartPosition.x >= 0 ?
            renderStartPosition.x :
            0;

        renderStartPosition.y =
            renderStartPosition.y >= 0 ?
            renderStartPosition.y :
            0;

        // Above limit

        renderStartPosition.x =
            renderStartPosition.x < map.width - renderWidth ?
                renderStartPosition.x :
                map.width - renderWidth;

        renderStartPosition.y =
            renderStartPosition.y < map.height - renderHeight ?
                renderStartPosition.y :
                map.height - renderHeight;

        Vector2 renderEndPosition =
            renderStartPosition + new Vector2(renderWidth, renderHeight);

        Vector2 i = new();

        for (i.y = renderStartPosition.y; i.y < renderEndPosition.y; i.y++)
        {
            for (i.x = renderStartPosition.x; i.x < renderEndPosition.x; i.x++)
            {
                if (i == playerPosition)
                {
                    builder.Append("@");
                    continue;
                }

                builder.Append(map[i]);
            }

            builder.Append("\n");
        }

        Console.WriteLine(builder.ToString());

        if (!didntWin)
            Console.WriteLine("\n\n\n You won! \n Press any key to play again");
    }

    #endregion

    #region External

#pragma warning disable CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()    // Оно задрало, вот и офнул, если так можно
#pragma warning disable CS0660 // Тип определяет оператор == или оператор !=, но не переопределяет Object.Equals(object o) // Та же фигня
    struct Vector2
    {
        public int x;
        public int y;

        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 Up => new Vector2(0, -1);
        public static Vector2 Down => new Vector2(0, 1);
        public static Vector2 Left => new Vector2(-1, 0);
        public static Vector2 Right => new Vector2(1, 0);


        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
            => new Vector2(v1.x + v2.x, v1.y + v2.y);

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
            => new Vector2(v1.x - v2.x, v1.y - v2.y);

        public static Vector2 operator *(Vector2 v1, int multiplier)
            => new Vector2(v1.x * multiplier, v1.y * multiplier);

        //public static Vector2 operator /(Vector2 v1, int divisor)
        //{
        //    float multiplier = 1f / divisor;

        //    return new Vector2((int)(v1.x * multiplier), (int)(v1.y * multiplier));
        //}

        public static bool operator ==(Vector2 v1, Vector2 v2)
            => v1.x == v2.x && v1.y == v2.y;

        public static bool operator !=(Vector2 v1, Vector2 v2)
            => !(v1 == v2);

        public override string ToString()
            => $"({x}, {y})";

        public void Deconstruct(out int x, out int y)
        {
            x = this.x;
            y = this.y;
        }
    }

    #endregion
}