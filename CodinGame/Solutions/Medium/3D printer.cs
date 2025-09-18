using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int width = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());
        int length = int.Parse(Console.ReadLine());

        var pic1 = new List<string>();
        var pic2 = new List<string>();
        var pic3 = new List<string>();

        for (int i = 0; i < height; i++)
            pic1.Add(Console.ReadLine());

        for (int i = 0; i < height; i++)
        {
            var line = Console.ReadLine();
            var chars = line.ToCharArray();
            Array.Reverse(chars);
            pic2.Add(new string(chars));
        }

        for (int i = 0; i < length; i++)
            pic3.Add(Console.ReadLine());

        var result = new List<string[]>();
        int sss = length;

        for (int i = 0; i < height; i++)
        {
            string front = pic1[i];
            string right = pic2[i];
            string[] layer = new string[sss];

            for (int ij = 0; ij < sss; ij++)
            {
                var row = new char[width];
                for (int j = 0; j < width; j++)
                {
                    bool a = (j < front.Length && front[j] == '#');
                    bool b = (ij < right.Length && right[ij] == '#');
                    bool c = (ij < pic3.Count && j < pic3[ij].Length && pic3[ij][j] == '#');
                    row[j] = (a && b && c) ? '#' : ' ';
                }
                layer[ij] = new string(row);
            }
            result.Add(layer);
        }

        for (int i = 0; i < result.Count; i++)
        {
            var layer = result[result.Count - 1 - i];
            foreach (var line in layer)
                Console.WriteLine(line.TrimEnd(' '));
            Console.WriteLine("--");
        }
    }
}
