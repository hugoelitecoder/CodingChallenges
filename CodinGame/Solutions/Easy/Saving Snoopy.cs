using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var numSwaps = int.Parse(Console.ReadLine());
        var decoder = new Dictionary<string, string>();
        for (var i = 0; i < numSwaps; i++)
        {
            var parts = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            decoder[parts[0]] = parts[2];
        }
        Console.ReadLine();
        var tokens = Console.ReadLine().Split('+');
        var stack = new List<string>();
        var decoded = new StringBuilder();
        foreach (var c in tokens)
        {
            if (c.StartsWith("#"))
            {
                if (c.Length > 1 && int.TryParse(c.Substring(1), out var cnt))
                {
                    for (var j = 0; j < cnt && stack.Count > 0; j++)
                        stack.RemoveAt(stack.Count - 1);
                }
            }
            else if (c == "%")
            {
                var even = stack.Where((_, idx) => idx % 2 == 0).ToList();
                var odd = stack.Where((_, idx) => idx % 2 != 0).ToList();
                stack = even.Concat(odd).ToList();
            }
            else if (c == "*")
            {
                if (stack.Count > 0)
                {
                    var item = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    if (decoder.TryGetValue(item, out var mapped))
                        item = mapped;
                    decoded.Append(item);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(c))
                    stack.Add(c);
            }
        }
        var words = decoded.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var lineSb = new StringBuilder();
        foreach (var word in words)
        {
            if (lineSb.Length == 0)
                lineSb.Append(word);
            else if (lineSb.Length + 1 + word.Length <= 75)
                lineSb.Append(' ').Append(word);
            else
            {
                Console.WriteLine(lineSb.ToString());
                lineSb.Clear();
                lineSb.Append(word);
            }
        }
        if (lineSb.Length > 0)
            Console.WriteLine(lineSb.ToString());
    }
}