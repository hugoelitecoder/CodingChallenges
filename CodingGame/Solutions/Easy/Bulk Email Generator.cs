using System;
using System.Linq;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var lines = new string[n];
        int i=0;
        for (i = 0; i < n; i++)
            lines[i] = Console.ReadLine();
        var template = string.Join("\n", lines);
        var result = new StringBuilder();
        var choiceIndex = 0;
        i = 0;
        while (i < template.Length)
        {
            if (template[i] == '(')
            {
                var end = template.IndexOf(')', i + 1);
                var content = template.Substring(i + 1, end - i - 1);
                var parts = content.Split('|');
                var pick = parts[choiceIndex % parts.Length];
                choiceIndex++;
                result.Append(pick);
                i = end + 1;
            }
            else
            {
                result.Append(template[i]);
                i++;
            }
        }
        Console.Write(result.ToString());
    }
}