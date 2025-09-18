using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        int total = int.Parse(Console.ReadLine().Trim());
        var symbols = new List<string>(total);
        for (int i = 0; i < total; i++)
        {
            symbols.Add(Console.ReadLine());
        }

        var binarySeq = Enumerable.Range(0, 1 << total)
                                  .Select(num => Convert.ToString(num, 2)
                                                            .PadLeft(total, '0'))
                                  .ToList();

        var combinations = new List<string>();

        for (int i = 1; i < total; i++)
        {
            var zeroSym = symbols[i - 1];
            var oneSym  = symbols[i];

            foreach (var bin in binarySeq)
            {
                var sb = new StringBuilder();
                foreach (var bit in bin)
                {
                    sb.Append(bit == '0' ? zeroSym : oneSym);
                }
                combinations.Add(sb.ToString());
            }

            if (i == 1)
            {
                binarySeq = binarySeq.Skip(1).ToList();
            }
        }

        foreach (var combo in combinations)
        {
            Console.WriteLine(combo);
        }
    }
}
