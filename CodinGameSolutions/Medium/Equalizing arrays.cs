using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var a = Console.ReadLine().Split().Select(long.Parse).ToArray();
        var b = Console.ReadLine().Split().Select(long.Parse).ToArray();

        var flows = BuildFlows(a, b);
        var operations = ExtractOperations(a, flows);
        Console.WriteLine(operations.Count);
        foreach (var op in operations)
            Console.WriteLine($"{op.position} {op.direction} {op.amount}");
    }

    static List<List<(int direction, long amount)>> BuildFlows(long[] a, long[] b)
    {
        int n = a.Length;
        var flows = new List<List<(int, long)>>(n);
        for (int i = 0; i < n; i++)
            flows.Add(new List<(int, long)>());

        long carry = 0;
        for (int i = 0; i < n; i++)
        {
            if (carry != 0)
            {
                if (carry > 0)
                    flows[i].Add((-1, carry));
                else
                    flows[i - 1].Add((+1, -carry));
            }
            carry = carry - a[i] + b[i];
        }

        return flows;
    }

    static List<(int position, int direction, long amount)> ExtractOperations(long[] a, List<List<(int direction, long amount)>> flows)
    {
        var ops = new List<(int, int, long)>();
        int n = a.Length;

        for (int i = 0; i < n; i++)
        {
            while (i >= 0 && flows[i].Count > 0)
            {
                var (d, x) = flows[i][0];
                if (a[i] < x) break;
                ops.Add((i + 1, d, x));
                a[i] -= x;
                a[i + d] += x;
                flows[i].RemoveAt(0);
                i--;
            }
        }

        return ops;
    }
}
