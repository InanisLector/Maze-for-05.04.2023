using System;
using System.Text;

namespace Maze;
class Program
{
    private static Map map = new Map(3, 3);
    private static Action<Vector2>? inputCycle;

    private static Vector2 playerPosition = new(1, 1);

    #region Game Cycle

    static void Main(string[] args)
    {
        inputCycle += GameCycle;

        RenderMap();

        while (true)
            CheckInput();
    }

    static void GameCycle(Vector2 input)
    {
        Console.WriteLine("In game loop");

        MovePlayer(input);

        RenderMap();
    }

    #endregion

    #region Player controls

    static void CheckInput()
    {
        Vector2 input = Console.ReadKey(true)
                .Key switch
            {
                ConsoleKey.W => new Vector2(0, -1),
                ConsoleKey.A => new Vector2(-1, 0),
                ConsoleKey.S => new Vector2(0, 1),
                ConsoleKey.D => new Vector2(1, 0),
                _ => new Vector2()
            };

        inputCycle?.Invoke(input);
    }

    static void MovePlayer(Vector2 input)
    {
        if (map[playerPosition + input] != '.')
            return;

        playerPosition += input;
    }

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

    private static char[,] GenerateMap(int width, int length)
    {
        char[,] map = new char[width, length];

        for (int j = 0; j < length; j++)
        {
            for (int i = 0; i < width; i++)
            {
                if (i == 0 || j == 0 || j == length - 1 || i == width - 1)
                    map[i, j] = '#';
                else
                    map[i, j] = '.';
            }
        }


        return map;
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
                if (playerPosition == i)
                    builder.Append("@");
                else
                    builder.Append(map[i]);
            }

            builder.Append("\n");
        }

        Console.WriteLine(builder.ToString());
    }

    #endregion

    #region External

#pragma warning disable CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()    // Оно задрало, вот и офнул, если так можно
#pragma warning disable CS0660 // Тип определяет оператор == или оператор !=, но не переопределяет Object.Equals(object o) // Та же фигня
    struct Vector2
    {
        public int x;
        public int y;

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

        public static bool operator ==(Vector2 v1, Vector2 v2)
            => v1.x == v2.x && v1.y == v2.y;

        public static bool operator !=(Vector2 v1, Vector2 v2)
            => !(v1 == v2);
    }

    #endregion
}