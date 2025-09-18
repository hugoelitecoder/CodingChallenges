using System;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split('/');
            long x = long.Parse(parts[0]);
            long y = long.Parse(parts[1]);
            try
            {
                var f = new Fraction(x, y);
                Console.WriteLine(f);
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("DIVISION BY ZERO");
            }
        }
    }
}

class Fraction
{
    private long _num;
    private long _den;

    public Fraction(long numerator, long denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException();

        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        long g = Gcd(Math.Abs(numerator), denominator);
        _num = numerator / g;
        _den = denominator / g;
    }

    private static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            long t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    public override string ToString()
    {
        if (_num == 0)
            return "0";

        if (_den == 1)
            return _num.ToString();

        long absNum = Math.Abs(_num);
        long intPart = _num / _den;
        long rem = absNum % _den;

        if (intPart != 0)
        {
            return rem == 0
                ? intPart.ToString()
                : string.Format("{0} {1}/{2}", intPart, rem, _den);
        }

        return string.Format("{0}/{1}", _num, _den);
    }
}
