using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var a = int.Parse(Console.ReadLine());
        
        var solver = new ArithmeticSolver(n, a);
        var answer = solver.FindMinOperations();

        Console.WriteLine(answer);
    }
}

class ArithmeticSolver
{
    private readonly int _targetN;
    private readonly int _baseA;
    private const int MaxOperations = 12;
    private readonly List<HashSet<long>> _dp;

    public ArithmeticSolver(int n, int a)
    {
        _targetN = n;
        _baseA = a;
        _dp = new List<HashSet<long>> { new HashSet<long>() };
    }

    public int FindMinOperations()
    {
        var set1 = new HashSet<long> { _baseA };
        _dp.Add(set1);
        if (set1.Contains(_targetN))
        {
            return 1;
        }

        for (var k = 2; k <= MaxOperations; k++)
        {
            var currentSet = new HashSet<long>();
            for (var i = 1; i <= k / 2; i++)
            {
                var j = k - i;
                CombineSets(currentSet, _dp[i], _dp[j]);
            }
            
            _dp.Add(currentSet);

            if (currentSet.Contains(_targetN))
            {
                return k;
            }
        }
        
        return -1;
    }

    private void CombineSets(HashSet<long> resultSet, HashSet<long> set1, HashSet<long> set2)
    {
        if (set1 == set2)
        {
            var list = set1.ToList();
            for (var m = 0; m < list.Count; m++)
            {
                for (var l = m; l < list.Count; l++)
                {
                     AddAllCombinations(resultSet, list[m], list[l]);
                }
            }
        }
        else
        {
            foreach (var num1 in set1)
            {
                foreach (var num2 in set2)
                {
                    AddAllCombinations(resultSet, num1, num2);
                }
            }
        }
    }

    private void AddAllCombinations(HashSet<long> set, long n1, long n2)
    {
        set.Add(n1 + n2);
        set.Add(n1 * n2);
        set.Add(n1 - n2);
        set.Add(n2 - n1);
        if (n2 != 0 && n1 % n2 == 0) set.Add(n1 / n2);
        if (n1 != 0 && n2 % n1 == 0) set.Add(n2 / n1);
    }
}
