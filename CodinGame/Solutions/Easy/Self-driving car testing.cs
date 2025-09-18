using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var initialParts = Console.ReadLine().Split(';');
        int pos = int.Parse(initialParts[0]);  // 1-based index
        var commandTokens = initialParts.Skip(1);

        var moves = new List<char>();
        foreach (var token in commandTokens)
        {
            char dir = token[^1];
            int count = int.Parse(token[..^1]);
            for (int i = 0; i < count; i++)
                moves.Add(dir);
        }

        var road = new List<string>();
        for (int i = 0; i < N; i++)
        {
            var line = Console.ReadLine();
            var sem = line.IndexOf(';');
            int repeat = int.Parse(line[..sem]);
            string pattern = line[(sem + 1)..];
            for (int r = 0; r < repeat; r++)
                road.Add(pattern);
        }

        for (int i = 0; i < moves.Count; i++)
        {
            switch (moves[i])
            {
                case 'L': pos--; break;
                case 'R': pos++; break;
                case 'S': break;
            }

            var rowChars = road[i].ToCharArray();
            rowChars[pos - 1] = '#';
            Console.WriteLine(new string(rowChars));
        }
    }
}
