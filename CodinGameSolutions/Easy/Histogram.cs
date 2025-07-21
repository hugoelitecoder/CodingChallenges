using System;

class Solution
{
    static void Main()
    {
        var s = Console.ReadLine();

        var counts = new int[26];
        int total = 0;
        foreach (var ch in s)
            if (char.IsLetter(ch))
            {
                int idx = char.ToUpper(ch) - 'A';
                counts[idx]++;
                total++;
            }

        var pctStr = new string[26];
        var barLen = new int[26];
        for (int i = 0; i < 26; i++)
        {
            double pct = total > 0 ? counts[i] * 100.0 / total : 0;
            pctStr[i] = Math.Round(pct, 2).ToString("0.00") + "%";
            barLen[i] = (int)Math.Round(pct, MidpointRounding.AwayFromZero);
        }

        int prev = 0;
        for (int i = 0; i < 26; i++)
        {
            char letter = (char)('A' + i);
            int curr = barLen[i];

            if (curr > 0)
            {
                if (prev > 0)
                    DrawZipper(prev, curr);
                else
                    PrintRun(curr);

                PrintLine(letter, curr, pctStr[i]);
                if (i == 25)
                    PrintRun(curr);
            }
            else
            {
                if (prev > 0)
                    PrintRun(prev);
                else
                    Console.WriteLine("  +");

                PrintLine(letter, 0, pctStr[i]);

                if (i == 25)
                    Console.WriteLine("  +");
            }

            prev = curr;
        }
    }

    static void PrintRun(int len)
    {
        Console.WriteLine("  +" + new string('-', len) + "+");
    }

    static void DrawZipper(int prev, int curr)
    {
        int total = Math.Max(prev, curr);
        var arr = new char[total];
        for (int k = 0; k < total; k++) arr[k] = '-';
        int pos = Math.Min(prev, curr);
        if (pos < total) arr[pos] = '+';
        Console.WriteLine("  +" + new string(arr) + "+");
    }

    static void PrintLine(char letter, int len, string pct)
    {
        Console.Write(letter);
        Console.Write(" |");
        if (len > 0)
            Console.Write(new string(' ', len) + "|");
        Console.WriteLine(pct);
    }
}
