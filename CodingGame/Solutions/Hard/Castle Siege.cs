using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var dims = Console.ReadLine().Split(' ');
        int W = int.Parse(dims[0]);
        int H = int.Parse(dims[1]);
        var lines = new List<string>();
        for (int i = 0; i < H; i++) lines.Add(Console.ReadLine());
        var map = Map.Parse(lines, W, H);
        var simulator = new SiegeSimulator();
        var result = simulator.Run(map);
        Console.WriteLine($"{result.Status} {result.Round}");
    }
}

class SiegeResult
{
    public string Status;
    public int Round;
    public SiegeResult(string status, int round)
    {
        Status = status;
        Round = round;
    }
}

class SiegeSimulator
{
    public SiegeResult Run(Map map)
    {
        int round = 1;
        while (true)
        {
            var targetings = TowerTargeting(map.Towers, map.Enemies);
            map.ApplyDamage(targetings);
            if (map.Enemies.Count == 0) return new SiegeResult("WIN", round);
            if (!map.MoveEnemiesNorth()) return new SiegeResult("LOSE", round);
            round++;
        }
    }

    private Dictionary<Enemy, int> TowerTargeting(List<Tower> towers, List<Enemy> enemies)
    {
        var targets = new Dictionary<Enemy, int>();
        foreach (var tower in towers)
        {
            var candidates = new List<Enemy>();
            foreach (var enemy in enemies)
                if (tower.InRange(enemy)) candidates.Add(enemy);
            if (candidates.Count == 0) continue;
            var best = candidates
                .OrderBy(e => e.Row)
                .ThenBy(e => tower.Distance(e))
                .ThenByDescending(e => e.Col)
                .First();
            if (!targets.ContainsKey(best)) targets[best] = 0;
            targets[best]++;
        }
        return targets;
    }
}

class Map
{
    public int Width { get; }
    public int Height { get; }
    public char[,] Cells { get; }
    public List<Tower> Towers { get; }
    public List<Enemy> Enemies { get; set; }

    private Map(char[,] cells, List<Tower> towers, List<Enemy> enemies, int w, int h)
    {
        Cells = cells;
        Towers = towers;
        Enemies = enemies;
        Width = w;
        Height = h;
    }

    public static Map Parse(List<string> lines, int W, int H)
    {
        var cells = new char[H, W];
        var towers = new List<Tower>();
        var enemies = new List<Enemy>();
        for (int r = 0; r < H; r++)
        {
            for (int c = 0; c < W; c++)
            {
                char ch = lines[r][c];
                cells[r, c] = ch;
                if (ch == 'T')
                    towers.Add(new Tower(r, c));
                else if (char.IsDigit(ch)) 
                    enemies.Add(new Enemy(r, c, ch - '0'));
            }
        }
        return new Map(cells, towers, enemies, W, H);
    }

    public void ApplyDamage(Dictionary<Enemy, int> targetings)
    {
        foreach (var pair in targetings)
            pair.Key.HP -= pair.Value;
        Enemies = Enemies.Where(e => e.HP > 0).ToList();
    }

    public bool MoveEnemiesNorth()
    {
        var survivors = new List<Enemy>();
        foreach (var e in Enemies)
        {
            int nr = e.Row - 1;
            if (nr < 0) return false; // Moved off map: lose
            if (Cells[nr, e.Col] == 'T') continue; // Collided with tower: destroyed
            survivors.Add(new Enemy(nr, e.Col, e.HP));
        }
        Enemies = survivors;
        return true;
    }
}

class Enemy
{
    public int Row { get; }
    public int Col { get; }
    public int HP { get; set; }
    public Enemy(int row, int col, int hp)
    {
        Row = row;
        Col = col;
        HP = hp;
    }
}

class Tower
{
    public int Row { get; }
    public int Col { get; }
    public Tower(int row, int col)
    {
        Row = row;
        Col = col;
    }
    public bool InRange(Enemy e)
    {
        return Math.Abs(Row - e.Row) <= 2 && Math.Abs(Col - e.Col) <= 2;
    }
    public int Distance(Enemy e)
    {
        return Math.Abs(Row - e.Row) + Math.Abs(Col - e.Col);
    }
}
