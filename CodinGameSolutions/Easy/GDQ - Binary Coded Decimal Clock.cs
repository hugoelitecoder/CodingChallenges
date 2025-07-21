using System;

class Solution
{
    static void Main()
    {
        string input = Console.ReadLine();
        string[] parts = input.Split(':');
        char[] digits = new char[]
        {
            parts[0][1], // H digit
            parts[1][0], parts[1][1], // M digits
            parts[2][0], parts[2][1], // S digits
            parts[3][0] // f digit
        };
        var rows = new string[4];
        foreach (char digit in digits)
        {
            int value = digit - '0';
            string bits = Convert.ToString(value, 2).PadLeft(4, '0');

            for (int i = 0; i < 4; i++)
                rows[i] += "|" + (bits[i] == '1' ? "#####" : "_____");
        }
        foreach (var row in rows)
            Console.WriteLine(row + "|");
    }
}
