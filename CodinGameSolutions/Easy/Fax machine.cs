using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int W = int.Parse(Console.ReadLine());
        int H = int.Parse(Console.ReadLine());
        var input = Console.ReadLine().Split().Select(int.Parse).ToList();

        int runIdx = 0, runLeft = 0;
        char color = '*';
        var line = "";

        for (int i = 0; i < W * H;)
        {
            if (runLeft == 0 && runIdx < input.Count)
            {
                runLeft = input[runIdx++];
                if (runLeft == 0 && runIdx < input.Count)
                {
                    color = color == '*' ? ' ' : '*';
                }
                continue; 
            }
            line += color;
            runLeft--;
            i++;
            if (runLeft == 0 && runIdx < input.Count)
            {
                color = color == '*' ? ' ' : '*';
            }
            if (line.Length == W)
            {
                Console.WriteLine("|" + line + "|");
                line = "";
            }
        }
    }
}
