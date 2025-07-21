using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        int b = int.Parse(Console.ReadLine());
        var shelf = new List<string>();
        for (int i = 0; i < b; i++)
            shelf.Add(Console.ReadLine());

        var d = new Dictionary<string, List<string>>();
        for (int r = 0; r <= 10; r++)
            d[r.ToString()] = new List<string>();

        int dup = -1;
        int n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var rankStr = parts.Last();
            var title = string.Join(" ", parts.Take(parts.Length - 1));
            if (rankStr == "None")
            {
                if (!shelf.Contains(title))
                    shelf.Add(title);
            }
            else
            {
                int rank = int.Parse(rankStr);
                if (shelf.Contains(title) && dup < rank)
                    dup = rank;
                else
                    d[rankStr].Add(title);
            }
        }

        int z = 10;
        while (z >= 0 && d[z.ToString()].Count == 0)
            z--;

        if (dup <= z)
        {
            foreach (var t in d[z.ToString()])
                shelf.Add(t);
            z--;
        }

        if (shelf.Count == n)
        {
            shelf.Sort(StringComparer.Ordinal);
            foreach (var t in shelf)
                Console.WriteLine(t);
            return;
        }
        else if (shelf.Count > n)
        {
            Console.WriteLine("Your TBR is out of control Clara!");
            return;
        }
        else
        {
            var temp = new List<string>(shelf);
            for (int r = z; r >= 0; r--)
            {
                foreach (var t in d[r.ToString()])
                    shelf.Add(t);
                if (shelf.Count == n)
                {
                    shelf.Sort(StringComparer.Ordinal);
                    foreach (var tt in shelf)
                        Console.WriteLine(tt);
                    return;
                }
                else if (shelf.Count > n)
                {
                    temp.Sort(StringComparer.Ordinal);
                    foreach (var tt in temp)
                        Console.WriteLine(tt);
                    return;
                }
                else
                {
                    temp = new List<string>(shelf);
                }
            }
        }
    }
}