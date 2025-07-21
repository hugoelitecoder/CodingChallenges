using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        string input = string.Join("\n",
            Enumerable.Range(0, n)
                      .Select(_ => Console.ReadLine())
        );

        Console.WriteLine(MinifyCgs(input));
    }

    static string MinifyCgs(string input)
    {
        var varMap = new Dictionary<string, char>();
        char nextVar = 'a';
        var sb = new StringBuilder();
        bool inQuote = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '\'')
            {
                inQuote = !inQuote;
                sb.Append(c);
                continue;
            }

            if (inQuote)
            {
                sb.Append(c);
                continue;
            }

            if (char.IsWhiteSpace(c))
                continue;

            if (c == '$')
            {
                int end = input.IndexOf('$', i + 1);
                if (end > i)
                {
                    string name = input.Substring(i + 1, end - i - 1);
                    if (!varMap.TryGetValue(name, out var shortName))
                        varMap[name] = shortName = nextVar++;
                    sb.Append('$').Append(shortName).Append('$');
                    i = end;
                }
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
