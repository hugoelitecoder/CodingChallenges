using System;

class Solution
{
    static void Main()
    {
        var inputs = Console.ReadLine().Split();
        int a = int.Parse(inputs[0]);
        int b = int.Parse(inputs[1]);

        int originalA = a, originalB = b;

        while (b != 0)
        {
            int q = a / b;
            int r = a % b;
            Console.WriteLine($"{a}={b}*{q}+{r}");
            a = b;
            b = r;
        }

        Console.WriteLine($"GCD({originalA},{originalB})={a}");
    }
}
