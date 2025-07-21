using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var monthMap = new Dictionary<string, int>();
        for (int i = 0; i < monthNames.Length; i++)
            monthMap[monthNames[i]] = i;

        int N = int.Parse(Console.ReadLine().Trim());
        var sum = new List<int> { 0 };

        for (int i = 0; i < N; i++)
        {
            string M = Console.ReadLine().Trim();
            int dCount = M.Length / 3;
            var digits = new List<int>(dCount);
            for (int j = 0; j < dCount; j++)
            {
                string chunk = M.Substring(3 * j, 3);
                digits.Add(monthMap[chunk]);
            }
            digits.Reverse();
            AddToSum(sum, digits);
        }

        while (sum.Count > 1 && sum[sum.Count - 1] == 0)
            sum.RemoveAt(sum.Count - 1);

        var sb = new StringBuilder();
        for (int i = sum.Count - 1; i >= 0; i--)
            sb.Append(monthNames[sum[i]]);

        Console.WriteLine(sb.ToString());
    }

    static void AddToSum(List<int> sum, List<int> add)
    {
        int maxLen = Math.Max(sum.Count, add.Count);
        int carry = 0;
        for (int i = 0; i < maxLen; i++)
        {
            int a = i < sum.Count ? sum[i] : 0;
            int b = i < add.Count ? add[i] : 0;
            int s = a + b + carry;
            int d = s % 12;
            carry = s / 12;
            if (i < sum.Count) sum[i] = d;
            else sum.Add(d);
        }
        while (carry > 0)
        {
            sum.Add(carry % 12);
            carry /= 12;
        }
    }
}