using System;
using System.Collections.Generic;
using System.Diagnostics;

public record struct Point(int X, int Y);

public struct Human
{
    public int Id;
    public Point Pos;
}

public struct Zombie
{
    public int Id;
    public Point Pos;
    public int TargetId;
}

public struct Ash
{
    public int Id;
    public Point Pos;
    public int TargetZombieId;
    public Point TargetPos;
}

public class GameState
{
    public Ash Ash;
    public List<Human> Humans;
    public List<Zombie> Zombies;
    public long Score;
}

public class SimulationResult
{
    public long Score;
    public List<Point> Moves;
}

public class Solution
{
    private const int MAP_WIDTH = 16000;
    private const int MAP_HEIGHT = 9000;
    private const int ASH_ID = -1;
    private const int NO_TARGET = -1;
    private const int MAX_SIMULATION_TIME_MS = 95;
    private const int SIMULATION_MOVE_LIMIT = 25;
    private const int MIN_RANDOM_MOVES = 1;
    private const int MAX_RANDOM_MOVES = 4;
    private const int RANDOM_SEED = 42;
    private const int ASH_MOVE_SPEED = 1000;
    private const int ZOMBIE_MOVE_SPEED = 400;
    private const int ASH_KILL_RANGE = 2000;
    private const int ASH_KILL_RANGE_SQUARED = ASH_KILL_RANGE * ASH_KILL_RANGE;
    private const int ZOMBIE_EAT_RANGE_SQUARED = 0;
    private const int SCORE_HUMAN_FACTOR_MULTIPLIER = 10;
    private static readonly long[] FIB_COMBO = { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946 };
    private static readonly Random _random = new(RANDOM_SEED);
    private static SimulationResult _bestResult = new() { Score = -1, Moves = new List<Point>() };
    private static int _moveNum = 0;

    public static void Main(string[] args)
    {
        while (true)
        {
            var inputs = Console.ReadLine().Split(' ');
            var ash = new Ash { Id = ASH_ID, Pos = new Point(int.Parse(inputs[0]), int.Parse(inputs[1])) };

            var humanCount = int.Parse(Console.ReadLine());
            var humans = new Human[humanCount];
            for (var i = 0; i < humanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                humans[i] = new Human { Id = int.Parse(inputs[0]), Pos = new Point(int.Parse(inputs[1]), int.Parse(inputs[2])) };
            }

            var zombieCount = int.Parse(Console.ReadLine());
            var zombies = new Zombie[zombieCount];
            for (var i = 0; i < zombieCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                zombies[i] = new Zombie { Id = int.Parse(inputs[0]), Pos = new Point(int.Parse(inputs[1]), int.Parse(inputs[2])) };
            }

            var timer = Stopwatch.StartNew();
            var newBestFound = false;
            
            while (timer.ElapsedMilliseconds < MAX_SIMULATION_TIME_MS)
            {
                var currentSimResult = RunSimulation(ash, humans, zombies, _bestResult.Score);
                if (currentSimResult.Score > _bestResult.Score)
                {
                    _bestResult = currentSimResult;
                    newBestFound = true;
                }
            }
            
            timer.Stop();
            if (newBestFound) _moveNum = 0; else _moveNum++;
            if (_bestResult.Moves != null && _moveNum < _bestResult.Moves.Count)
            {
                var move = _bestResult.Moves[_moveNum];
                Console.WriteLine($"{move.X} {move.Y}");
            }
            else
            {
                if (zombies.Length > 0)
                {
                    Zombie fallbackTarget = zombies[0];
                    var minDistanceSq = long.MaxValue;
                    foreach (var zombie in zombies)
                    {
                        var d2 = DistanceSquared(ash.Pos, zombie.Pos);
                        if (d2 < minDistanceSq) { minDistanceSq = d2; fallbackTarget = zombie; }
                    }
                    Console.WriteLine($"{fallbackTarget.Pos.X} {fallbackTarget.Pos.Y}");
                }
                else
                {
                    Console.WriteLine($"{ash.Pos.X} {ash.Pos.Y}");
                }
            }
        }
    }
    
    public static SimulationResult RunSimulation(Ash initialAsh, Human[] initialHumans, Zombie[] initialZombies, long currentBestScore)
    {
        var state = new GameState 
        {
            Ash = initialAsh,
            Humans = new List<Human>(initialHumans),
            Zombies = new List<Zombie>(initialZombies),
            Score = 0
        };

        var moves = new List<Point>();
        var randomMoves = _random.Next(MIN_RANDOM_MOVES, MAX_RANDOM_MOVES);

        for(var turn = 0; turn < SIMULATION_MOVE_LIMIT; turn++)
        {
            if (state.Zombies.Count == 0 || state.Humans.Count == 0) break;
            
            var maxHypotheticalScore = state.Score + CalculateMaxHypotheticalScore(state.Humans.Count, state.Zombies.Count);
            if (maxHypotheticalScore < currentBestScore) return new SimulationResult { Score = -1 };

            var humanPositions = new Dictionary<int, Point>(state.Humans.Count);
            foreach (var human in state.Humans) humanPositions[human.Id] = human.Pos;

            UpdateAllZombieTargets(state);
            UpdateAshTarget(state, turn, randomMoves);
            moves.Add(state.Ash.TargetPos);
            
            MoveZombies(state, humanPositions);
            MoveAsh(state);
            
            ResolveKills(ref state);
            if (state.Zombies.Count == 0) break;
            
            ResolveEats(ref state);
        }
        
        return new SimulationResult { Score = state.Score, Moves = moves };
    }

