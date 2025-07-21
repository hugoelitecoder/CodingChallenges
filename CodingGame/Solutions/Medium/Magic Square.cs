using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var square =Enumerable.Range(0, n)
            .Select(_ => Console.ReadLine().Split().Select(int.Parse).ToArray())
            .ToArray();

        Console.WriteLine(IsMagic(square, n) ? "MAGIC" : "MUGGLE");
    }

    static bool IsMagic(int[][] sq, int n)
    {
        var all = sq.SelectMany(r => r);
        if (new HashSet<int>(all).Count != n * n || all.Min() != 1 || all.Max() != n * n)
            return false;

        int target = sq[0].Sum();
        return RowsOk(sq, target)
            && ColsOk(sq, target, n)
            && DiagsOk(sq, target, n);
    }

    static bool RowsOk(int[][] sq, int target) =>
        sq.All(r => r.Sum() == target);

    static bool ColsOk(int[][] sq, int target, int n) =>
        Enumerable.Range(0, n).All(j => sq.Sum(r => r[j]) == target);

    static bool DiagsOk(int[][] sq, int target, int n) =>
        Enumerable.Range(0, n).Sum(i => sq[i][i]) == target
        && Enumerable.Range(0, n).Sum(i => sq[i][n - 1 - i]) == target;
}
