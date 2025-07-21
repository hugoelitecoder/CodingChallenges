using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var years = int.Parse(Console.ReadLine());
        var capacity = int.Parse(Console.ReadLine());
        var nGroups = int.Parse(Console.ReadLine());
        var crewInput = new List<(int age, int count)>();
        int maxAge = 0;
        for (int i = 0; i < nGroups; i++)
        {
            var parts = Console.ReadLine().Split();
            var age = int.Parse(parts[0]);
            var number = int.Parse(parts[1]);
            crewInput.Add((age, number));
            if (age > maxAge) maxAge = age;
        }
        var arrSize = maxAge + years + 2;
        var startAges = new int[arrSize];
        foreach (var (age, count) in crewInput)
            startAges[age] = count;

        var startCrew = new Crew(startAges);
        var expedition = new Expedition(years, capacity, startCrew);
        var bounds = expedition.FindBounds();
        Console.WriteLine($"{bounds.Min} {bounds.Max}");
    }
}

class Crew
{
    readonly int[] _ages;
    public Crew(int[] ages) { _ages = ages; }
    public int Population => _ages.Sum();
    public Crew NextYear(int expectancy, out int babies)
    {
        var n = expectancy + 1;
        var next = new int[n];
        for (int age = 0; age < expectancy; age++)
            next[age + 1] = (age < _ages.Length ? _ages[age] : 0);
        int minF = 20, maxF = expectancy / 2;
        babies = 0;
        if (maxF >= minF)
        {
            int fertile = 0;
            for (int i = minF; i <= maxF && i < next.Length; i++)
                fertile += next[i];
            babies = fertile / 10;
            next[0] = babies;
        }
        return new Crew(next);
    }
    public int[] ToArray() => (int[])_ages.Clone();
}

class ExpeditionBounds
{
    public int Min { get; }
    public int Max { get; }
    public ExpeditionBounds(int min, int max) { Min = min; Max = max; }
}

class Expedition
{
    readonly int _years, _capacity;
    readonly Crew _startCrew;
    public Expedition(int years, int capacity, Crew startCrew)
    {
        _years = years;
        _capacity = capacity;
        _startCrew = startCrew;
    }
    public ExpeditionBounds FindBounds()
    {
        int min = -1, max = -1, hi = _capacity + _years;
        for (int le = 1; le <= hi; le++)
        {
            if (Sim(le))
            {
                min = le;
                break;
            }
        }
        for (int le = min + 1; le <= hi; le++)
        {
            if (!Sim(le))
            {
                max = le - 1;
                break;
            }
        }
        if (max == -1) max = hi;
        return new ExpeditionBounds(min, max);
    }
    bool Sim(int expectancy)
    {
        var cur = _startCrew.ToArray();
        var n = expectancy + 1;
        if (cur.Length < n)
        {
            var tmp = new int[n];
            Array.Copy(cur, tmp, cur.Length);
            cur = tmp;
        }
        int pop = cur.Sum();
        for (int y = 0; y < _years; y++)
        {
            var next = new int[n];
            for (int age = 0; age < expectancy; age++)
                next[age + 1] = cur[age];
            int minF = 20, maxF = expectancy / 2;
            int fertile = 0;
            if (maxF >= minF)
                for (int i = minF; i <= maxF && i < n; i++)
                    fertile += next[i];
            next[0] = fertile / 10;
            pop = 0;
            for (int i = 0; i < n; i++) pop += next[i];
            if (pop > _capacity) return false;
            cur = next;
        }
        return pop >= 200;
    }
}
