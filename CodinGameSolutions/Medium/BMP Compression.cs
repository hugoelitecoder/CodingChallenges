using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    static char WHITE = '.';
    static char BLACK = '#';

    static void Main()
    {
        var headerParts = Console.ReadLine().Split();
        char mode = headerParts[0][0];
        int cols = int.Parse(headerParts[1]);
        int rows = int.Parse(headerParts[2]);
        int ln = int.Parse(Console.ReadLine());

        if (mode == 'B')
        {
            var bitmap = new char[rows][];
            for (int r = 0; r < rows; r++)
                bitmap[r] = Console.ReadLine().ToCharArray();

            var sb = new StringBuilder();
            EncodeRegion(bitmap, 0, 0, rows, cols, sb);
            var data = sb.ToString();

            Console.WriteLine($"C {cols} {rows}");
            int lines = (data.Length + 49) / 50;
            Console.WriteLine(lines);
            for (int i = 0; i < lines; i++)
                Console.WriteLine(data.Substring(i * 50, Math.Min(50, data.Length - i * 50)));
        }
        else
        {
            var all = new StringBuilder();
            for (int i = 0; i < ln; i++)
                all.Append(Console.ReadLine());
            string encoded = all.ToString();

            var bitmap = new char[rows][];
            for (int r = 0; r < rows; r++)
                bitmap[r] = Enumerable.Repeat(WHITE, cols).ToArray();

            int idx = 0;
            DecodeRegion(bitmap, 0, 0, rows, cols, encoded, ref idx);

            Console.WriteLine($"B {cols} {rows}");
            Console.WriteLine(rows);
            for (int r = 0; r < rows; r++)
                Console.WriteLine(new string(bitmap[r]));
        }
    }

    static void EncodeRegion(char[][] bmp, int r0, int c0, int h, int w, StringBuilder sb)
    {
        if (h <= 0 || w <= 0) return;
        bool hasWhite = false, hasBlack = false;
        for (int i = r0; i < r0 + h && !(hasWhite && hasBlack); i++)
            for (int j = c0; j < c0 + w && !(hasWhite && hasBlack); j++)
                if (bmp[i][j] == WHITE) hasWhite = true;
                else hasBlack = true;

        if (!hasBlack)
        {
            sb.Append('0');
        }
        else if (!hasWhite)
        {
            sb.Append('1');
        }
        else
        {
            sb.Append('+');
            int h1 = (h + 1) / 2;
            int w1 = (w + 1) / 2;
            EncodeRegion(bmp, r0, c0, h1, w1, sb);
            EncodeRegion(bmp, r0, c0 + w1, h1, w - w1, sb);
            EncodeRegion(bmp, r0 + h1, c0, h - h1, w1, sb);
            EncodeRegion(bmp, r0 + h1, c0 + w1, h - h1, w - w1, sb);
        }
    }

    static void DecodeRegion(char[][] bmp, int r0, int c0, int h, int w, string s, ref int idx)
    {
        if (h <= 0 || w <= 0 || idx >= s.Length) return;
        char c = s[idx++];
        if (c == '0' || c == '1')
        {
            char fill = (c == '0') ? WHITE : BLACK;
            for (int i = r0; i < r0 + h; i++)
                for (int j = c0; j < c0 + w; j++)
                    bmp[i][j] = fill;
        }
        else // '+'
        {
            int h1 = (h + 1) / 2;
            int w1 = (w + 1) / 2;
            DecodeRegion(bmp, r0, c0, h1, w1, s, ref idx);
            DecodeRegion(bmp, r0, c0 + w1, h1, w - w1, s, ref idx);
            DecodeRegion(bmp, r0 + h1, c0, h - h1, w1, s, ref idx);
            DecodeRegion(bmp, r0 + h1, c0 + w1, h - h1, w - w1, s, ref idx);
        }
    }
}