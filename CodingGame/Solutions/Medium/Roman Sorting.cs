using System;
using System.Linq;

class Solution
{
    static string ToRoman(int n) =>
        n >= 1000 ? "M" + ToRoman(n - 1000) :
        n >= 900  ? "CM" + ToRoman(n - 900) :
        n >= 500  ? "D" + ToRoman(n - 500) :
        n >= 400  ? "CD" + ToRoman(n - 400) :
        n >= 100  ? "C" + ToRoman(n - 100) :
        n >= 90   ? "XC" + ToRoman(n - 90) :
        n >= 50   ? "L" + ToRoman(n - 50) :
        n >= 40   ? "XL" + ToRoman(n - 40) :
        n >= 10   ? "X" + ToRoman(n - 10) :
        n >= 9    ? "IX" + ToRoman(n - 9) :
        n >= 5    ? "V" + ToRoman(n - 5) :
        n >= 4    ? "IV" + ToRoman(n - 4) :
        n >= 1    ? "I" + ToRoman(n - 1) :
                    "";

    static void Main()
    {
        var sorted = Enumerable.Range(0, int.Parse(Console.ReadLine()))
            .Select(_ => int.Parse(Console.ReadLine()))
            .Select(x => (Value: x, Roman: ToRoman(x)))
            .OrderBy(p => p.Roman, StringComparer.Ordinal)
            .Select(p => p.Value);

        Console.WriteLine(string.Join(" ", sorted));
    }
}
