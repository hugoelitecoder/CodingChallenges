using System;
using System.Numerics;

public class Solution
{
    public static void Main()
    {
        if (!int.TryParse(Console.ReadLine(), out var N))
            return;

        var sol = new Solution();
        for (var i = 0; i < N; i++)
        {
            var line = Console.ReadLine();
            if (!BigInteger.TryParse(line, out var x))
            {
                Console.WriteLine();
                continue;
            }
            Console.WriteLine(sol.NumberToWords(x).ToLowerInvariant());
        }
    }

    private static readonly string[] BelowTen = 
        { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    private static readonly string[] BelowTwenty = 
        { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
    private static readonly string[] BelowHundred = 
        { "", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

    public string NumberToWords(BigInteger num) => num switch
    {
        var n when n == 0                              => "Zero",
        var n when n < 0                               => "Negative " + NumberToWords(BigInteger.Negate(n)),
        var n when n < 10                              => BelowTen[(int)n],
        var n when n < 20                              => BelowTwenty[(int)(n - 10)],
        var n when n < 100                             => 
            BelowHundred[(int)(n / 10)] 
            + ((n % 10 > 0) ? "-" + BelowTen[(int)(n % 10)] : ""),
        var n when n < 1_000                           => 
            NumberToWords(n / 100) + " Hundred" 
            + ((n % 100 > 0) ? " " + NumberToWords(n % 100) : ""),
        var n when n < 1_000_000                       => 
            NumberToWords(n / 1_000) + " Thousand" 
            + ((n % 1_000 > 0) ? " " + NumberToWords(n % 1_000) : ""),
        var n when n < 1_000_000_000                   => 
            NumberToWords(n / 1_000_000) + " Million" 
            + ((n % 1_000_000 > 0) ? " " + NumberToWords(n % 1_000_000) : ""),
        var n when n < 1_000_000_000_000               => 
            NumberToWords(n / 1_000_000_000) + " Billion" 
            + ((n % 1_000_000_000 > 0) ? " " + NumberToWords(n % 1_000_000_000) : ""),
        var n when n < 1_000_000_000_000_000           => 
            NumberToWords(n / 1_000_000_000_000) + " Trillion" 
            + ((n % 1_000_000_000_000 > 0) 
               ? " " + NumberToWords(n % 1_000_000_000_000) 
               : ""),
        var n when n < 1_000_000_000_000_000_000       => 
            NumberToWords(n / 1_000_000_000_000_000) + " Quadrillion" 
            + ((n % 1_000_000_000_000_000 > 0) 
               ? " " + NumberToWords(n % 1_000_000_000_000_000) 
               : ""),
        var n                                        => 
            NumberToWords(n / 1_000_000_000_000_000_000) + " Quintillion" 
            + ((n % 1_000_000_000_000_000_000 > 0) 
               ? " " + NumberToWords(n % 1_000_000_000_000_000_000) 
               : "")
    };
}
