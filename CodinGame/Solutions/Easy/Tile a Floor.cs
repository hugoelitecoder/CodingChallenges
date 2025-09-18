using System;
using System.Collections.Generic;

class Solution
{
    static Dictionary<char, char> lrMirror = new() {
        { '(', ')' }, { ')', '(' }, { '{', '}' }, { '}', '{' },
        { '[', ']' }, { ']', '[' }, { '<', '>' }, { '>', '<' }
    };

    static Dictionary<char, char> tbMirror = new() {
        { '^', 'v' }, { 'v', '^' }, { 'A', 'V' }, { 'V', 'A' },
        { 'w', 'm' }, { 'm', 'w' }, { 'W', 'M' }, { 'M', 'W' },
        { 'u', 'n' }, { 'n', 'u' }
    };

    static char FlipDiag(char ch)
    {
        return ch switch
        {
            '/' => '\\',
            '\\' => '/',
            _ => ch
        };
    }

    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var input = new List<string>();
        for (int i = 0; i < n; i++)
            input.Add(Console.ReadLine());

        int size = 2 * n - 1;
        char[,] tile = new char[size, size];

        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
            {
                char ch = input[r][c];
                tile[r, c] = ch;

                // Top-right (mirror left-right + diagonal)
                char chR = lrMirror.ContainsKey(ch) ? lrMirror[ch] : ch;
                tile[r, size - 1 - c] = FlipDiag(chR);

                // Bottom-left (mirror top-bottom + diagonal)
                char chB = tbMirror.ContainsKey(ch) ? tbMirror[ch] : ch;
                tile[size - 1 - r, c] = FlipDiag(chB);

                // Bottom-right (mirror both sides, no diagonal flip)
                char chBR = ch;
                if (lrMirror.ContainsKey(chBR)) chBR = lrMirror[chBR];
                if (tbMirror.ContainsKey(chBR)) chBR = tbMirror[chBR];
                tile[size - 1 - r, size - 1 - c] = chBR;
            }
        }

        string grout = "+" + new string('-', size) + "+" + new string('-', size) + "+";

        for (int tileBlock = 0; tileBlock < 2; tileBlock++)
        {
            Console.WriteLine(grout);
            for (int row = 0; row < size; row++)
            {
                string line = "|";
                for (int col = 0; col < size; col++) line += tile[row, col];
                line += "|";
                for (int col = 0; col < size; col++) line += tile[row, col];
                line += "|";
                Console.WriteLine(line);
            }
        }
        Console.WriteLine(grout);
    }
}
