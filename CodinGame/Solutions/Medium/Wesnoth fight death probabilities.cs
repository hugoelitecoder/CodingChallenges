using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var attacker = ReadUnit();
        var defender = ReadUnit();
        Console.Error.WriteLine($"[DEBUG] Attacker: {attacker}");
        Console.Error.WriteLine($"[DEBUG] Defender: {defender}");
        var simulator = new BattleSimulator(attacker, defender);
        var stopwatch = Stopwatch.StartNew();
        var (attackerDeathProb, defenderDeathProb) = simulator.CalculateProbabilities();
        stopwatch.Stop();
        var attackerDeathPercent = Math.Round(attackerDeathProb * 100);
        var defenderDeathPercent = Math.Round(defenderDeathProb * 100);
        Console.Error.WriteLine($"[DEBUG] Result: P(A dies)={attackerDeathProb:P2}, P(D dies)={defenderDeathProb:P2}");
        Console.Error.WriteLine($"[DEBUG] Execution time: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"{attackerDeathPercent} {defenderDeathPercent}");
    }

    private static Unit ReadUnit()
    {
        var inputs = Console.ReadLine().Split(' ');
        var name = inputs[0];
        var hp = int.Parse(inputs[1]);
        var blows = int.Parse(inputs[2]);
        var toHit = int.Parse(inputs[3]);
        var damage = int.Parse(inputs[4]);
        return new Unit(name, hp, blows, toHit / 100.0, damage);
    }
}

public record Unit(string Name, int InitialHp, int InitialBlows, double ToHitProb, int Damage);

public class BattleSimulator
{
    private readonly Unit _attacker;
    private readonly Unit _defender;
    private readonly Dictionary<(int, int, int, int), (double, double, double)> _memo;

    public BattleSimulator(Unit attacker, Unit defender)
    {
        _attacker = attacker;
        _defender = defender;
        _memo = new Dictionary<(int, int, int, int), (double, double, double)>();
    }

    public (double AttackerDeathProb, double DefenderDeathProb) CalculateProbabilities()
    {
        var (attackerWins, defenderWins, _) = DFS(_attacker.InitialHp, _defender.InitialHp, _attacker.InitialBlows, _defender.InitialBlows);
        return (defenderWins, attackerWins);
    }

    private (double, double, double) DFS(int hp0, int hp1, int blows0, int blows1)
    {
        if (hp0 <= 0) return (0, 1, 0);
        if (hp1 <= 0) return (1, 0, 0);
        if (blows0 == 0 && blows1 == 0) return (0, 0, 1);
        var state = (hp0, hp1, blows0, blows1);
        if (_memo.TryGetValue(state, out var cachedResult))
        {
            return cachedResult;
        }
        var strikesAttacker = _attacker.InitialBlows - blows0;
        var strikesDefender = _defender.InitialBlows - blows1;
        var isAttackerTurn = (strikesAttacker <= strikesDefender) ? (blows0 > 0) : !(blows1 > 0);
        (double, double, double) result;
        if (isAttackerTurn)
        {
            var hitProb = _attacker.ToHitProb;
            var resHit = DFS(hp0, hp1 - _attacker.Damage, blows0 - 1, blows1);
            var resMiss = DFS(hp0, hp1, blows0 - 1, blows1);
            var pAWins = hitProb * resHit.Item1 + (1 - hitProb) * resMiss.Item1;
            var pDWins = hitProb * resHit.Item2 + (1 - hitProb) * resMiss.Item2;
            var pDraw = hitProb * resHit.Item3 + (1 - hitProb) * resMiss.Item3;
            result = (pAWins, pDWins, pDraw);
        }
        else
        {
            var hitProb = _defender.ToHitProb;
            var resHit = DFS(hp0 - _defender.Damage, hp1, blows0, blows1 - 1);
            var resMiss = DFS(hp0, hp1, blows0, blows1 - 1);
            var pAWins = hitProb * resHit.Item1 + (1 - hitProb) * resMiss.Item1;
            var pDWins = hitProb * resHit.Item2 + (1 - hitProb) * resMiss.Item2;
            var pDraw = hitProb * resHit.Item3 + (1 - hitProb) * resMiss.Item3;
            result = (pAWins, pDWins, pDraw);
        }
        _memo[state] = result;
        return result;
    }
}

