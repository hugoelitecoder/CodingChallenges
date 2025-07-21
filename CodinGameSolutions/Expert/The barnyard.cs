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
        var species = Console.ReadLine().Split(' ');
        
        var charNames = new List<string>(n);
        var totals = new Dictionary<string, int>(n);
        for (var i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var thing = inputs[0];
            var number = int.Parse(inputs[1]);
            charNames.Add(thing);
            totals[thing] = number;
        }

        var matrix = new double[n, n + 1];
        for (var i = 0; i < n; i++)
        {
            var charName = charNames[i];
            for (var j = 0; j < n; j++)
            {
                var speciesName = species[j];
                matrix[i, j] = AnimalData.Characteristics[speciesName][charName];
            }
            matrix[i, n] = totals[charName];
        }

        var solver = new BarnyardSolver();
        var solution = solver.Solve(matrix);

        for (var i = 0; i < n; i++)
        {
            var speciesName = species[i];
            var count = (long)Math.Round(solution[i]);
            Console.WriteLine($"{speciesName} {count}");
        }
    }
}

static class AnimalData
{
    public static readonly Dictionary<string, Dictionary<string, int>> Characteristics = new Dictionary<string, Dictionary<string, int>>
    {
        ["Rabbits"] = new Dictionary<string, int> { {"Heads", 1}, {"Horns", 0}, {"Legs", 4}, {"Wings", 0}, {"Eyes", 2} },
        ["Chickens"] = new Dictionary<string, int> { {"Heads", 1}, {"Horns", 0}, {"Legs", 2}, {"Wings", 2}, {"Eyes", 2} },
        ["Cows"] = new Dictionary<string, int> { {"Heads", 1}, {"Horns", 2}, {"Legs", 4}, {"Wings", 0}, {"Eyes", 2} },
        ["Pegasi"] = new Dictionary<string, int> { {"Heads", 1}, {"Horns", 0}, {"Legs", 4}, {"Wings", 2}, {"Eyes", 2} },
        ["Demons"] = new Dictionary<string, int> { {"Heads", 1}, {"Horns", 4}, {"Legs", 4}, {"Wings", 2}, {"Eyes", 4} }
    };
}

class BarnyardSolver
{
    public double[] Solve(double[,] matrix)
    {
        var n = matrix.GetLength(0);
        for (var p = 0; p < n; p++)
        {
            var max = p;
            for (var i = p + 1; i < n; i++)
            {
                if (Math.Abs(matrix[i, p]) > Math.Abs(matrix[max, p]))
                {
                    max = i;
                }
            }
            for (var j = p; j < n + 1; j++)
            {
                var temp = matrix[p, j];
                matrix[p, j] = matrix[max, j];
                matrix[max, j] = temp;
            }
            for (var i = p + 1; i < n; i++)
            {
                var factor = matrix[i, p] / matrix[p, p];
                for (var j = p; j < n + 1; j++)
                {
                    matrix[i, j] -= factor * matrix[p, j];
                }
            }
        }
        var solution = new double[n];
        for (var i = n - 1; i >= 0; i--)
        {
            var sum = 0.0;
            for (var j = i + 1; j < n; j++)
            {
                sum += matrix[i, j] * solution[j];
            }
            solution[i] = (matrix[i, n] - sum) / matrix[i, i];
        }
        return solution;
    }
}