using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var d = int.Parse(Console.ReadLine());
        var n = int.Parse(Console.ReadLine());
        var perm = Console.ReadLine()
            .Split(' ')
            .Select(int.Parse)
            .Select(x => x - 1)
            .ToArray();
        var vectors = new List<(int[] vec, int idx)>();
        for (var i = 0; i < n; i++)
        {
            var vec = Console.ReadLine()
                .Split(' ')
                .Select(int.Parse)
                .ToArray();
            vectors.Add((vec, i + 1));
        }
        vectors.Sort((a, b) => Compare(a.vec, b.vec, perm));
        Console.WriteLine(string.Join(" ", vectors.Select(x => x.idx)));
    }
    private static int Compare(int[] a, int[] b, int[] perm)
    {
        for (var i = 0; i < perm.Length; i++)
        {
            var diff = a[perm[i]] - b[perm[i]];
            if (diff != 0) return diff;
        }
        return 0;
    }
}
