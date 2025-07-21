using System;

class Solution
{
    static void Main()
    {
        const int N = 16;
        var grid = new string[N];
        for (int i = 0; i < N; i++)
            grid[i] = Console.ReadLine() ?? "";

        int count = 0;
        for (int y = 0; y < N; y++)
        {
            bool inside = false;
            int sw = 0;
            var line = grid[y];
            for (int x = 0; x < line.Length; x++)
            {
                char c = line[x];
                if (inside && c == 'o')
                    count++;

                if (c == '|')
                {
                    inside = !inside;
                }
                else if (c == '+')
                {
                    int newState = (y > 0 && (grid[y - 1][x] == '|' || grid[y - 1][x] == '+')) ? 1 : -1;
                    if (sw == 0)
                    {
                        sw = newState;
                    }
                    else
                    {
                        if (newState != sw)
                            inside = !inside;
                        sw = 0;
                    }
                }
            }
        }

        Console.WriteLine(count);
    }
}
