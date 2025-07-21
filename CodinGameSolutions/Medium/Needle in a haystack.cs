using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    static void Main()
    {
        var wh = Console.ReadLine().Split();
        int w = int.Parse(wh[0]), h = int.Parse(wh[1]);

        var sb = new StringBuilder(w * h);
        for (int i = 0; i < h; i++)
            sb.Append(Console.ReadLine());
        string hay = sb.ToString();
        int N = hay.Length;

        int T = int.Parse(Console.ReadLine());
        while (T-- > 0)
        {
            var pattern = Console.ReadLine().Trim();
            var wanted = new HashSet<char>();
            foreach (var tok in pattern.Split(','))
            {
                if (tok.Length == 3 && tok[1] == '-')
                {
                    for (char c = tok[0]; c <= tok[2]; c++)
                        wanted.Add(c);
                }
                else
                {
                    wanted.Add(tok[0]);
                }
            }

            int W = wanted.Count;
            int[] count = new int[256];
            int formed = 0;
            int bestLen = int.MaxValue, bestL = 0, bestR = 0;
            int l = 0;

            for (int r = 0; r < N; r++)
            {
                char c = hay[r];
                if (wanted.Contains(c))
                {
                    count[c]++;
                    if (count[c] == 1)
                        formed++;
                }

                while (formed == W)
                {
                    int len = r - l + 1;
                    if (len < bestLen)
                    {
                        bestLen = len;
                        bestL = l;
                        bestR = r;
                    }

                    char cl = hay[l];
                    if (wanted.Contains(cl))
                    {
                        count[cl]--;
                        if (count[cl] == 0)
                            formed--;
                    }
                    l++;
                }
            }
            Console.WriteLine($"{bestL} {bestR}");
        }
    }
}
