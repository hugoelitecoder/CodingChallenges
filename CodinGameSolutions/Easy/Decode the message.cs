using System;

class Solution
{
    static void Main()
    {
        var P = long.Parse(Console.ReadLine());
        var C = Console.ReadLine();
        int k = C.Length;

        long sum = 0;
        long kPow = 1;
        int L = 0;
        while (true)
        {
            L++;
            if (kPow > long.MaxValue / k)
            {
                kPow = P + 1;
            }
            else
            {
                kPow *= k;
            }
            if (sum > long.MaxValue - kPow)
                sum = P + 1;
            else
                sum += kPow;

            if (sum > P) break;
        }

        long offset = sum - kPow;
        long idxInLen = P - offset;
        var chars = new char[L];
        for (int j = 0; j < L; j++)
        {
            var d = (int)(idxInLen % k);
            chars[j] = C[d];
            idxInLen /= k;
        }

        Console.WriteLine(new string(chars));
    }
}
