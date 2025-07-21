using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main() {
        int lengthOfLine = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());
        var entries = new List<string>();
        for (int i = 0; i < n; i++)
            entries.Add(Console.ReadLine());

        var counters = new int[30];
        foreach (var entry in entries) {
            int level = 0;
            while (level < entry.Length && entry[level] == '>') level++;

            var line = entry.Substring(level).Trim();
            var lastSpace = line.LastIndexOf(' ');
            var title = line.Substring(0, lastSpace);
            var page = line.Substring(lastSpace + 1);

            counters[level]++;
            for (int j = level + 1; j < counters.Length; j++)
                counters[j] = 0;

            var number = counters[level];
            var indent = new string(' ', level * 4);
            var baseText = $"{indent}{number} {title}";
            var dots = new string('.', lengthOfLine - baseText.Length - page.Length);
            Console.WriteLine($"{baseText}{dots}{page}");
        }
    }
}
