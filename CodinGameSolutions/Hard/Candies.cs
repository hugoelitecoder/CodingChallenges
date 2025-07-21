using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var n = int.Parse(inputs[0]);
        var k = int.Parse(inputs[1]);
        var res = new List<List<int>>();
        Backtrack(n, k, new List<int>(), res);
        res.Sort((a, b) => {
            for (int i = 0; i < Math.Min(a.Count, b.Count); i++)
            {
                if (a[i] != b[i]) return a[i] - b[i];
            }
            return a.Count - b.Count;
        });
        foreach (var seq in res)
            Console.WriteLine(string.Join(" ", seq));
    }
    static void Backtrack(int remain, int k, List<int> curr, List<List<int>> res)
    {
        if (remain == 0)
        {
            res.Add(new List<int>(curr));
            return;
        }
        for (int i = 1; i <= Math.Min(k, remain); i++)
        {
            curr.Add(i);
            Backtrack(remain - i, k, curr, res);
            curr.RemoveAt(curr.Count - 1);
        }
    }
}
