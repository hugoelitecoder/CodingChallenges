using System;
using System.Collections.Generic;
using System.Linq;
class Solution
{
    public static void Main(string[] args)
    {
        var c = int.Parse(Console.ReadLine());
        var names = Enumerable.Range(0, c).Select(_ => Console.ReadLine()).ToList();
        var v = int.Parse(Console.ReadLine());
        var ballots = Enumerable.Range(0, v)
            .Select(_ => Console.ReadLine().Split(' ').Select(x => int.Parse(x) - 1).ToArray())
            .ToList();
        var eliminated = GetEliminationOrder(c, names, ballots);
        foreach (var name in eliminated.Take(eliminated.Count - 1))
            Console.WriteLine(name);
        Console.WriteLine("winner:" + eliminated.Last());
    }

    static List<string> GetEliminationOrder(int c, List<string> names, List<int[]> ballots)
    {
        var alive = Enumerable.Repeat(true, c).ToArray();
        var order = new List<string>();
        for (var left = c; left > 1; left--)
        {
            var counts = CountVotes(alive, ballots, c);
            var min = counts.Where((_, i) => alive[i]).Min();
            var idx = Enumerable.Range(0, c)
                .Where(i => alive[i] && counts[i] == min)
                .First();
            alive[idx] = false;
            order.Add(names[idx]);
        }
        order.Add(names[Array.FindIndex(alive, x => x)]);
        return order;
    }

    static int[] CountVotes(bool[] alive, List<int[]> ballots, int c)
    {
        var counts = new int[c];
        foreach (var ballot in ballots)
            counts[ballot.First(id => alive[id])]++;
        return counts;
    }
}
