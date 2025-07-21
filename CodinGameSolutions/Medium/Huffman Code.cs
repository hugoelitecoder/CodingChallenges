using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var pq = Console.ReadLine()
                        .Split()
                        .Select(long.Parse)
                        .ToList();
        if (n == 1)
        {
            Console.WriteLine(pq[0]);
            return;
        }
        
        pq.Sort();
        long cost = 0;
        
        while (pq.Count > 1)
        {
            long a = pq[0], b = pq[1];
            pq.RemoveRange(0, 2);
            
            long s = a + b;
            cost += s;
            
            int idx = pq.BinarySearch(s);
            if (idx < 0) idx = ~idx;
            pq.Insert(idx, s);
        }
        
        Console.WriteLine(cost);
    }
}
