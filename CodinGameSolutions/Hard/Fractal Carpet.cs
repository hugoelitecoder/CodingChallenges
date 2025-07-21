using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int L = int.Parse(Console.ReadLine());
        string[] inputs = Console.ReadLine().Split(' ');
        
        long x1 = long.Parse(inputs[0]);
        long y1 = long.Parse(inputs[1]);
        long x2 = long.Parse(inputs[2]);
        long y2 = long.Parse(inputs[3]);
        
        int patchWidth = (int)(x2 - x1 + 1);

        for (long y = y1; y <= y2; y++)
        {
            StringBuilder rowBuilder = new StringBuilder(patchWidth);
            for (long x = x1; x <= x2; x++)
            {
                rowBuilder.Append(GetChar(L, x, y));
            }
            Console.WriteLine(rowBuilder.ToString());
        }
    }

    static readonly long[] powersOf3 = new long[40];

    static Solution()
    {
        powersOf3[0] = 1;
        for (int i = 1; i < 40; i++)
        {
            powersOf3[i] = powersOf3[i - 1] * 3;
        }
    }

    static char GetChar(int level, long x, long y)
    {
        for (int l = level; l > 0; l--)
        {
            long subSize = powersOf3[l - 1];
            if ((x / subSize) == 1 && (y / subSize) == 1)
            {
                return '+';
            }
            x %= subSize;
            y %= subSize;
        }
        return '0';
    }
}