    private static void UpdateAshTarget(GameState state, int turnNum, int randomMoves)
    {
        bool targetExists = false;
        foreach (var z in state.Zombies) { if (z.Id == state.Ash.TargetZombieId) { targetExists = true; break; } }
        
        if (turnNum < randomMoves)
        {
            state.Ash.TargetZombieId = NO_TARGET;
            state.Ash.TargetPos = new Point(_random.Next(0, MAP_WIDTH), _random.Next(0, MAP_HEIGHT));
            return;
        }

        if (state.Ash.TargetZombieId == NO_TARGET || !targetExists)
        {
            var threats = new List<Zombie>();
            foreach(var zombie in state.Zombies) { if (zombie.TargetId != ASH_ID) threats.Add(zombie); }

            Zombie target;
            if (threats.Count > 0) target = threats[_random.Next(threats.Count)];
            else target = state.Zombies[_random.Next(state.Zombies.Count)];
            state.Ash.TargetZombieId = target.Id;
        }
        
        Zombie targetZombie = default;
        foreach (var z in state.Zombies) { if (z.Id == state.Ash.TargetZombieId) { targetZombie = z; break; } }
        
        Point interceptPos;
        if (targetZombie.TargetId == state.Ash.Id)
        {
            interceptPos = MoveTowards(targetZombie.Pos, state.Ash.Pos, ZOMBIE_MOVE_SPEED);
        }
        else 
        {
            Point targetHumanPos = default;
            bool humanFound = false;
            foreach (var human in state.Humans) { if (human.Id == targetZombie.TargetId) { targetHumanPos = human.Pos; humanFound = true; break; } }
            interceptPos = humanFound ? MoveTowards(targetZombie.Pos, targetHumanPos, ZOMBIE_MOVE_SPEED) : targetZombie.Pos;
        }
        state.Ash.TargetPos = interceptPos;
    }

    private static void UpdateAllZombieTargets(GameState state)
    {
        for (var i = 0; i < state.Zombies.Count; i++)
        {
            var zombie = state.Zombies[i];
            int bestTargetId = state.Ash.Id;
            var minDisSq = DistanceSquared(zombie.Pos, state.Ash.Pos);

            foreach (var human in state.Humans)
            {
                var d2 = DistanceSquared(zombie.Pos, human.Pos);
                if (d2 < minDisSq) { minDisSq = d2; bestTargetId = human.Id; }
            }
            zombie.TargetId = bestTargetId;
            state.Zombies[i] = zombie;
        }
    }

    private static void MoveZombies(GameState state, IReadOnlyDictionary<int, Point> humanPositions)
    {
        for (var i = 0; i < state.Zombies.Count; i++)
        {
            var zombie = state.Zombies[i];
            Point targetPos = zombie.TargetId == ASH_ID || !humanPositions.TryGetValue(zombie.TargetId, out var humanPos) 
                ? state.Ash.Pos 
                : humanPos;
            zombie.Pos = MoveTowards(zombie.Pos, targetPos, ZOMBIE_MOVE_SPEED);
            state.Zombies[i] = zombie;
        }
    }
    
    private static void MoveAsh(GameState state)
    {
        state.Ash.Pos = MoveTowards(state.Ash.Pos, state.Ash.TargetPos, ASH_MOVE_SPEED);
    }

    private static void ResolveKills(ref GameState state)
    {
        int killCount = 0;
        long humanFactor = (long)state.Humans.Count * state.Humans.Count * SCORE_HUMAN_FACTOR_MULTIPLIER;

        for (int i = state.Zombies.Count - 1; i >= 0; i--)
        {
            if (DistanceSquared(state.Ash.Pos, state.Zombies[i].Pos) <= ASH_KILL_RANGE_SQUARED)
            {
                state.Score += humanFactor * FIB_COMBO[Math.Min(killCount, FIB_COMBO.Length - 1)];
                killCount++;
                state.Zombies.RemoveAt(i);
            }
        }
    }

    private static void ResolveEats(ref GameState state)
    {
        var eatenIds = new HashSet<int>();
        foreach (var z in state.Zombies)
        {
            if (z.TargetId != ASH_ID)
                foreach(var h in state.Humans)
                    if (h.Id == z.TargetId)
                    {
                        if (DistanceSquared(z.Pos, h.Pos) <= ZOMBIE_EAT_RANGE_SQUARED) eatenIds.Add(h.Id);
                        break;
                    }
        }
        if (eatenIds.Count > 0) state.Humans.RemoveAll(h => eatenIds.Contains(h.Id));
    }

    private static long CalculateMaxHypotheticalScore(int humanCount, int zombieCount)
    {
        if (humanCount == 0 || zombieCount == 0) return 0;
        long hf = (long)humanCount * humanCount * SCORE_HUMAN_FACTOR_MULTIPLIER;
        long total = 0;
        for(var i=0; i<zombieCount; i++) total += hf * FIB_COMBO[Math.Min(i, FIB_COMBO.Length - 1)];
        return total;
    }
    
    private static long DistanceSquared(Point p1, Point p2)
    {
        long dx = p1.X - p2.X;
        long dy = p1.Y - p2.Y;
        return dx * dx + dy * dy;
    }

    private static Point MoveTowards(Point current, Point target, int distance)
    {
        long distSq = DistanceSquared(current, target);
        if (distSq <= (long)distance * distance) return target;
        
        var dist = Math.Sqrt(distSq);
        return new Point(
            (int)Math.Floor(current.X + distance * (target.X - current.X) / dist),
            (int)Math.Floor(current.Y + distance * (target.Y - current.Y) / dist)
        );
    }
}