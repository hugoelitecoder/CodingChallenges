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
        int n = int.Parse(Console.ReadLine());
        string[] inputs = Console.ReadLine().Split(' ');
        int maxvalue=int.MinValue;
        int lossvalue=int.MinValue;
        int maxlossvalue=int.MinValue;
        for (int i = 0; i < n; i++)
        {
            int v = int.Parse(inputs[i]);
            if (v > maxvalue){
                maxvalue = v;
            }
            if (maxvalue-v > lossvalue){
                lossvalue = maxvalue-v;
            }
            if (lossvalue > maxlossvalue){
                maxlossvalue = lossvalue;
            }
        }
        Console.WriteLine(-maxlossvalue);
    }
}