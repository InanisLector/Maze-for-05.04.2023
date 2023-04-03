using System;
using System.Text;

namespace Maze;
class Program
{
    public static char[,] map;

    static void Main(string[] args)
    {
        map = GenerateMap(40, 20);

        RenderMap(map);
    }

    static char[,] GenerateMap(int width, int length)
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

    static void RenderMap(char[,] map)
    {
        int width = map.GetUpperBound(0) + 1;
        int length = map.GetUpperBound(1) + 1;

        StringBuilder builder = new StringBuilder((width + 2) * length); // +1 is For \n

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                builder.Append(map[j, i]);
            }

            builder.Append("\n");
        }

        Console.WriteLine(builder.ToString());
    }

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
    }
}