using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        string[] lines = new string[5];
        for (int i = 0; i < 5; i++)
            lines[i] = Console.ReadLine();

        var recognizer = new DigitRecognizer();
        int width = lines[0].Length;
        for (int col = 0; col + 5 <= width; col += 6)
        {
            var block = recognizer.GetBlock(lines, col);
            int digit = recognizer.Recognize(block);
            Console.Write(digit != -1 ? digit.ToString() : "?");
        }
        Console.WriteLine();
    }
}

class DigitRecognizer
{
    static readonly List<List<string>> digitPatterns = new List<List<string>>
    {
        new List<string>{ "*000*", "0***0", "0***0", "0***0", "*000*" }, // 0
        new List<string>{ "*****", "*****", "00000", "*****", "*****" }, // 1
        new List<string>{ "00000", "*****", "*****", "*****", "00000" }, // 2
        new List<string>{ "00000", "*****", "*000*", "*****", "00000" }, // 3
        new List<string>{ "00000", "0*0*0", "00*00", "0***0", "00000" }, // 4
        new List<string>{ "00000", "**0**", "*0000", "**0*0", "00000" }, // 5
        new List<string>{ "**0**", "**0**", "00000", "*0*0*", "0***0" }, // 6
        new List<string>{ "**0**", "**0**", "00000", "**0**", "**000" }, // 7
        new List<string>{ "*0*0*", "*0*0*", "*0*0*", "*0*0*", "0***0" }, // 8
        new List<string>{ "**0**", "**0**", "0000*", "*0*0*", "0**00" }  // 9
    };

    public int Recognize(List<string> block)
    {
        for (int d = 0; d < 10; d++)
            if (digitPatterns[d].SequenceEqual(block))
                return d;
        return -1;
    }

    public List<string> GetBlock(string[] lines, int col)
    {
        var block = new List<string>();
        for (int i = 0; i < 5; i++)
            block.Add(lines[i].Substring(col, 5));
        return block;
    }
}