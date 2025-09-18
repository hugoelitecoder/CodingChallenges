using System;
class Solution
{
    public static void Main(string[] args)
    {
        var a1 = int.Parse(Console.ReadLine());
        var N = int.Parse(Console.ReadLine());
        if (N == 1) { Console.WriteLine(a1); return; }
        var size = Math.Max(N, a1) + 1;
        var lastOccurrence = new int[size];
        var prev = a1;
        lastOccurrence[prev] = 1;
        for (var i = 2; i <= N; i++)
        {
            var last = lastOccurrence[prev];
            var next = last > 0 ? (i - 1) - last : 0;
            lastOccurrence[prev] = i - 1;
            prev = next;
        }
        Console.WriteLine(prev);
    }
}
