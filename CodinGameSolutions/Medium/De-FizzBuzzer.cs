using System;
using System.Collections.Generic;
using System.Text;

class Program
{
    
    static void Main()
    {
        var map = new Dictionary<string, int>();

        for (int i = 1; i <= 1000; i++)
        {
            var fb = FizzBuzzer(i);
            if (fb != i.ToString() && !map.ContainsKey(fb))
                map[fb] = i;
        }

        int n = int.Parse(Console.ReadLine());
        while (n-- > 0)
        {
            var row = Console.ReadLine();
            if (map.TryGetValue(row, out var val))
            {
                Console.WriteLine(val);
            }
            else if (int.TryParse(row, out var x) && FizzBuzzer(x) == row)
            {
                Console.WriteLine(row);
            }
            else
            {
                Console.WriteLine("ERROR");
            }
        }
    }

    static string FizzBuzzer(int x)
    {
        var b = new StringBuilder();
        var s = x.ToString();

        foreach (var c in s)
            if (c == '3')
                b.Append("Fizz");

        for (long p = 3; p <= x; p *= 3)
            if (x % p == 0)
                b.Append("Fizz");

        foreach (var c in s)
            if (c == '5')
                b.Append("Buzz");

        for (long p = 5; p <= x; p *= 5)
            if (x % p == 0)
                b.Append("Buzz");

        return b.Length > 0 ? b.ToString() : s;
    }

}