using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split(' ');
        var n = int.Parse(parts[0]);
        var k = int.Parse(parts[1]);
        var positions = new List<int[]>();
        for (var i = 0; i < k; i++)
        {
            var s = Console.ReadLine().Split(' ');
            var arr = new int[n];
            for (var j = 0; j < n; j++) arr[j] = int.Parse(s[j]);
            positions.Add(arr);
        }
        for (var i = 0; i < k; i++)
        {
            var game = new MisereNim(positions[i]);
            var moves = game.WinningMoves();
            if (moves.Count == 0)
            {
                Console.WriteLine("CONCEDE");
            }
            else
            {
                moves.Sort((a, b) =>
                {
                    if (a.Item1 != b.Item1) return a.Item1.CompareTo(b.Item1);
                    return a.Item2.CompareTo(b.Item2);
                });
                var outList = new List<string>();
                for (var m = 0; m < moves.Count; m++)
                {
                    outList.Add($"{moves[m].Item1}:{moves[m].Item2}");
                }
                Console.WriteLine(string.Join(" ", outList));
            }
        }
    }
}

class MisereNim
{
    private int[] _heaps;
    public MisereNim(int[] heaps)
    {
        _heaps = new int[heaps.Length];
        for (var i = 0; i < heaps.Length; i++) _heaps[i] = heaps[i];
    }

    public List<Tuple<int, int>> WinningMoves()
    {
        var n = _heaps.Length;
        var result = new List<Tuple<int, int>>();
        for (var i = 0; i < n; i++)
        {
            for (var take = 1; take <= _heaps[i]; take++)
            {
                var state = new int[n];
                for (var j = 0; j < n; j++) state[j] = _heaps[j];
                state[i] -= take;
                if (IsLosingPosition(state))
                {
                    result.Add(Tuple.Create(i + 1, take));
                }
            }
        }
        return result;
    }

    private static bool IsLosingPosition(int[] heaps)
    {
        var n = heaps.Length;
        var cntNonOne = 0;
        var cntAlive = 0;
        for (var i = 0; i < n; i++)
        {
            if (heaps[i] > 1) cntNonOne++;
            if (heaps[i] > 0) cntAlive++;
        }
        if (cntNonOne == 0)
        {
            return (cntAlive % 2 == 1);
        }
        var nim = 0;
        for (var i = 0; i < n; i++) nim ^= heaps[i];
        return nim == 0;
    }
}