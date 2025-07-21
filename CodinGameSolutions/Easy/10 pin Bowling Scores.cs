using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0)
        {
            var frames = Console.ReadLine().Split(' ');
            var rolls = new List<int>();
            var frameStart = new int[11]; 
            int rollIdx = 0;
            for (int f = 1; f <= 10; f++)
            {
                frameStart[f] = rollIdx;
                var s = frames[f - 1];
                if (f < 10)
                {
                    if (s == "X")
                    {
                        rolls.Add(10);
                        rollIdx++;
                    }
                    else
                    {
                        int first = CharToPins(s[0]);
                        int second = s[1] == '/' ? 10 - first : CharToPins(s[1]);
                        rolls.Add(first);
                        rolls.Add(second);
                        rollIdx += 2;
                    }
                }
                else
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        char c = s[i];
                        int pins;
                        if (c == 'X')
                            pins = 10;
                        else if (c == '/')
                            pins = 10 - rolls[rolls.Count - 1];
                        else
                            pins = CharToPins(c);
                        rolls.Add(pins);
                        rollIdx++;
                    }
                }
            }

            var cumScores = new int[10];
            int cumulative = 0;
            for (int f = 1; f <= 10; f++)
            {
                int idx = frameStart[f];
                int frameScore = 0;
                if (f < 10)
                {
                    // Strike?
                    if (rolls[idx] == 10)
                    {
                        frameScore = 10 + rolls[idx + 1] + rolls[idx + 2];
                    }
                    else
                    {
                        int first = rolls[idx];
                        int second = rolls[idx + 1];
                        if (first + second == 10)
                        {
                            // Spare
                            frameScore = 10 + rolls[idx + 2];
                        }
                        else
                        {
                            // Open
                            frameScore = first + second;
                        }
                    }
                }
                else
                {
                    // 10th frame: sum all its rolls
                    int nextStart = rolls.Count;
                    frameScore = rolls.Skip(idx).Sum();
                }

                cumulative += frameScore;
                cumScores[f - 1] = cumulative;
            }

            Console.WriteLine(string.Join(" ", cumScores));
        }
    }

    static int CharToPins(char c)
    {
        if (c == '-') return 0;
        return c - '0';
    }
}
