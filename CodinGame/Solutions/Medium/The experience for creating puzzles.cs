using System;

class Solution
{
    static void Main()
    {
        int level = int.Parse(Console.ReadLine());
        int xpNeeded = int.Parse(Console.ReadLine());
        int puzzles = int.Parse(Console.ReadLine());

        for (int i = 0; i < puzzles; i++)
        {
            int gain = 300;
            while (gain > 0)
            {
                if (gain < xpNeeded)
                {
                    xpNeeded -= gain;
                    gain = 0;
                }
                else
                {
                    gain -= xpNeeded;
                    level++;
                    xpNeeded = (int)(level * Math.Sqrt(level) * 10);
                }
            }
        }

        Console.WriteLine(level);
        Console.WriteLine(xpNeeded);
    }
}
