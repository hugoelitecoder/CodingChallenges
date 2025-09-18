using System;

class Solution
{
    static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var minS = int.MaxValue;
        for (var a = 1; a * a * a <= N; ++a)
        {
            if (N % a != 0) continue;
            var rem1 = N / a;
            for (var b = 1; b * b <= rem1; ++b)
            {
                if (rem1 % b != 0) continue;
                var c = rem1 / b;
                var s = 2 * (a * b + b * c + c * a);
                if (s < minS) minS = s;
            }
        }
        var maxS = 4 * N + 2;
        Console.WriteLine($"{minS} {maxS}");
    }
}
