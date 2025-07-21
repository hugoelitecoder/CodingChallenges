using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var s = Console.ReadLine();
        var n = s.Length;
        var maxLen = 0;
        var palindromes = new HashSet<string>();
        for (int center = 0; center < n; center++)
        {
            Expand(s, center, center, palindromes, ref maxLen);
            Expand(s, center, center + 1, palindromes, ref maxLen);
        }
        var result = new List<string>();
        var seen = new HashSet<string>();
        for (int i = 0; i <= n - maxLen; i++)
        {
            var sub = s.Substring(i, maxLen);
            if (IsPalindrome(sub) && !seen.Contains(sub))
            {
                result.Add(sub);
                seen.Add(sub);
            }
        }
        foreach (var str in result)
            Console.WriteLine(str);
    }

    static void Expand(string s, int left, int right, HashSet<string> palindromes, ref int maxLen)
    {
        while (left >= 0 && right < s.Length && s[left] == s[right])
        {
            var len = right - left + 1;
            if (len > maxLen)
            {
                palindromes.Clear();
                palindromes.Add(s.Substring(left, len));
                maxLen = len;
            }
            else if (len == maxLen)
            {
                palindromes.Add(s.Substring(left, len));
            }
            left--;
            right++;
        }
    }

    static bool IsPalindrome(string s)
    {
        int l = 0, r = s.Length - 1;
        while (l < r)
        {
            if (s[l++] != s[r--]) return false;
        }
        return true;
    }
}
