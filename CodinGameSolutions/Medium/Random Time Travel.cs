using System;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        var p = Console.ReadLine().Split();
        BigInteger a = BigInteger.Parse(p[0]), c = BigInteger.Parse(p[1]), m = BigInteger.Parse(p[2]);
        BigInteger seed = BigInteger.Parse(Console.ReadLine());
        long s = long.Parse(Console.ReadLine());
        if (s < 0)
        {
            BigInteger inv = ModInv(a, m);
            BigInteger c2 = (m - (inv * c % m)) % m;
            a = inv;
            c = c2;
            s = -s;
        }
        BigInteger steps = s;
        BigInteger resA = 1, resC = 0, curA = a % m, curC = c % m;
        while (steps > 0)
        {
            if ((steps & 1) == 1)
            {
                resC = (curA * resC + curC) % m;
                resA = (curA * resA) % m;
            }
            curC = (curA * curC + curC) % m;
            curA = (curA * curA) % m;
            steps >>= 1;
        }
        BigInteger result = (resA * (seed % m) + resC) % m;
        Console.WriteLine(result);
    }

    static BigInteger ModInv(BigInteger x, BigInteger m)
    {
        BigInteger s, t;
        BigInteger g = Egcd(x % m + m, m, out s, out t);
        if (g != 1) throw new ArgumentException("Inverse does not exist");
        return (s % m + m) % m;
    }

    static BigInteger Egcd(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
    {
        if (b == 0) { x = 1; y = 0; return a; }
        BigInteger x1, y1;
        BigInteger g = Egcd(b, a % b, out x1, out y1);
        x = y1;
        y = x1 - (a / b) * y1;
        return g;
    }
}
