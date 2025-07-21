using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        var p1 = Console.ReadLine();
        var p2 = Console.ReadLine();
        var p3 = Console.ReadLine();
        var inputs = new List<string> { p1, p2, p3 };
        var solver = new ConditionalProbabilitySolver();
        try
        {
            var results = solver.SolveProbabilities(inputs);
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
        }
        catch (ContradictionException)
        {
            Console.WriteLine("IMPOSSIBLE");
        }
        catch (DivideByZeroException)
        {
            Console.WriteLine("IMPOSSIBLE");
        }
    }
}

public class ConditionalProbabilitySolver
{
    private readonly Dictionary<string, Fraction> _probs = new Dictionary<string, Fraction>();
    private static readonly List<string> _allProbNames = new List<string>
    {
        "A", "A AND B", "A AND NOT B", "A GIVEN B", "A GIVEN NOT B",
        "B", "B GIVEN A", "B GIVEN NOT A", "NOT A", "NOT A AND B",
        "NOT A AND NOT B", "NOT A GIVEN B", "NOT A GIVEN NOT B", "NOT B",
        "NOT B GIVEN A", "NOT B GIVEN NOT A"
    };

    public List<string> SolveProbabilities(List<string> initialProbs)
    {
        foreach (var line in initialProbs)
        {
            ParseAndAdd(line);
        }
        Solve();
        return GetFormattedResults();
    }

    private void ParseAndAdd(string line)
    {
        var parts = line.Split(new[] { " = " }, StringSplitOptions.None);
        var name = parts[0];
        var value = Fraction.Parse(parts[1]);
        AddProbability(name, value);
    }

    private void AddProbability(string name, Fraction value)
    {
        if (value < Fraction.Zero || value > Fraction.One)
        {
            throw new ContradictionException($"Probability {name} = {value} is out of [0, 1] range.");
        }
        if (_probs.TryGetValue(name, out var existingValue))
        {
            if (value != existingValue)
            {
                throw new ContradictionException($"Contradiction for {name}: {existingValue} vs {value}.");
            }
        }
        else
        {
            _probs[name] = value;
        }
    }

    private bool Get(string name, out Fraction value) => _probs.TryGetValue(name, out value);

    private void Solve()
    {
        while (true)
        {
            var countBefore = _probs.Count;
            ApplyComplementRules();
            ApplyTotalProbabilityRules();
            ApplyConditionalRules();
            ApplyTotalJointProbabilityRule();
            if (_probs.Count == countBefore)
            {
                break;
            }
        }
    }

    private void ApplyComplementRules()
    {
        if (Get("A", out var pA) && !Get("NOT A", out _)) AddProbability("NOT A", Fraction.One - pA);
        if (Get("NOT A", out var pNotA) && !Get("A", out _)) AddProbability("A", Fraction.One - pNotA);
        if (Get("B", out var pB) && !Get("NOT B", out _)) AddProbability("NOT B", Fraction.One - pB);
        if (Get("NOT B", out var pNotB) && !Get("B", out _)) AddProbability("B", Fraction.One - pNotB);
        if (Get("A GIVEN B", out var pAgB) && !Get("NOT A GIVEN B", out _)) AddProbability("NOT A GIVEN B", Fraction.One - pAgB);
        if (Get("NOT A GIVEN B", out var pNotAgB) && !Get("A GIVEN B", out _)) AddProbability("A GIVEN B", Fraction.One - pNotAgB);
        if (Get("A GIVEN NOT B", out var pAgNotB) && !Get("NOT A GIVEN NOT B", out _)) AddProbability("NOT A GIVEN NOT B", Fraction.One - pAgNotB);
        if (Get("NOT A GIVEN NOT B", out var pNotAgNotB) && !Get("A GIVEN NOT B", out _)) AddProbability("A GIVEN NOT B", Fraction.One - pNotAgNotB);
        if (Get("B GIVEN A", out var pBgA) && !Get("NOT B GIVEN A", out _)) AddProbability("NOT B GIVEN A", Fraction.One - pBgA);
        if (Get("NOT B GIVEN A", out var pNotBgA) && !Get("B GIVEN A", out _)) AddProbability("B GIVEN A", Fraction.One - pNotBgA);
        if (Get("B GIVEN NOT A", out var pBgNotA) && !Get("NOT B GIVEN NOT A", out _)) AddProbability("NOT B GIVEN NOT A", Fraction.One - pBgNotA);
        if (Get("NOT B GIVEN NOT A", out var pNotBgNotA) && !Get("B GIVEN NOT A", out _)) AddProbability("B GIVEN NOT A", Fraction.One - pNotBgNotA);
    }

    private void ApplyTotalProbabilityRules()
    {
        TryApplyTotalRule("A", "B");
        TryApplyTotalRule("NOT A", "B");
        TryApplyTotalRule("B", "A");
        TryApplyTotalRule("NOT B", "A");
    }

    private void TryApplyTotalRule(string eventX, string eventYBase)
    {
        var notEventYBase = eventYBase == "A" ? "NOT A" : "NOT B";
        var joint1 = CanonicalAndName(eventX, eventYBase);
        var joint2 = CanonicalAndName(eventX, notEventYBase);
        if (Get(joint1, out var pJ1) && Get(joint2, out var pJ2) && !Get(eventX, out _))
        {
            AddProbability(eventX, pJ1 + pJ2);
        }
        if (Get(eventX, out var pX) && Get(joint1, out var pJ1_2) && !Get(joint2, out _))
        {
            AddProbability(joint2, pX - pJ1_2);
        }
        if (Get(eventX, out var pX2) && Get(joint2, out var pJ2_2) && !Get(joint1, out _))
        {
            AddProbability(joint1, pX2 - pJ2_2);
        }
    }

