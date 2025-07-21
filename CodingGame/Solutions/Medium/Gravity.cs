using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int w = dims[0], h = dims[1];
        var grid = Enumerable.Range(0, h)
                             .Select(_ => Console.ReadLine())
                             .ToArray();

        var counts = Enumerable.Range(0, w)
                               .Select(x => grid.Count(row => row[x] == '#'))
                               .ToArray();

        foreach (int y in Enumerable.Range(0, h))
        {
            Console.WriteLine(string.Concat(
                    Enumerable.Range(0, w)
                              .Select(x => y < h - counts[x] ? '.' : '#')
                )
            );
        }
    }
}