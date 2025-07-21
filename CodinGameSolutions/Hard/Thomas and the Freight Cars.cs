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
        var n = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split(' ');
        var weights = new int[n];
        for (var i = 0; i < n; i++)
        {
            weights[i] = int.Parse(inputs[i]);
        }
        
        var solver = new TrainSolver(weights);
        var answer = solver.FindLongestTrainLength();
        
        Console.WriteLine(answer);
    }
}

class TrainSolver
{
    private readonly int[] _weights;
    private readonly int _count;

    public TrainSolver(int[] weights)
    {
        _weights = weights;
        _count = weights.Length;
    }

    public int FindLongestTrainLength()
    {
        if (_count == 0)
        {
            return 0;
        }
        
        var maxLength = 1;
        
        for (var i = 0; i < _count; i++)
        {
            var pivotWeight = _weights[i];
            
            var greater = new List<int>();
            var lesser = new List<int>();
            
            for (var j = i + 1; j < _count; j++)
            {
                var currentWeight = _weights[j];
                if (currentWeight > pivotWeight)
                {
                    greater.Add(currentWeight);
                }
                else
                {
                    lesser.Add(currentWeight);
                }
            }
            
            var lisLength = CalculateLIS(greater);
            var ldsLength = CalculateLDS(lesser);
            
            var currentLength = 1 + lisLength + ldsLength;
            if (currentLength > maxLength)
            {
                maxLength = currentLength;
            }
        }
        
        return maxLength;
    }
    
    private int CalculateLIS(List<int> seq)
    {
        if (seq.Count == 0)
        {
            return 0;
        }
        
        var n = seq.Count;
        var dp = new int[n];
        var maxLen = 0;
        
        for (var i = 0; i < n; i++)
        {
            dp[i] = 1;
            for (var j = 0; j < i; j++)
            {
                if (seq[i] > seq[j])
                {
                    dp[i] = Math.Max(dp[i], 1 + dp[j]);
                }
            }
            maxLen = Math.Max(maxLen, dp[i]);
        }
        
        return maxLen;
    }

    private int CalculateLDS(List<int> seq)
    {
        if (seq.Count == 0)
        {
            return 0;
        }
        
        var n = seq.Count;
        var dp = new int[n];
        var maxLen = 0;
        
        for (var i = 0; i < n; i++)
        {
            dp[i] = 1;
            for (var j = 0; j < i; j++)
            {
                if (seq[i] < seq[j])
                {
                    dp[i] = Math.Max(dp[i], 1 + dp[j]);
                }
            }
            maxLen = Math.Max(maxLen, dp[i]);
        }
        
        return maxLen;
    }
}