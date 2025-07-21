using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class Solution
{
    static void Main()
    {
        var m = Regex.Match(Console.ReadLine(), @"(.+)\+(.+)=(.+)");
        var x = m.Groups[1].Value;
        var y = m.Groups[2].Value;
        var z = m.Groups[3].Value;
        int minBase = RadixConverter.Digits.IndexOf($"{x}{y}{z}".Max()) + 1;
        var converter = new RadixConverter();
        for (int b = minBase; ; b++)
        {
            if (converter.From(x, b) + converter.From(y, b) == converter.From(z, b))
            {
                Console.WriteLine(b);
                break;
            }
        }
    }

    class RadixConverter
    {
        public const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public long From(string s, int b)
        {
            long v = 0;
            foreach (char c in s) {
                v = v * b + Digits.IndexOf(c);
            }
            return v;
        }

        public string To(long n, int b)
        {
            if (n == 0) return "0";
            var sb = new StringBuilder();
            while (n > 0)
            {
                sb.Append(Digits[(int)(n % b)]);
                n /= b;
            }
            var a = sb.ToString().ToCharArray();
            Array.Reverse(a);
            return new string(a);
        }
    }
}
