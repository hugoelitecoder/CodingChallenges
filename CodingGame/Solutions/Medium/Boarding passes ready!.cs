using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int h = int.Parse(Console.ReadLine());
        int w = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());

        var leftRows = new List<(int col, string name)>[h];
        var rightRows = new List<(int col, string name)>[h];
        for (int i = 0; i < h; i++)
        {
            leftRows[i] = new List<(int, string)>();
            rightRows[i] = new List<(int, string)>();
        }

        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var parts = line.Split(',');
            string name = parts[0];
            string seat = parts[1];
            int row = int.Parse(seat[..^1]) - 1;
            int col = seat[^1] - 'A';
            if (col < w/2)
                leftRows[row].Add((col, name));
            else
                rightRows[row].Add((col, name));
        }
        var leftQ = new Queue<string>[h];
        var rightQ = new Queue<string>[h];
        for (int r = 0; r < h; r++)
        {
            leftRows[r].Sort((a, b) => a.col.CompareTo(b.col));
            leftQ[r] = new Queue<string>(leftRows[r].Select(x => x.name));
            rightRows[r].Sort((a, b) => b.col.CompareTo(a.col));
            rightQ[r] = new Queue<string>(rightRows[r].Select(x => x.name));
        }

        int boarded = 0, group = 1;
        while (boarded < n)
        {
            var boarding = new List<string>();
            bool isLeft = (group % 2 == 1);
            for (int r = h - 1; r >= 0; r--)
            {
                var q = isLeft ? leftQ[r] : rightQ[r];
                if (q.Count > 0)
                {
                    boarding.Add(q.Dequeue());
                    boarded++;
                }
            }

            if (boarding.Count > 0)
                Console.WriteLine("Now boarding: " + string.Join(",", boarding));

            group++;
        }
    }
}
