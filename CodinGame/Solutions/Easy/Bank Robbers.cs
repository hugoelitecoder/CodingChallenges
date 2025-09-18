using System;

class Solution
{
    static void Main(string[] args)
    {
        var R = int.Parse(Console.ReadLine());
        var V = int.Parse(Console.ReadLine());
        var times = new long[R];

        for (var i = 0; i < V; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var C = int.Parse(inputs[0]);
            var N = int.Parse(inputs[1]);

            long combos = 1;
            for (var j = 0; j < N; j++) combos *= 10;
            for (var j = 0; j < C - N; j++) combos *= 5;

            var minIdx = 0;
            for (var j = 1; j < R; j++)
                if (times[j] < times[minIdx]) minIdx = j;

            times[minIdx] += combos;
        }

        long maxTime = 0;
        foreach (var t in times)
            if (t > maxTime) maxTime = t;

        Console.WriteLine(maxTime);
    }
}
