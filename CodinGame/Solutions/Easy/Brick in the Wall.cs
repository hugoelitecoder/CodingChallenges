using System;
class Solution
{
    public static void Main(string[] args)
    {
        var X = int.Parse(Console.ReadLine());
        var N = int.Parse(Console.ReadLine());
        var weights = new int[N];
        var parts = Console.ReadLine().Split();
        for (var i = 0; i < N; i++) weights[i] = int.Parse(parts[i]);
        Array.Sort(weights);
        Array.Reverse(weights);
        var S = 0.0;
        for (var i = 0; i < N; i++) S += (i / X) * weights[i];
        var W = S * 0.65;
        Console.WriteLine(W.ToString("F3"));
    }
}
