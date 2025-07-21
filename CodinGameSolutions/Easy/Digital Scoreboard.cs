using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    // 5‑row representations for digits 0–9:
    static readonly Dictionary<string,int> _digitMap = new Dictionary<string,int>
    {
        { String.Join("\n", new[]{" ~~~ ","|   |","     ","|   |"," ~~~ "}), 0 },
        { String.Join("\n", new[]{"     ","    |","     ","    |","     "}), 1 },
        { String.Join("\n", new[]{" ~~~ ","    |"," ~~~ ","|    "," ~~~ "}), 2 },
        { String.Join("\n", new[]{" ~~~ ","    |"," ~~~ ","    |"," ~~~ "}), 3 },
        { String.Join("\n", new[]{"     ","|   |"," ~~~ ","    |","     "}), 4 },
        { String.Join("\n", new[]{" ~~~ ","|    "," ~~~ ","    |"," ~~~ "}), 5 },
        { String.Join("\n", new[]{" ~~~ ","|    "," ~~~ ","|   |"," ~~~ "}), 6 },
        { String.Join("\n", new[]{" ~~~ ","    |","     ","    |","     "}), 7 },
        { String.Join("\n", new[]{" ~~~ ","|   |"," ~~~ ","|   |"," ~~~ "}), 8 },
        { String.Join("\n", new[]{" ~~~ ","|   |"," ~~~ ","    |"," ~~~ "}), 9 },
    };

    static void Main()
    {
        var all = Console.In
            .ReadToEnd()
            .Split(new[]{ "\r\n","\n" }, StringSplitOptions.None)
            .Where(l => l.Length > 0).ToList();

        var original = all.GetRange(0, 7).ToArray();
        var subtract = all.GetRange(8, 7).ToArray();
        var add      = all.GetRange(16,7).ToArray();

        // 1) Grab only the 5 “content” rows
        var A = original.Skip(1).Take(5).ToArray();
        var S = subtract.Skip(1).Take(5).ToArray();
        var B = add     .Skip(1).Take(5).ToArray();

        // 2) Apply off/on changes, but only for '|' and '~'
        var F = new string[5];
        for(int i=0; i<5; i++)
        {
            var row = A[i].ToCharArray();
            for(int j=0; j<row.Length; j++)
            {
                if (S[i][j] == '|' || S[i][j] == '~')    // turn off
                    row[j] = ' ';
                if (B[i][j] == '|' || B[i][j] == '~')    // turn on
                    row[j] = B[i][j];
            }
            F[i] = new string(row);
        }

        // 3) Extract the 5×5 block for each digit
        int left  = Extract(F,  2);
        int right = Extract(F, 10);

        if (left < 0 || right < 0)
            Console.WriteLine("Invalid Display");
        else
            Console.WriteLine($"{left}{right}");
    }

    static int Extract(string[] disp, int startCol)
    {
        // Take a 5×5 slice, normalize non‑segments to spaces, and look up
        var block = new string[5];
        for(int i=0; i<5; i++)
        {
            var slice = disp[i].Substring(startCol, 5)
                               .Select(c => (c=='|'||c=='~') ? c : ' ')
                               .ToArray();
            block[i] = new string(slice);
        }
        var key = string.Join("\n", block);
        return _digitMap.TryGetValue(key, out var d) ? d : -1;
    }
}
