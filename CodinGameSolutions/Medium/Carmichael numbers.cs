using System;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        if (n < 2 || IsPrime(n))
        {
            Console.WriteLine("NO");
            return;
        }

        for (int a = 2; a < n; a++)
        {
            if (Gcd(a, n) != 1) 
                continue;
            if (ModPow(a, n, n) != a % n)
            {
                Console.WriteLine("NO");
                return;
            }
        }

        Console.WriteLine("YES");
    }

    static bool IsPrime(int x)
    {
        if (x < 2) return false;
        if (x % 2 == 0) return x == 2;
        int r = (int)Math.Sqrt(x);
        for (int i = 3; i <= r; i += 2)
            if (x % i == 0) return false;
        return true;
    }

    static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = a % b;
            a = b;
            b = t;
        }
        return a;
    }

    static int ModPow(int a, int exp, int mod)
    {
        long result = 1;
        long baseVal = a % mod;
        while (exp > 0)
        {
            if ((exp & 1) == 1)
                result = (result * baseVal) % mod;
            baseVal = (baseVal * baseVal) % mod;
            exp >>= 1;
        }
        return (int)result;
    }
}
