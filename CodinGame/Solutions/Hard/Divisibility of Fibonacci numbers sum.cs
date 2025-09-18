using System;
class Solution
{
    public static void Main(string[] args)
    {
        var nb = int.Parse(Console.ReadLine());
        for (var t = 0; t < nb; t++)
        {
            var s = Console.ReadLine().Split(' ');
            var a = long.Parse(s[0]);
            var b = long.Parse(s[1]);
            var d = int.Parse(s[2]);
            var sum = (FibMod(b + 2, d) - FibMod(a + 1, d) + d) % d;
            var label = $"F_{a} + ... + F_{b} is ";
            Console.WriteLine(label + (sum == 0 ? "divisible" : "NOT divisible") + $" by {d}");
        }
    }
    static int FibMod(long n, int mod)
    {
        if (n == 0) return 0;
        var m = new int[,] { { 1, 1 }, { 1, 0 } };
        var r = PowMat(m, n - 1, mod);
        return r[0, 0];
    }
    static int[,] PowMat(int[,] m, long exp, int mod)
    {
        var res = new int[,] { { 1, 0 }, { 0, 1 } };
        while (exp > 0)
        {
            if ((exp & 1) == 1) res = MulMat(res, m, mod);
            m = MulMat(m, m, mod);
            exp >>= 1;
        }
        return res;
    }
    static int[,] MulMat(int[,] a, int[,] b, int mod)
    {
        var r = new int[2, 2];
        r[0, 0] = (int)(((long)a[0, 0] * b[0, 0] + (long)a[0, 1] * b[1, 0]) % mod);
        r[0, 1] = (int)(((long)a[0, 0] * b[0, 1] + (long)a[0, 1] * b[1, 1]) % mod);
        r[1, 0] = (int)(((long)a[1, 0] * b[0, 0] + (long)a[1, 1] * b[1, 0]) % mod);
        r[1, 1] = (int)(((long)a[1, 0] * b[0, 1] + (long)a[1, 1] * b[1, 1]) % mod);
        return r;
    }
}