    private void ApplyTotalJointProbabilityRule()
    {
        var joints = new[] { "A AND B", "A AND NOT B", "NOT A AND B", "NOT A AND NOT B" };
        var knownJoints = joints.Where(j => _probs.ContainsKey(j)).ToList();
        if (knownJoints.Count == 3)
        {
            var unknownJoint = joints.First(j => !knownJoints.Contains(j));
            var sumOfKnown = knownJoints.Select(j => _probs[j]).Aggregate(Fraction.Zero, (sum, current) => sum + current);
            AddProbability(unknownJoint, Fraction.One - sumOfKnown);
        }
    }

    private void ApplyConditionalRules()
    {
        TryApplyConditional("A", "B"); TryApplyConditional("A", "NOT B");
        TryApplyConditional("NOT A", "B"); TryApplyConditional("NOT A", "NOT B");
        TryApplyConditional("B", "A"); TryApplyConditional("B", "NOT A");
        TryApplyConditional("NOT B", "A"); TryApplyConditional("NOT B", "NOT A");
    }

    private void TryApplyConditional(string eventX, string eventY)
    {
        var jointName = CanonicalAndName(eventX, eventY);
        var condName = $"{eventX} GIVEN {eventY}";
        if (Get(jointName, out var pJoint) && Get(eventY, out var pY) && !Get(condName, out _))
        {
            if (pY == Fraction.Zero) throw new ContradictionException($"Conditioning on zero-probability event {eventY}.");
            AddProbability(condName, pJoint / pY);
        }
        if (Get(condName, out var pCond) && Get(eventY, out var pY2) && !Get(jointName, out _))
        {
            AddProbability(jointName, pCond * pY2);
        }
        if (Get(jointName, out var pJoint2) && Get(condName, out var pCond2) && !Get(eventY, out _))
        {
            if (pCond2 == Fraction.Zero)
            {
                if (pJoint2 != Fraction.Zero)
                {
                    throw new ContradictionException($"Contradiction: P({jointName}) > 0 but P({condName}) = 0.");
                }
            }
            else
            {
                AddProbability(eventY, pJoint2 / pCond2);
            }
        }
    }

    private string CanonicalAndName(string e1, string e2)
    {
        var partA = e1.Contains("A") ? e1 : (e2.Contains("A") ? e2 : null);
        var partB = e1.Contains("B") ? e1 : (e2.Contains("B") ? e2 : null);
        if (partA == null || partB == null) throw new ArgumentException("Invalid pair for AND name.");
        return $"{partA} AND {partB}";
    }

    private List<string> GetFormattedResults()
    {
        var knownAndSorted = _allProbNames
            .Where(name => _probs.ContainsKey(name))
            .ToList();
        var results = new List<string>();
        foreach (var name in knownAndSorted)
        {
            results.Add($"{name} = {_probs[name]}");
        }
        return results;
    }
}

public class ContradictionException : Exception
{
    public ContradictionException(string message) : base(message) { }
}

public struct Fraction : IComparable<Fraction>, IEquatable<Fraction>
{
    public long Numerator { get; }
    public long Denominator { get; }
    public static readonly Fraction Zero = new Fraction(0);
    public static readonly Fraction One = new Fraction(1);
    public Fraction(long numerator, long denominator = 1)
    {
        if (denominator == 0)
        {
            throw new DivideByZeroException("Denominator cannot be zero.");
        }
        if (numerator == 0)
        {
            Numerator = 0;
            Denominator = 1;
            return;
        }
        var common = (long)BigInteger.GreatestCommonDivisor(Math.Abs(numerator), Math.Abs(denominator));
        Numerator = numerator / common;
        Denominator = denominator / common;
        if (Denominator < 0)
        {
            Numerator = -Numerator;
            Denominator = -Denominator;
        }
    }
    public static Fraction Parse(string s)
    {
        var parts = s.Split('/');
        var num = long.Parse(parts[0]);
        var den = parts.Length > 1 ? long.Parse(parts[1]) : 1;
        return new Fraction(num, den);
    }
    public override string ToString() => Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}";
    public static Fraction operator +(Fraction a, Fraction b) => new Fraction(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static Fraction operator -(Fraction a, Fraction b) => new Fraction(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static Fraction operator *(Fraction a, Fraction b) => new Fraction(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    public static Fraction operator /(Fraction a, Fraction b)
    {
        if (b.Numerator == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero fraction.");
        }
        return new Fraction(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }
    public int CompareTo(Fraction other) => (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
    public bool Equals(Fraction other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override bool Equals(object obj) => obj is Fraction other && Equals(other);
    public override int GetHashCode() => (Numerator, Denominator).GetHashCode();
    public static bool operator ==(Fraction a, Fraction b) => a.Equals(b);
    public static bool operator !=(Fraction a, Fraction b) => !a.Equals(b);
    public static bool operator <(Fraction a, Fraction b) => a.CompareTo(b) < 0;
    public static bool operator >(Fraction a, Fraction b) => a.CompareTo(b) > 0;
    public static bool operator <=(Fraction a, Fraction b) => a.CompareTo(b) <= 0;
    public static bool operator >=(Fraction a, Fraction b) => a.CompareTo(b) >= 0;
}