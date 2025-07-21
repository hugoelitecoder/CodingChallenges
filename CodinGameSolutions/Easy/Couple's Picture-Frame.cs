using System;
using System.Numerics;
using System.Text;

class Solution
{
    static void Main()
    {
        string wife = Console.ReadLine()!;
        string husband = Console.ReadLine()!;

        int wLen = wife.Length;
        int hLen = husband.Length;

        int gcd = (int)BigInteger.GreatestCommonDivisor(wLen, hLen);
        int lcm = wLen / gcd * hLen;

        string top = Repeat(wife, lcm / wLen);
        string low = Repeat(husband, lcm / hLen);

        Console.WriteLine(top);

        int innerSpaces = lcm - 2;
        var spaces = new string(' ', innerSpaces);
        for (int i = 0; i < lcm; i++)
        {
            Console.WriteLine($"{low[i]}{spaces}{top[i]}");
        }

        Console.WriteLine(low);
    }

    static string Repeat(string s, int count)
    {
        var sb = new StringBuilder(s.Length * count);
        for (int i = 0; i < count; i++)
            sb.Append(s);
        return sb.ToString();
    }
}
