using System;
using System.Linq;

public class Solution
{
    public static void Main()
    {
        var s = Console.ReadLine().Split("\\n");
        Enumerable.Range(0, int.Parse(Console.ReadLine()))
                  .Select(_ => Console.ReadLine().Split('|'))
                  .Select(p => new { l = int.Parse(p[0]), o = int.Parse(p[1]), t = p[2].Replace("\\n", "\n") })
                  .OrderByDescending(x => x.o)
                  .ToList()
                  .ForEach(c => s[c.l] = s[c.l].Insert(c.o, c.t));
        Console.Write(string.Join('\n', s));
    }
}
