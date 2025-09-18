using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        string aWord = Console.ReadLine().Trim();
        var font = new Dictionary<char, ulong>
        {
            ['A'] = 0x1818243C42420000UL,
            ['B'] = 0x7844784444780000UL,
            ['C'] = 0x3844808044380000UL,
            ['D'] = 0x7844444444780000UL,
            ['E'] = 0x7C407840407C0000UL,
            ['F'] = 0x7C40784040400000UL,
            ['G'] = 0x3844809C44380000UL,
            ['H'] = 0x42427E4242420000UL,
            ['I'] = 0x3E080808083E0000UL,
            ['J'] = 0x1C04040444380000UL,
            ['K'] = 0x4448507048440000UL,
            ['L'] = 0x40404040407E0000UL,
            ['M'] = 0x4163554941410000UL,
            ['N'] = 0x4262524A46420000UL,
            ['O'] = 0x1C222222221C0000UL,
            ['P'] = 0x7844784040400000UL,
            ['Q'] = 0x1C222222221C0200UL,
            ['R'] = 0x7844785048440000UL,
            ['S'] = 0x1C22100C221C0000UL,
            ['T'] = 0x7F08080808080000UL,
            ['U'] = 0x42424242423C0000UL,
            ['V'] = 0x8142422424180000UL,
            ['W'] = 0x4141495563410000UL,
            ['X'] = 0x4224181824420000UL,
            ['Y'] = 0x4122140808080000UL,
            ['Z'] = 0x7E040810207E0000UL
        };

        var output = new List<string>();

        for (int row = 0; row < 8; row++)
        {
            var sb = new StringBuilder();
            foreach (char ch in aWord)
            {
                if (!font.ContainsKey(ch)) continue;
                ulong bits = font[ch];
                int shift = (7 - row) * 8;
                byte rowBits = (byte)((bits >> shift) & 0xFFUL);
                for (int b = 7; b >= 0; b--)
                    sb.Append(((rowBits >> b) & 1) == 1 ? 'X' : ' ');
            }
            var line = sb.ToString().TrimEnd();
            if (line.Length > 0)
                output.Add(line);
        }

        foreach (var line in output)
            Console.WriteLine(line);
    }
}
