using System.Text;

namespace Maze;
class Program
{
    private static Map map ;
    private static Action<Vector2>? inputCycle;

    private static Vector2 playerPosition = new(1, 1);
    private static bool didntWin;

    private const int maxCrossLength = 10;

    #region Game Cycle

    static void Main(string[] args)
    {
        inputCycle += GameCycle;

        while (true)
        {
            playerPosition.x = 1;
            playerPosition.y = 1;

            map = new Map(30, 20);
            didntWin = true;
            
            RenderMap();

            while (didntWin)
                CheckInput();

            Console.ReadKey(true);
        }
    }

    static void GameCycle(Vector2 input)
    {
        MovePlayer(input);
        didntWin = !CheckForWin();
        RenderMap();
    }

    #endregion

    #region Player controls

    static void CheckInput()
    {
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

        inputCycle?.Invoke(input);
    }

    static void MovePlayer(Vector2 input)
    {
        if (map[playerPosition + input] == '#')
            return;

        playerPosition += input;
    }

    static bool CheckForWin()
        => map[playerPosition] == 'E';
        // => playerPosition == new Vector2(map.width - 2, map.height - 2) // Возможно быстрее

    #endregion

    #region Map

    struct Map
    {
        public readonly int width;
        public readonly int height;

        private readonly char[,] _map;

        public Map(int width, int height)
        {
            width = width >= 5 ? width : 5; // Check for smallest maze
            height = height >= 5 ? height : 5;

            width = width % 2 == 0 ? width + 1 : width; // Check for walls working properly
            height = height % 2 == 0 ? height + 1 : height;

            this.width = width;
            this.height = height;

            _map = GenerateMap(width, height);
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

    private static char[,] GenerateMap(int width, int height)
    {
        char[,] map = new char[width, height];

        Vector2 i = new();

        for (i.y = 0; i.y < height; i.y++)
        {
            for (i.x = 0; i.x < width; i.x++)
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


                map[i.x, i.y] = '.';
            }
        }

        char[,] generatedMap;

        while(!GenerateInnerWalls(map, width, height, out generatedMap)) {}

        generatedMap[width - 2, height - 2] = 'E';

        return generatedMap;
    }

    private static bool GenerateInnerWalls(char[,] map, int width, int height, out char[,] finishedMap)
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
        
        path.Append(currentPosition);

        Random rand = new();

        do
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
            if(currentPosition.y < heightOfBool - 1)
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
            else if(availableNeighbors == 0)
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

        } while (toVisit > 0);
            
        return true;
    }

    static void RenderMap()
    {
        Console.Clear();

        StringBuilder builder = new StringBuilder((map.width + 2) * map.height); // +2 is For \n

        Vector2 i = new(); // 0, 0 - base value

        for (i.y = 0; i.y < map.height; i.y++)
        {
            for (i.x = 0; i.x < map.width; i.x++)
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
        public static Vector2 Down=> new Vector2(0, 1);
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