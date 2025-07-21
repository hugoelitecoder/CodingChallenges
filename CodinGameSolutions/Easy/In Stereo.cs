using System;
class Solution
{
    static void Main()
    {
        int pLen = int.Parse(Console.ReadLine());
        string pattern0 = Console.ReadLine();
        int sLen = int.Parse(Console.ReadLine());
        string stock0 = Console.ReadLine();
        var rc = Console.ReadLine().Split();
        int H = int.Parse(rc[0]), W = int.Parse(rc[1]);
        var depth = new int[W];
        for (int i = 0; i < H; i++)
        {
            var line = Console.ReadLine();
            string pat = pattern0, stk = stock0, outL = "";
            int d = 0;
            for (int j = 0; j < W; j++)
            {
                int nd = line[j] - '0', diff = nd - d;
                if (diff > 0) pat = pat.Substring(diff);
                else if (diff < 0)
                {
                    diff = -diff;
                    pat = stk.Substring(0, diff) + pat;
                    stk = stk.Substring(diff);
                }
                outL += pat[0];
                pat = pat.Substring(1) + pat[0];
                d = nd;
            }
            Console.WriteLine(outL);
        }
    }
}
