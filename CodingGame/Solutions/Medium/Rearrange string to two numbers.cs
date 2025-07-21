using System;
using System.Linq;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        string s = Console.ReadLine();
        int[] counts = GetCounts(s);

        var a1 = (int[])counts.Clone();
        var b1 = new int[10]; b1[0] = 1;
        if (Check(a1, b1))
        {
            Console.WriteLine($"{ToNumber(b1)} {ToNumber(a1)}");
            return;
        }

        var a2 = (int[])counts.Clone();
        var b2 = new int[10]; b2[0] = 18; b2[1] = 1;
        if (Check(a2, b2))
        {
            Console.WriteLine($"{ToNumber(a2)} {ToNumber(b2)}");
            return;
        }

        var a3 = (int[])counts.Clone();
        var b3 = new int[10];
        int k = 18, last = -1;
        for (int i = 9; i >= 0; i--)
        {
            int d = Math.Min(k, a3[i]);
            if (d > 0 && i > 0) last = i;
            a3[i] -= d;
            b3[i] += d;
            k -= d;
        }
        if (!IsNumber(a3) && last != -1)
        {
            a3[last]++;
            b3[last]--;
            if (a3[0] > 0)
            {
                a3[0]--;
                b3[0]++;
            }
        }
        if (IsNumber(a3) && IsNumber(b3))
        {
            Console.WriteLine($"{ToNumber(a3)} {ToNumber(b3)}");
        }
        else
        {
            Console.WriteLine("-1 -1");
        }
    }

    static int[] GetCounts(string s)
    {
        var counts = new int[10];
        foreach (char c in s)
            counts[c - '0']++;
        return counts;
    }

    static bool IsNumber(int[] a)
    {
        long len = a.Sum();
        if (a.Any(x => x < 0) || len == 0)
            return false;
        if (len == 1)
            return true;
        if (len == 19 && a[0] == 18 && a[1] == 1)
            return true;
        if (len < 19 && a[0] < len)
            return true;
        return false;
    }

    static string ToNumber(int[] a)
    {
        var b = (int[])a.Clone();
        var sb = new StringBuilder();
        for (int i = 1; i <= 9; i++)
        {
            if (b[i] > 0)
            {
                b[i]--;
                sb.Append((char)('0' + i));
                break;
            }
        }
        for (int i = 0; i <= 9; i++)
            for (int j = 0; j < b[i]; j++)
                sb.Append((char)('0' + i));
        return sb.ToString();
    }

    static bool Check(int[] a, int[] b)
    {
        for (int i = 0; i < 10; i++)
            a[i] -= b[i];
        if (IsNumber(a))
            return true;
        for (int i = 0; i < 10; i++)
            a[i] += b[i];
        return false;
    }
}