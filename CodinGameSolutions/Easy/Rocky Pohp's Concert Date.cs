using System;

class Solution
{
    static void Main()
    {
        string s = Console.ReadLine().Trim();
        for (int i = 1; i < s.Length; i++)
        {
            long a = long.Parse(s.Substring(0, i));
            long b = long.Parse(s.Substring(i));
            if (AreValidFactors(a, b))
            {
                string concatDate = ConcatDate(a, b);
                string date = ReverseConcat(concatDate);
                if (IsValidDate(date))
                {
                    Console.WriteLine(date);
                    return;
                }
            }
        }
    }

    static bool AreValidFactors(long a, long b)
    {
        long n = a * b;
        if (n == 0) return false;

        long div = (long)Math.Floor(Math.Sqrt(n));
        while (div > 0 && n % div != 0)
            div--;

        return div == a || div == b;
    }

    static string ConcatDate(long a, long b)
        => (a * b).ToString().PadLeft(8, '0');

    static string ReverseConcat(string cd)
        => $"{cd.Substring(0, 4)}-{cd.Substring(4, 2)}-{cd.Substring(6, 2)}";

    static bool IsValidDate(string date)
    {
        var parts = date.Split('-');
        int month = int.Parse(parts[1]);
        int day   = int.Parse(parts[2]);
        return month >= 1 && month <= 12
            && day   >= 1 && day   <= 31;
    }
}
