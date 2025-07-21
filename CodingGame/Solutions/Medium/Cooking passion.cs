using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main()
    {
        var tokens = Console.ReadLine().Split();
        int recipeCount = int.Parse(tokens[0]);
        int ingCount    = int.Parse(tokens[1]);

        var requirements = new Dictionary<string,int>();
        var isLiquid     = new Dictionary<string,bool>();

        for (int i = 0; i < recipeCount; i++)
        {
            var parts = Console.ReadLine().Split();
            if (parts[0] != "-") continue;
            string qty  = parts[1];
            string name = string.Join(" ", parts.Skip(2));
            bool liquid = qty.EndsWith("L") || qty.EndsWith("cl");
            isLiquid[name]     = liquid;
            requirements[name] = UnitConverter.ToBase(qty);
        }

        var stock = new Dictionary<string,int>();
        for (int i = 0; i < ingCount; i++)
        {
            var parts = Console.ReadLine().Split();
            string qty  = parts[^1];
            string name = string.Join(" ", parts.Take(parts.Length - 1));
            stock[name] = UnitConverter.ToBase(qty);
        }

        var calc = new CookCalculator(requirements, isLiquid, stock);
        calc.Compute();

        Console.WriteLine(calc.Limit);
        Console.WriteLine(calc.Times);
        foreach (var item in calc.Leftovers)
            Console.WriteLine($"{item.Name} {UnitConverter.Format(item.Quantity, isLiquid[item.Name])}");
    }
}

static class UnitConverter
{
    public static int ToBase(string s)
    {
        string u = s.Length >= 2 && (s.EndsWith("kg") || s.EndsWith("cl")) ? s[^2..] : s[^1..];
        double v = double.Parse(s[..^u.Length], CultureInfo.InvariantCulture);
        return u switch
        {
            "kg" => (int)(v * 1000),
            "g"  => (int)v,
            "L"  => (int)(v * 100),
            "cl" => (int)v,
            _    => 0
        };
    }

    public static string Format(int amount, bool liquid)
    {
        if (!liquid)
        {
            if (amount < 1000) return $"{amount}g";
            return $"{(amount / 1000.0).ToString("0.0################", CultureInfo.InvariantCulture)}kg";
        }
        if (amount < 100) return $"{amount}cl";
        return $"{(amount / 100.0).ToString("0.0################", CultureInfo.InvariantCulture)}L";
    }
}

class CookCalculator
{
    private readonly Dictionary<string,int> _req;
    private readonly Dictionary<string,bool> _liq;
    private readonly Dictionary<string,int> _stock;

    public int Times { get; private set; }
    public string Limit { get; private set; }
    public List<(string Name,int Quantity)> Leftovers { get; private set; }

    public CookCalculator(
        Dictionary<string,int> req,
        Dictionary<string,bool> liq,
        Dictionary<string,int> stock)
    {
        _req   = req;
        _liq   = liq;
        _stock = new Dictionary<string,int>(stock);
    }

    public void Compute()
    {
        Times = _req.Min(kv => _stock[kv.Key] / kv.Value);
        Limit = _req.Keys.First(name => _stock[name] - _req[name] * Times == 0);
        Leftovers = _stock
            .Where(kv => kv.Key != Limit)
            .Select(kv => (kv.Key, kv.Value - _req[kv.Key] * Times))
            .OrderBy(kv => _liq[kv.Key] ? 1 : 0)
            .ThenBy(kv => kv.Item2)
            .Select(kv => (kv.Key, kv.Item2))
            .ToList();
    }
}
