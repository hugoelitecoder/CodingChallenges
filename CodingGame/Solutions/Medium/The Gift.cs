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
        int N = int.Parse(Console.ReadLine());
        int C = int.Parse(Console.ReadLine());
        Console.Error.WriteLine($"TARGET={C} with {N} people");
        int[] MONEY = new int[N];
        for (int i = 0; i < N; i++)
        {
            int B = int.Parse(Console.ReadLine());
            MONEY[i] = B;
        }
        MONEY = MONEY.OrderBy(e=>e).ToArray();
        if (MONEY.Sum() < C){
            Console.WriteLine("IMPOSSIBLE");
        } else {
            int contribution=0;
            int contributee=0;
            while(contribution < C && contributee < N){
                var max_contribution = Convert.ToInt32((C-contribution)/(N-contributee));
                var actual_contribution = MONEY[contributee] < max_contribution ? MONEY[contributee] : max_contribution;
                contribution += actual_contribution;
                Console.WriteLine(actual_contribution);
                contributee++;
            }
            
        }
        
    }
}