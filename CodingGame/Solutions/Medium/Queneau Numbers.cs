using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        if (!int.TryParse(Console.ReadLine(), out int N))
        {
            Console.Error.WriteLine("Invalid input");
            return;
        }

        var qn = new QueneauNumber(N);
        if (!qn.Compute())
        {
            Console.WriteLine("IMPOSSIBLE");
            return;
        }

        foreach (var seq in qn.Steps)
            Console.WriteLine(string.Join(",", seq));
    }
}

class QueneauNumber
{
    public int Size { get; }
    private readonly int[] _map;
    private readonly int[] _original;
    private readonly List<int[]> _steps = new List<int[]>();
    public IReadOnlyList<int[]> Steps => _steps;

    public QueneauNumber(int size)
    {
        if (size < 2 || size > 30)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be between 2 and 30.");

        Size      = size;
        _map      = BuildMap(size);
        _original = Enumerable.Range(1, size).ToArray();
    }

    public bool Compute()
    {
        _steps.Clear();
        var current = (int[])_original.Clone();

        for (int i = 0; i < Size; i++)
        {
            current = ApplyPermutation(current);
            _steps.Add(current);
        }

        return current.SequenceEqual(_original);
    }

    private int[] ApplyPermutation(int[] seq)
    {
        var next = new int[Size];
        for (int i = 0; i < Size; i++)
            next[i] = seq[_map[i]];
        return next;
    }

    private static int[] BuildMap(int n)
    {
        var map = new int[n];
        int left = 0, right = n - 1, idx = 0;
        while (left < right)
        {
            map[idx++] = right--;
            map[idx++] = left++;
        }
        if (left == right)
            map[idx] = left;
        return map;
    }
}
