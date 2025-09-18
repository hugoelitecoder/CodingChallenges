using System;

class Solution
{
    public static void Main()
    {
        int kr = -1, kc = -1;
        int pr = -1, pc = -1;
        char piece = ' ';

        for (int r = 0; r < 8; r++)
        {
            var cols = Console.ReadLine().Split(' ');
            for (int c = 0; c < 8; c++)
            {
                var ch = cols[c][0];
                if (ch == 'K')
                {
                    kr = r; kc = c;
                }
                else if (ch == 'B' || ch == 'N' || ch == 'R' || ch == 'Q')
                {
                    piece = ch;
                    pr = r; pc = c;
                }
            }
        }

        var dr = Math.Abs(kr - pr);
        var dc = Math.Abs(kc - pc);
        var inCheck = false;

        switch (piece)
        {
            case 'N':
                if ((dr == 2 && dc == 1) || (dr == 1 && dc == 2))
                    inCheck = true;
                break;
            case 'B':
                if (dr == dc)
                    inCheck = true;
                break;
            case 'R':
                if (dr == 0 || dc == 0)
                    inCheck = true;
                break;
            case 'Q':
                if (dr == dc || dr == 0 || dc == 0)
                    inCheck = true;
                break;
        }

        Console.WriteLine(inCheck ? "Check" : "No Check");
    }
}
