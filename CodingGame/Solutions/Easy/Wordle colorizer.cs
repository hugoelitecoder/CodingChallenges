using System;

class Solution
{
    public static void Main()
    {
        var answer = Console.ReadLine().Trim();
        var N = int.Parse(Console.ReadLine().Trim());
        for (var t = 0; t < N; t++)
        {
            var attempt = Console.ReadLine().Trim();
            var result = new char[5];
            var freq = new int[26];
            foreach (var ch in answer)
                freq[ch - 'A']++;

            // First pass: correct positions
            for (var i = 0; i < 5; i++)
                if (attempt[i] == answer[i])
                {
                    result[i] = '#';
                    freq[attempt[i] - 'A']--;
                }

            // Second pass: misplaced or absent
            for (var i = 0; i < 5; i++)
            {
                if (result[i] != '\0') continue;
                var idx = attempt[i] - 'A';
                if (freq[idx] > 0)
                {
                    result[i] = 'O';
                    freq[idx]--;
                }
                else result[i] = 'X';
            }

            Console.WriteLine(new string(result));
        }
    }
}
