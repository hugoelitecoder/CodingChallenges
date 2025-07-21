using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        List<int> strengths = new List<int>();
        for (int i = 0; i < N; i++)
        {
            int pi = int.Parse(Console.ReadLine());
            strengths.Add(pi);
            Console.Error.WriteLine(pi);
        }

        int D = int.MaxValue;
        var Di = (from s in strengths orderby s descending select s).Aggregate((x,y) => { var l = x - y; if (D > l) { D = l; } return y; });
        Console.WriteLine(D);
    }
}