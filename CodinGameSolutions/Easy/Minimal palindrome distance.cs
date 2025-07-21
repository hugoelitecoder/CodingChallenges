using System;

class Solution
{
    public static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        string s = Console.ReadLine();
        const long m = 1000000007;
        const long r = 26;
        long reversePrefix = 0;
        long suffixHash = 0;
        int best = 0;
        long pow = 1;
        var suffixes = new long[n+1];
        for (int i = n-1; i >= 0; i--)
        {
            int c = s[i] - 'A';
            suffixHash = (suffixHash + pow * c) % m;
            suffixes[n - i] = suffixHash;
            pow = pow * r % m;
        }
        for (int i = n-1; i >= 0; i--)
        {
            int c = s[i] - 'A';
            reversePrefix = (reversePrefix * r + c) % m;
            int len = n - i;
            if (reversePrefix == suffixes[len])
            {
                string suf = s.Substring(n - len, len);
                char[] revArr = suf.ToCharArray();
                Array.Reverse(revArr);
                if (new string(revArr) == suf)
                    best = len;
            }
        }
        Console.WriteLine(n - best);
    }
}