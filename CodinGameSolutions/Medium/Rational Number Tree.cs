using System.Text;
using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        for (int _ = 0; _ < N; _++)
        {
            string s = Console.ReadLine().Trim();
            if (s.Contains('/'))
            {
                var parts = s.Split('/');
                BigInteger p = BigInteger.Parse(parts[0]);
                BigInteger q = BigInteger.Parse(parts[1]);
                BigInteger ln = 0, ld = 1, rn = 1, rd = 0;
                var path = new StringBuilder();
                while (true)
                {
                    BigInteger mn = ln + rn;
                    BigInteger md = ld + rd;
                    BigInteger cmp = p * md - mn * q;
                    if (cmp == 0)
                        break;
                    else if (cmp < 0)
                    {
                        rn = mn;
                        rd = md;
                        path.Append('L');
                    }
                    else
                    {
                        ln = mn;
                        ld = md;
                        path.Append('R');
                    }
                }

                Console.WriteLine(path);
            }
            else
            {
                BigInteger ln = 0, ld = 1, rn = 1, rd = 0;
                foreach (char c in s)
                {
                    BigInteger mn = ln + rn;
                    BigInteger md = ld + rd;
                    if (c == 'L')
                    {
                        rn = mn;
                        rd = md;
                    }
                    else
                    {
                        ln = mn;
                        ld = md;
                    }
                }
                BigInteger fn = ln + rn;
                BigInteger fd = ld + rd;
                Console.WriteLine($"{fn}/{fd}");
            }
        }
    }
}
