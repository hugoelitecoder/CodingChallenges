using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    struct Candidate { public int A,B; public char Op; }

    static int n;
    static int[] R;
    static int[] counts = new int[10];
    static List<Candidate>[] allCands;
    static bool[] usedR;
    static Candidate[] sol;
    static int solutionCount = 0;
    static Candidate[] firstSol;

    static void Main()
    {
        n = int.Parse(Console.ReadLine());
        R = Console.ReadLine().Split()
                .Select(int.Parse).ToArray();
        var occ = Console.ReadLine().Split()
                .Select(int.Parse).ToArray();
        for (int i = 1; i <= 9; i++)
            counts[i] = occ[i-1];

        allCands = new List<Candidate>[n];
        for (int idx = 0; idx < n; idx++)
        {
            int target = R[idx];
            var list = new List<Candidate>();
            for (int a = 1; a <= 9; a++)
                for (int b = a; b <= 9; b++)
                {
                    if (a + b == target)
                        list.Add(new Candidate{A=a,B=b,Op='+'});
                    if (a * b == target)
                        list.Add(new Candidate{A=a,B=b,Op='x'});
                }
            allCands[idx] = list;
        }

        usedR = new bool[n];
        sol   = new Candidate[n];
        firstSol = new Candidate[n];

        DFS(0);

        Console.WriteLine(solutionCount);
        if (solutionCount == 1)
        {
            var order = Enumerable.Range(0,n)
                          .OrderBy(i => R[i]);
            foreach (var i in order)
            {
                var c = firstSol[i];
                Console.WriteLine($"{c.A} {c.Op} {c.B} = {R[i]}");
            }
        }
    }

    static void DFS(int placed)
    {
        if (placed == n)
        {
            solutionCount++;
            if (solutionCount == 1)
                Array.Copy(sol, firstSol, n);
            return;
        }

        int bestIdx = -1, bestCnt = int.MaxValue;
        for (int idx = 0; idx < n; idx++)
        {
            if (usedR[idx]) continue;
            int cnt = 0;
            foreach (var c in allCands[idx])
            {
                if (Valid(c)) cnt++;
            }
            if (cnt == 0) return;
            if (cnt < bestCnt)
            {
                bestCnt = cnt;
                bestIdx = idx;
            }
        }

        usedR[bestIdx] = true;
        foreach (var c in allCands[bestIdx])
        {
            if (!Valid(c)) continue;
            Apply(c, -1);
            sol[bestIdx] = c;
            DFS(placed+1);
            Apply(c, +1);
        }
        usedR[bestIdx] = false;
    }

    static bool Valid(Candidate c)
    {
        if (c.A == c.B)
            return counts[c.A] >= 2;
        else
            return counts[c.A] >= 1 && counts[c.B] >= 1;
    }

    static void Apply(Candidate c, int delta)
    {
        counts[c.A] += delta;
        counts[c.B] += delta;
    }
}
