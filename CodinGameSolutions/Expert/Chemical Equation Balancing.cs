using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class Solution
{
    static void Main(string[] args)
    {
        var unbalanced = Console.ReadLine();
        var balancer = new ChemicalFormula(unbalanced);
        var balanced = balancer.Balance();
        Console.WriteLine(balanced);
    }
}

public class ChemicalFormula
{
    private readonly List<Molecule> _reactants;
    private readonly List<Molecule> _products;
    private readonly List<Molecule> _allMolecules;
    private readonly List<string> _uniqueElements;

    public ChemicalFormula(string equation)
    {
        var sides = equation.Split(new[] { " -> " }, StringSplitOptions.None);
        _reactants = sides[0].Split(new[] { " + " }, StringSplitOptions.None).Select(s => new Molecule(s)).ToList();
        _products = sides[1].Split(new[] { " + " }, StringSplitOptions.None).Select(s => new Molecule(s)).ToList();
        _allMolecules = _reactants.Concat(_products).ToList();
        _uniqueElements = _allMolecules.SelectMany(m => m.Atoms.Keys).Distinct().ToList();
    }

    public string Balance()
    {
        var maxCoeff = 1;
        while (true)
        {
            var coeffs = new int[_allMolecules.Count];
            if (TryBalance(0, coeffs, maxCoeff))
            {
                return FormatEquation(coeffs);
            }
            maxCoeff++;
        }
    }

    private bool TryBalance(int moleculeIndex, int[] coeffs, int maxCoeff)
    {
        if (moleculeIndex == _allMolecules.Count)
        {
            return IsBalanced(coeffs);
        }

        for (var c = 1; c <= maxCoeff; c++)
        {
            coeffs[moleculeIndex] = c;
            if (TryBalance(moleculeIndex + 1, coeffs, maxCoeff))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBalanced(int[] coeffs)
    {
        foreach (var element in _uniqueElements)
        {
            var balance = 0;
            for (var i = 0; i < _allMolecules.Count; i++)
            {
                var molecule = _allMolecules[i];
                molecule.Atoms.TryGetValue(element, out var atomCount);
                if (i < _reactants.Count)
                {
                    balance += coeffs[i] * atomCount;
                }
                else
                {
                    balance -= coeffs[i] * atomCount;
                }
            }
            if (balance != 0)
            {
                return false;
            }
        }
        return true;
    }

    private string FormatEquation(int[] coeffs)
    {
        var finalCoeffs = Array.ConvertAll(coeffs, c => (long)c);
        if (finalCoeffs.Length > 0)
        {
            var commonDivisor = finalCoeffs[0];
            for (var i = 1; i < finalCoeffs.Length; i++)
            {
                commonDivisor = GCD(commonDivisor, finalCoeffs[i]);
            }
            for (var i = 0; i < finalCoeffs.Length; i++)
            {
                finalCoeffs[i] /= commonDivisor;
            }
        }
        
        var sb = new StringBuilder();
        for (var i = 0; i < _reactants.Count; i++)
        {
            if (i > 0) sb.Append(" + ");
            if (finalCoeffs[i] > 1) sb.Append(finalCoeffs[i]);
            sb.Append(_reactants[i].Text);
        }
        
        sb.Append(" -> ");
        
        for (var i = 0; i < _products.Count; i++)
        {
            if (i > 0) sb.Append(" + ");
            var coeffIndex = _reactants.Count + i;
            if (finalCoeffs[coeffIndex] > 1) sb.Append(finalCoeffs[coeffIndex]);
            sb.Append(_products[i].Text);
        }
        
        return sb.ToString();
    }

    private static long GCD(long a, long b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}

public class Molecule
{
    public string Text { get; }
    public Dictionary<string, int> Atoms { get; }

    public Molecule(string formula)
    {
        Text = formula;
        Atoms = new Dictionary<string, int>();
        var regex = new Regex("([A-Z][a-z]?)(\\d*)");
        var matches = regex.Matches(formula);
        foreach (Match match in matches)
        {
            var element = match.Groups[1].Value;
            var countStr = match.Groups[2].Value;
            var count = string.IsNullOrEmpty(countStr) ? 1 : int.Parse(countStr);
            if (Atoms.ContainsKey(element))
            {
                Atoms[element] += count;
            }
            else
            {
                Atoms.Add(element, count);
            }
        }
    }
}