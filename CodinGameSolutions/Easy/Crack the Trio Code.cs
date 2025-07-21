using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        // Read input list L
        var L = Console.ReadLine()
                       .Split(',')
                       .Select(int.Parse)
                       .ToArray();
        int M = L.Max();

        var count = 0;
        (int a,int b,int c) single = (0,0,0);

        for (int a = 1; a <= M; a++)
        {
            for (int b = a; b <= M; b++)
            {
                for (int c = b; c <= M; c++)
                {
                    var sums = new HashSet<int>();
                    sums.Add(a); sums.Add(b); sums.Add(c);
                    sums.Add(a + a); sums.Add(a + b); sums.Add(a + c);
                    sums.Add(b + b); sums.Add(b + c); sums.Add(c + c);
                    sums.Add(a + a + a); sums.Add(a + a + b); sums.Add(a + a + c);
                    sums.Add(a + b + b); sums.Add(a + b + c); sums.Add(a + c + c);
                    sums.Add(b + b + b); sums.Add(b + b + c); sums.Add(b + c + c);
                    sums.Add(c + c + c);

                    bool ok = true;
                    foreach (var x in L)
                    {
                        if (!sums.Contains(x)) { ok = false; break; }
                    }
                    if (!ok) continue;

                    count++;
                    if (count == 1)
                        single = (a, b, c);

                    if (count > 1)
                    {
                        Console.WriteLine("many");
                        return;
                    }
                }
            }
        }

        if (count == 0)
        {
            Console.WriteLine("none");
        }
        else
        {
            var (a, b, c) = single;
            Console.WriteLine($"{a},{b},{c}");
        }
    }
}
