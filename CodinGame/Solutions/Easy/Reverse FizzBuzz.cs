using System;
class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var lines = new string[n];
        for (var i = 0; i < n; i++)
        {
            lines[i] = Console.ReadLine();
        }
        var start = GetStart(lines);
        for (var f = 1; f < 60; f++)
        {
            for (var b = 1; b < 60; b++)
            {
                if (Matches(f, b, start, lines))
                {
                    Console.WriteLine($"{f} {b}");
                    return;
                }
            }
        }
    }
    private static int GetStart(string[] lines)
    {
        for (var i = 0; i < lines.Length; i++)
        {
            if (int.TryParse(lines[i], out var v))
            {
                return v - i;
            }
        }
        return 1;
    }
    private static bool Matches(int f, int b, int start, string[] lines)
    {
        for (var i = 0; i < lines.Length; i++)
        {
            var x = start + i;
            var expected = (x % f == 0 && x % b == 0) ? "FizzBuzz"
                         : (x % f == 0) ? "Fizz"
                         : (x % b == 0) ? "Buzz"
                         : x.ToString();
            if (expected != lines[i]) return false;
        }
        return true;
    }
}
