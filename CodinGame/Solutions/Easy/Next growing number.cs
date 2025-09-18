using System;

class Solution
{
    static void Main()
    {
        string s = Console.ReadLine();
        int L = s.Length;

        // Try to find next growing number of same length
        for (int pos = L - 1; pos >= 0; pos--)
        {
            // Check if prefix s[0..pos-1] is non-decreasing
            bool prefixOK = true;
            for (int k = 1; k < pos; k++)
            {
                if (s[k] < s[k - 1])
                {
                    prefixOK = false;
                    break;
                }
            }
            if (!prefixOK) continue;

            int prevDigit = (pos > 0) ? s[pos - 1] - '0' : 0;
            int curDigit  = s[pos]     - '0';
            int d = Math.Max(prevDigit, curDigit + 1);

            if (d <= 9)
            {
                // Build result: prefix + d repeated for remaining positions
                var ch = (char)('0' + d);
                var result = s.Substring(0, pos)
                           + new string(ch, L - pos);
                Console.WriteLine(result);
                return;
            }
        }

        // If none found, the next growing number has length L+1 of all '1's
        Console.WriteLine(new string('1', L + 1));
    }
}
