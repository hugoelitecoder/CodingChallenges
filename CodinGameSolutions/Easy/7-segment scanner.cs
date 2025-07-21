using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var line0 = Console.ReadLine();
        var line1 = Console.ReadLine();
        var line2 = Console.ReadLine();
        var digitCount = line0.Length / 3;

        var map = new Dictionary<string, char>
        {
            { " _ " + "| |" + "|_|", '0' },
            { "   " + "  |" + "  |", '1' },
            { " _ " + " _|" + "|_ ", '2' },
            { " _ " + " _|" + " _|", '3' },
            { "   " + "|_|" + "  |", '4' },
            { " _ " + "|_ " + " _|", '5' },
            { " _ " + "|_ " + "|_|", '6' },
            { " _ " + "  |" + "  |", '7' },
            { " _ " + "|_|" + "|_|", '8' },
            { " _ " + "|_|" + " _|", '9' },
        };

        var result = new char[digitCount];
        for (var i = 0; i < digitCount; i++)
        {
            var key = line0.Substring(i * 3, 3) +
                      line1.Substring(i * 3, 3) +
                      line2.Substring(i * 3, 3);

            result[i] = map.TryGetValue(key, out var digit) ? digit : '?';
        }

        Console.WriteLine(new string(result));
    }
}
