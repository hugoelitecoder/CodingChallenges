using System;

class Solution
{
    static void Main()
    {
        int H = int.Parse(Console.ReadLine());
        var rows = new string[H];
        for (int i = 0; i < H; i++)
            rows[i] = Console.ReadLine();
        Console.WriteLine(IsStable(rows) ? "STABLE" : "UNSTABLE");
    }

    static bool IsStable(string[] rows)
    {
        string prev = null;
        foreach (var row in rows)
        {
            if (row.Contains("/.") || row.Contains(@".\") ||
                row.Contains("//") || row.Contains(@"\\"))
                return false;

            if (prev != null)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    char a = prev[i], b = row[i];
                    if ((a == '/' && b != '\\') || 
                       (a == '\\' && b != '/')) 
                       return false;
                }
            }

            prev = row;
        }
        return true;
    }
}
