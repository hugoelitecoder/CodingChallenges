using System;

class Solution
{
    static void Main(string[] args)
    {
        int lines = int.Parse(Console.ReadLine());
        int pats  = int.Parse(Console.ReadLine());

        int[] masks = new int[pats];
        int[] steps = new int[pats];

        for (int i = 0; i < pats; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            string pat = parts[0];
            steps[i]    = int.Parse(parts[1]);
            int m      = 0;
            for (int b = 0; b < 4; b++)
                if (pat[b] == 'X')
                    m |= 1 << (3 - b);
            masks[i] = m;
        }

        int[] grid = new int[lines];

        for (int i = 0; i < pats; i++)
        {
            int step = steps[i];
            int mask = masks[i];
            for (int j = step; j <= lines; j += step)
                grid[j - 1] |= mask;
        }

        for (int i = lines - 1; i >= 0; i--)
        {
            int m = grid[i];
            for (int b = 3; b >= 0; b--)
                Console.Write((m & (1 << b)) != 0 ? 'X' : '0');
            Console.WriteLine();
        }
    }
}
