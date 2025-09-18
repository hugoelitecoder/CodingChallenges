using System;
using System.Collections.Generic;

class Solution
{
    class Node
    {
        public Dictionary<char, Node> Children = new();
        public char? Char = null;
    }

    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var root = new Node();

        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            string code = parts[0];
            char ch = (char)int.Parse(parts[1]);
            var node = root;
            foreach (char bit in code)
            {
                if (!node.Children.ContainsKey(bit))
                    node.Children[bit] = new Node();
                node = node.Children[bit];
            }
            node.Char = ch;
        }

        string s = Console.ReadLine();
        string output = "";
        var current = root;
        int start = 0;

        for (int i = 0; i < s.Length; i++)
        {
            char bit = s[i];
            if (!current.Children.ContainsKey(bit))
            {
                Console.WriteLine($"DECODE FAIL AT INDEX {start}");
                return;
            }
            current = current.Children[bit];
            if (current.Char.HasValue)
            {
                output += current.Char.Value;
                current = root;
                start = i + 1;
            }
        }

        if (current != root)
            Console.WriteLine($"DECODE FAIL AT INDEX {start}");
        else
            Console.WriteLine(output);
    }
}
