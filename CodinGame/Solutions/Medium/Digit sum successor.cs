using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        string input = Console.ReadLine().Trim();
        int sum = ComputeDigitSum(input);
        string result = FindNextSameLength(input, sum) 
                           ?? FindNextLonger(input.Length, sum);
        Console.WriteLine(result);
    }

    static int ComputeDigitSum(string s)
    {
        return s.Sum(ch => ch - '0');
    }

    static string FindNextSameLength(string s, int targetSum)
    {
        int len = s.Length;
        int[] digits = s.Select(ch => ch - '0').ToArray();
        for (int pos = len - 1; pos >= 0; pos--)
        {
            int prefixSum = digits.Take(pos).Sum();
            for (int d = digits[pos] + 1; d <= 9; d++)
            {
                int rem = targetSum - (prefixSum + d);
                int suffixLen = len - pos - 1;
                if (rem < 0 || rem > 9 * suffixLen) continue;

                int[] candidate = new int[len];
                Array.Copy(digits, candidate, pos);
                candidate[pos] = d;
                DistributeSuffix(candidate, pos + 1, rem);
                return JoinDigits(candidate);
            }
        }

        return null;
    }

    static void DistributeSuffix(int[] arr, int start, int sum)
    {
        for (int i = arr.Length - 1; i >= start; i--)
        {
            int take = Math.Min(9, sum);
            arr[i] = take;
            sum -= take;
        }
    }

    static string FindNextLonger(int oldLen, int targetSum)
    {
        int newLen = oldLen + 1;
        int[] arr = new int[newLen];
        arr[0] = Math.Max(1, targetSum - 9 * (newLen - 1));
        int remaining = targetSum - arr[0];
        DistributeSuffix(arr, 1, remaining);
        return JoinDigits(arr);
    }

    static string JoinDigits(int[] arr)
    {
        return string.Concat(arr);
    }
}
