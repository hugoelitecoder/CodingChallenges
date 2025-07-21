using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int total=0;
        List<string> lookups = new List<string>();
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            string telephone = Console.ReadLine();
            int max=0;
            foreach(var number in lookups){
                int c=0;
                while(c <number.Length && c <telephone.Length && number[c] == telephone[c] ){ c++;}
                if (c>max) max=c;
            }
            total+=telephone.Length-max;
            lookups.Add(telephone);
        }

        Console.WriteLine($"{total}");
    }
}