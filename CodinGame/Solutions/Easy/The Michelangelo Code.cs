using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        var raw = Console.ReadLine();
        var letters = new List<char>(raw.Length);
        foreach (var ch in raw)
            if (char.IsLetter(ch))
                letters.Add(char.ToLowerInvariant(ch));
        int L = letters.Count;

        var posMap = new Dictionary<char, List<int>>();
        for (int i = 0; i < L; i++)
        {
            var ch = letters[i];
            if (!posMap.ContainsKey(ch))
                posMap[ch] = new List<int>();
            posMap[ch].Add(i);
        }

        int N = int.Parse(Console.ReadLine());
        var words = new List<string>(N);
        for (int i = 0; i < N; i++)
            words.Add(Console.ReadLine().Trim().ToLowerInvariant());

        words.Sort((a, b) => b.Length.CompareTo(a.Length));
        int bestStart = -1, bestStep = -1, bestLen = 0;

        foreach (var word in words)
        {
            int m = word.Length;
            if (m == 0 || m > L) 
                continue;

            if (m == 1)
            {
                char letter = word[0];
                if (!posMap.TryGetValue(letter, out var positions) || positions.Count == 0)
                    continue;
                bestStart = positions[0];
                bestStep = 1;
                bestLen = 1;
                break;
            }

            char firstLetter = word[0];
            char secondLetter = word[1];
            if (!posMap.TryGetValue(firstLetter, out var startPositions) ||
                !posMap.TryGetValue(secondLetter, out var secondPositions))
                continue;

            bool found = false;
            foreach (var start in startPositions)
            {
                foreach (var second in secondPositions)
                {
                    if (second <= start) 
                        continue;
                    int step = second - start;
                    int endPos = start + (m - 1) * step;
                    if (endPos >= L) 
                        break;

                    bool match = true;
                    for (int k = 2; k < m; k++)
                    {
                        if (letters[start + k * step] != word[k])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        bestStart = start;
                        bestStep = step;
                        bestLen = m;
                        found = true;
                        break;
                    }
                }
                if (found) 
                    break;
            }
            if (found) 
                break;
        }

        int endIndex = bestStart + (bestLen - 1) * bestStep;
        var sb = new StringBuilder();
        for (int i = bestStart; i <= endIndex; i++)
        {
            int offset = i - bestStart;
            if (bestLen > 0 && offset % bestStep == 0 && offset / bestStep < bestLen)
                sb.Append(char.ToUpperInvariant(letters[i]));
            else
                sb.Append(letters[i]);
        }

        Console.WriteLine(sb.ToString());
    }
}
