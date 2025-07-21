using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        long result = 0;
        for (int i = 1; i <= n; i++) {
            result += n/i * i;
        }
        Console.WriteLine(result);
    }
}
