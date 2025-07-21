using System;
class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var x = 1;
        while (x * (x + 1) / 2 < n) x++;
        Console.WriteLine(x);
    }
}
