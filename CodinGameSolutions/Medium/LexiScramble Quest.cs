using System;
using System.Numerics;

class Solution {
    static void Main() {
        var word = Console.ReadLine().Trim();
        int length = word.Length;

        var factorial = new BigInteger[length + 1];
        factorial[0] = 1;
        for (int i = 1; i <= length; i++)
            factorial[i] = factorial[i - 1] * i;

        var freq = new int[26];
        foreach (var ch in word) 
            freq[ch - 'a']++;

        BigInteger rank = 0;
        for (int pos = 0; pos < length; pos++) {
            int currIdx = word[pos] - 'a';
            for (int letter = 0; letter < currIdx; letter++) {
                if (freq[letter] == 0) continue;
                freq[letter]--;
                BigInteger perms = factorial[length - pos - 1];
                for (int k = 0; k < 26; k++)
                    perms /= factorial[freq[k]];
                rank += perms;
                freq[letter]++;
            }
            freq[currIdx]--;
        }
        Console.WriteLine(rank + 1);
    }
}
