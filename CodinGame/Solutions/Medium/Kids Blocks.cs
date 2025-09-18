using System;
using System.Collections.Generic;

class Solution
{
    static Dictionary<(int,int,int,int), bool> memo;
    static List<(int a3,int a2,int a1)> combos;
    static int totalWidth;

    static void Main()
    {
        int x1 = int.Parse(Console.ReadLine());
        int x2 = int.Parse(Console.ReadLine());
        int x3 = int.Parse(Console.ReadLine());

        totalWidth = x1 + 2*x2 + 3*x3;
        memo = new Dictionary<(int,int,int,int), bool>();
        bool possible = false;
        for (int H = 2; H <= x1+x2+x3; H++)
        {
            if (totalWidth % H != 0) continue;
            int W = totalWidth / H;
            combos = BuildCombos(W, x3, x2, x1);
            if (combos.Count == 0) continue;
            memo.Clear();
            if (CanFill(x3, x2, x1, H))
            {
                possible = true;
                break;
            }
        }
        Console.WriteLine(possible ? "POSSIBLE" : "NOT POSSIBLE");
    }

    static List<(int,int,int)> BuildCombos(int W, int max3, int max2, int max1)
    {
        var list = new List<(int,int,int)>();
        for (int a3 = 0; a3 <= Math.Min(max3, W/3); a3++)
            for (int a2 = 0; a2 <= Math.Min(max2, (W-3*a3)/2); a2++)
            {
                int rem = W - 3*a3 - 2*a2;
                if (rem >= 0 && rem <= max1)
                    list.Add((a3,a2,rem));
            }
        return list;
    }

    static bool CanFill(int c3,int c2,int c1,int rows)
    {
        if (rows == 0) return c3==0 && c2==0 && c1==0;
        var key = (c3,c2,c1,rows);
        if (memo.TryGetValue(key, out bool res)) return res;
        foreach (var (a3,a2,a1) in combos)
        {
            if (a3<=c3 && a2<=c2 && a1<=c1)
            {
                if (CanFill(c3-a3, c2-a2, c1-a1, rows-1))
                {
                    memo[key] = true;
                    return true;
                }
            }
        }
        memo[key] = false;
        return false;
    }
}