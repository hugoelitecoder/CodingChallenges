using System;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int rows = 0;
        while ((rows + 1) * (rows + 2) / 2 <= N) rows++;

        string[] shape = { "***", "* *", "* *", "*****" };
        int bottomWidth = shape[3].Length * rows + (rows - 1) * 1;

        for (int row = 1; row <= rows; row++)
        {
            for (int line = 0; line < shape.Length; line++)
            {
                int w   = shape[line].Length;
                int gap = (line == shape.Length-1) ? 1 : 3;
                int groupWidth = row * w + (row - 1) * gap;
                int indent = (bottomWidth - groupWidth) / 2;
                Console.Write(new string(' ', indent));
                for (int g = 0; g < row; g++)
                {
                    Console.Write(shape[line]);
                    if (g < row - 1)
                        Console.Write(new string(' ', gap));
                }
                int trailing = bottomWidth - indent - groupWidth;
                if (trailing > 0)
                    Console.Write(new string(' ', trailing));

                Console.WriteLine();
            }
        }
    }
}
