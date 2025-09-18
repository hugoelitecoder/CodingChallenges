using System;
using System.IO;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        
        bool[] isSquareUpToN = new bool[n + 1];
        for (int i = 0; i * i <= n; i++)
            isSquareUpToN[i * i] = true;
        
        int tMax = (int)(9 * Math.Sqrt(n)) + 1;
        bool[] isSquareT = new bool[tMax + 1];
        for (int i = 0; i * i <= tMax; i++)
            isSquareT[i * i] = true;
        
        int count = 0;
        int cMax = (int)Math.Sqrt(n);
        
        for (int c = 0; c <= cMax; c++)
        {
            int c2 = c * c;
            int remAfterC = n - c2;
            int dMax = (int)Math.Sqrt(remAfterC);
            
            for (int d = 0; d <= dMax; d++)
            {
                int cd2 = c2 + d * d;
                int remAfterCD = n - cd2;
                
                int T0 = 3 * c + 5 * d;
                int bMax = (int)Math.Sqrt(remAfterCD);
                for (int b = 0; b <= bMax; b++)
                {
                    int sumSqBCD = cd2 + b * b;
                    if (!isSquareUpToN[n - sumSqBCD]) 
                        continue;
                    
                    int T = b + T0;
                    if (T <= tMax && isSquareT[T])
                    {
                        count++;
                    }
                }
            }
        }
        
        Console.WriteLine(count);
    }
}
