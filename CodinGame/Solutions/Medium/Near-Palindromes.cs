using System;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var sb = new StringBuilder(N);
        for (int i = 0; i < N; i++)
        {
            string word = Console.ReadLine();
            sb.Append(IsNearPalindrome(word) ? '1' : '0');
        }
        Console.WriteLine(sb);
    }

    static bool IsNearPalindrome(string s)
    {
        int i = 0, j = s.Length - 1;
        while (i < j)
        {
            if (s[i] == s[j])
            {
                i++; j--;
            }
            else
            {
                return IsPalindromeRange(s, i + 1, j)
                    || IsPalindromeRange(s, i, j - 1)
                    || IsPalindromeRange(s, i + 1, j - 1);
            }
        }
        return true;
    }

    static bool IsPalindromeRange(string s, int l, int r)
    {
        while (l < r)
        {
            if (s[l++] != s[r--]) return false;
        }
        return true;
    }
}
