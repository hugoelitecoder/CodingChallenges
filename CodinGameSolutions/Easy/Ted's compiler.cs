using System;

class Solution
{
    static void Main(string[] args)
    {
        var line = Console.ReadLine();
        var balance = 0;
        var lastValid = 0;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '<') balance++;
            else if (c == '>') balance--;
            if (balance < 0) break;
            if (balance == 0) lastValid = i + 1;
        }
        Console.WriteLine(lastValid);
    }
}
