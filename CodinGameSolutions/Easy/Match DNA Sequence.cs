using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var delta = int.Parse(Console.ReadLine());
        var gene = Console.ReadLine();
        var n = int.Parse(Console.ReadLine());
        var geneLen = gene.Length;
        for (var ci = 0; ci < n; ci++)
        {
            var chr = Console.ReadLine();
            var chrLen = chr.Length;
            for (var start = 0; start < chrLen; start++)
            {
                var mismatches = 0;
                for (var i = 0; i < geneLen; i++)
                {
                    var pos = start + i;
                    if (pos < chrLen)
                    {
                        if (chr[pos] != gene[i])
                            mismatches++;
                    }
                    else
                    {
                        mismatches++;
                    }
                    if (mismatches > delta)
                        break;
                }
                if (mismatches <= delta)
                {
                    Console.WriteLine($"{ci} {start} {mismatches}");
                    return;
                }
            }
        }
        Console.WriteLine("NONE");
    }
}