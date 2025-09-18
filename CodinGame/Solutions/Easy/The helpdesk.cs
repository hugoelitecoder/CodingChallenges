using System;
using System.Linq;

public class Solution
{
    public static void Main()
    {
        var worktime = int.Parse(Console.ReadLine());
        var nc = int.Parse(Console.ReadLine());
        var eff = Console.ReadLine().Split().Select(double.Parse).ToArray();
        var nv = int.Parse(Console.ReadLine());
        var vis = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var help = new int[nc];
        var br = new int[nc];
        var t = new double[nc];
        var w = new double[nc];
        var i = 0;
        while (i < nv)
        {
            var m = t.Min();
            var c = -1;
            for (var j = 0; j < nc; j++)
                if (t[j] == m && w[j] < worktime) { c = j; break; }
            if (c >= 0)
            {
                help[c]++;
                var d = vis[i++] / eff[c];
                t[c] += d;
                w[c] += d;
            }
            else
            {
                c = Array.IndexOf(t, m);
                t[c] += 10;
                w[c] = 0;
                br[c]++;
            }
        }
        Console.WriteLine(string.Join(" ", help));
        Console.WriteLine(string.Join(" ", br));
    }
}
