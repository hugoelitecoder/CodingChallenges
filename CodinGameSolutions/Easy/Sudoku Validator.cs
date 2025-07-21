using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int[,] grid = new int[9, 9];
        for (int i = 0; i < 9; i++)
        {
            var inputs = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
            for (int j = 0; j < 9; j++)
                grid[i, j] = inputs[j];
        }

        bool valid = true;
        for (int i = 0; i < 9; i++)
        {
            if (!IsUnique(Enumerable.Range(0, 9).Select(j => grid[i, j])) ||
                !IsUnique(Enumerable.Range(0, 9).Select(j => grid[j, i])))  
            {
                valid = false;
                break;
            }
        }

        for (int r = 0; r < 9; r += 3)
        {
            for (int c = 0; c < 9; c += 3)
            {
                var block = Enumerable.Range(0, 3).SelectMany(dr =>
                    Enumerable.Range(0, 3).Select(dc => grid[r + dr, c + dc])
                );
                if (!IsUnique(block))
                {
                    valid = false;
                    break;
                }
            }
        }
        Console.WriteLine(valid.ToString().ToLower());
    }

    static bool IsUnique(IEnumerable<int> values)
    {
        return values.OrderBy(x => x).SequenceEqual(Enumerable.Range(1, 9));
    }
}
