using System;

class Solution
{
    static void Main(string[] args)
    {
        var s = Console.ReadLine();
        var k = int.Parse(Console.ReadLine());
        var freq = new int[26];
        var distinct = 0;
        var maxLen = 0;
        var left = 0;
        for (var right = 0; right < s.Length; right++)
        {
            var idx = s[right] - 'a';
            if (freq[idx] == 0) distinct++;
            freq[idx]++;
            while (distinct > k)
            {
                var lidx = s[left] - 'a';
                freq[lidx]--;
                if (freq[lidx] == 0) distinct--;
                left++;
            }
            var len = right - left + 1;
            if (len > maxLen) maxLen = len;
        }
        Console.WriteLine(maxLen);
    }
}
