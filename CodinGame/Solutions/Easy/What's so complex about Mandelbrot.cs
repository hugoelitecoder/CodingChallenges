using System;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        var s = Console.ReadLine().Trim();
        int m = int.Parse(Console.ReadLine());

        int split = -1;
        for (int i = 1; i < s.Length - 1; i++)
        {
            if (s[i] == '+' || s[i] == '-')
                split = i;
        }
        var realPart = s.Substring(0, split);
        var imagPart = s.Substring(split, s.Length - split - 1);

        double cRe = double.Parse(realPart, CultureInfo.InvariantCulture);
        double cIm = double.Parse(imagPart, CultureInfo.InvariantCulture);

        double re = 0.0, im = 0.0;
        for (int i = 1; i <= m; i++)
        {
            double nextRe = re * re - im * im + cRe;
            double nextIm = 2 * re * im + cIm;
            re = nextRe;
            im = nextIm;

            if (re * re + im * im > 4.0)
            {
                Console.WriteLine(i);
                return;
            }
        }

        Console.WriteLine(m);
    }
}
