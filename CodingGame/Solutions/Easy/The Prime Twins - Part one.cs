class Solution
{
    static void Main()
    {
        var n = int.Parse(System.Console.ReadLine()!);
        var limit = 200000;
        if (n + 10 > limit) limit = n + 10000;
        var isPrime = Sieve(limit);
        for (var p = n + 1; p + 2 <= limit; p++)
        {
            if (isPrime[p] && isPrime[p + 2])
            {
                System.Console.WriteLine($"{p} {p + 2}");
                return;
            }
        }
    }

    // Sieve of Eratosthenes up to max (inclusive)
    private static bool[] Sieve(int max)
    {
        var sieve = new bool[max + 1];
        for (var i = 2; i <= max; i++) sieve[i] = true;
        for (var i = 2; i * i <= max; i++)
        {
            if (!sieve[i]) continue;
            for (var j = i * i; j <= max; j += i)
                sieve[j] = false;
        }
        return sieve;
    }
}
