using System;

class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split(' ');
        var F0 = ulong.Parse(parts[0]);
        var N = int.Parse(parts[1]);
        parts = Console.ReadLine().Split(' ');
        var a = int.Parse(parts[0]);
        var b = int.Parse(parts[1]);
        var F = new ulong[N + 1];
        F[0] = F0;
        for (var i = 1; i <= N; i++)
        {
            var sum = 0ul;
            for (var age = a; age <= b; age++)
                if (i - age >= 0)
                    sum += F[i - age];
                else
                    break;
            F[i] = sum;
        }
        Console.WriteLine(F[N]);
    }
}
