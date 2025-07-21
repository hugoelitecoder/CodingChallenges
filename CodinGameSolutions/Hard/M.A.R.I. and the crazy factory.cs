using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main(string[] args)
    {
        string instructionLine = Console.ReadLine();
        string[] instructions;
        if (instructionLine.StartsWith(" "))
        {
            instructions = (new[] { "" }).Concat(instructionLine.Trim().Split(' ')).ToArray();
        }
        else
        {
            instructions = instructionLine.Split(' ');
        }

        string[] room = new string[7];
        for (int i = 0; i < 7; i++) room[i] = Console.ReadLine();

        Solution.Run(room, instructions);
    }
}

public class Player
{
    private List<List<char>> grid;
    private List<Tuple<int, int, string>> doors;
    private (int, int)? box;
    private (int, int)? key;
    private bool hasKey;
    public List<string> actionsList;
    private int? zigState;
    private int px, py, dir;
    private (int, int) exit;

    public Player(string[] room)
    {
        grid = room.Select(line => line.ToList()).ToList();
        doors = new List<Tuple<int, int, string>>();
        for (int i = 0; i < room.Length; i++)
        {
            for (int j = 0; j < room[i].Length; j++)
            {
                char c = room[i][j];
                if (c >= '0' && c <= '3')
                {
                    dir = int.Parse(c.ToString());
                    px = i; py = j;
                    grid[i][j] = '.';
                }
                else if (c == 'E') { exit = (i, j); grid[i][j] = '.'; }
                else if (c == 'd') { doors.Add(Tuple.Create(i, j, "#.")); grid[i][j] = '#'; }
                else if (c == 'r') { doors.Add(Tuple.Create(i, j, ".#")); grid[i][j] = '.'; }
                else if (c == 'W') { doors.Add(Tuple.Create(i, j, "##")); grid[i][j] = '#'; }
                else if (c == 'G') { box = (i, j); grid[i][j] = '.'; }
                else if (c == 'k') { key = (i, j); grid[i][j] = '.'; }
            }
        }
        actionsList = new List<string>();
        hasKey = false;
    }

    private static readonly (int, int)[] directions = { (-1, 0), (0, 1), (1, 0), (0, -1) };

    public void Move()
    {
        int nx = px + directions[dir].Item1;
        int ny = py + directions[dir].Item2;
        if (!(0 <= nx && nx < 7 && 0 <= ny && ny < 5)) throw new Exception("Out of bounds");
        if (grid[nx][ny] != '.') throw new Exception("Wall");
        if (box.HasValue && box.Value == (nx, ny))
        {
            int bx = box.Value.Item1 + directions[dir].Item1;
            int by = box.Value.Item2 + directions[dir].Item2;
            if (!(0 <= bx && bx < 7 && 0 <= by && by < 5)) throw new Exception("Box can't exit");
            if (grid[bx][by] != '.') throw new Exception("Box can't move");
            box = (bx, by);
        }
        px = nx; py = ny;
    }

    public void Move2() { Move(); Move(); }
    public void TurnR() { dir = (dir + 1) % 4; }
    public void TurnL() { dir = (dir + 3) % 4; }
    public void ZigR() { if (zigState == null) zigState = 0; Action[] turns = { TurnR, TurnL }; turns[zigState.Value](); zigState = 1 - zigState; }
    public void ZigL() { if (zigState == null) zigState = 1; Action[] turns = { TurnR, TurnL }; turns[zigState.Value](); zigState = 1 - zigState; }
    public void Nop() { }

    public void UseKey()
    {
        if (!hasKey) throw new Exception("Need key");
        if (doors.Count < 3) throw new Exception("Not enough doors");
        var door1 = doors[0];
        if (px == door1.Item1 && py == door1.Item2) throw new Exception("Crushed");
        string rev1 = new string(door1.Item3.Reverse().ToArray());
        doors[0] = Tuple.Create(door1.Item1, door1.Item2, rev1);
        grid[door1.Item1][door1.Item2] = doors[0].Item3[0];

        var door2 = doors[2];
        if (px == door2.Item1 && py == door2.Item2) throw new Exception("Crushed");
        string rev2 = new string(door2.Item3.Reverse().ToArray());
        doors[2] = Tuple.Create(door2.Item1, door2.Item2, rev2);
        grid[door2.Item1][door2.Item2] = doors[2].Item3[0];
    }

    public bool IsFinished()
    {
        return (px, py) == exit && (!key.HasValue || hasKey);
    }

    public void DoAction(string act)
    {
        switch (act)
        {
            case "p1": Move(); break;
            case "p2": Move2(); break;
            case "tr": TurnR(); break;
            case "tl": TurnL(); break;
            case "zr": ZigR(); break;
            case "zl": ZigL(); break;
            case "uk": UseKey(); break;
            case "nop": Nop(); break;
        }
    }

    public void DoActions()
    {
        if (actionsList.Contains("nop") && actionsList.Last() != "nop") throw new Exception("nop must be last");
        for (int i = 0; i < actionsList.Count; i++)
        {
            string current = actionsList[i];
            if (current == "bk") return;
            if (current == "x2" && i + 1 < actionsList.Count)
            {
                DoAction(actionsList[i + 1]);
                DoAction(actionsList[i + 1]);
                i++;
            }
            else
            {
                DoAction(current);
            }
        }
        if (key.HasValue && (px, py) == key.Value)
        {
            hasKey = true;
        }
    }
}

public static class PermutationHelper
{
    public static IEnumerable<IList<T>> Permutations<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        if (list.Count == 0) { yield return new List<T>(); yield break; }
        for (int i = 0; i < list.Count; i++)
        {
            var current = list[i];
            var rest = list.Take(i).Concat(list.Skip(i + 1));
            foreach (var perm in rest.Permutations())
                yield return new[] { current }.Concat(perm).ToList();
        }
    }
}

public static class Solution
{
    public static long Fact(long n)
    {
        if (n <= 1) return 1;
        long res = 1;
        for (long i = 2; i <= n; i++) res *= i;
        return res;
    }

    public static List<int> ToBaseFact(long n, int m)
    {
        long fact = 1;
        int p = 1;
        while (fact <= n) { p++; fact *= p; }
        fact /= p;
        p--;
        var res = new List<int>();
        while (p > 0)
        {
            int r = (int)(n / fact);
            n %= fact;
            res.Add(r);
            fact /= p;
            p--;
        }
        res.Reverse();
        while (res.Count < m) res.Add(0);
        return res;
    }

    public static void Run(string[] room, string[] instructions)
    {
        bool hasBlank = instructions[0] == "";
        var moves = hasBlank ? instructions.Skip(1).ToList() : instructions.Skip(1).ToList();
        int len = moves.Count;

        foreach (var perm in PermutationHelper.Permutations(moves))
        {
            long limit = Fact(len + 1 - (hasBlank ? 1 : 0));
            for (long n = 0; n < limit; n++)
            {
                Player player = new Player(room);
                var steps = new List<string>();
                var order = ToBaseFact(n, len);

                List<string> currentSeq = new List<string>();
                if (!hasBlank) currentSeq.Add(instructions[0]);
                List<int> idxList = order;
                if (hasBlank)
                {
                    idxList.Insert(0, 0);
                    idxList.RemoveAt(idxList.Count - 1);
                }

                bool broken = false;
                for (int i = 0; i < perm.Count; i++)
                {
                    currentSeq.Insert(idxList[i], perm.ElementAt(i));
                    steps.Add(string.Join(" ", currentSeq));
                    player.actionsList = new List<string>(currentSeq);
                    try
                    {
                        player.DoActions();
                    }
                    catch (Exception)
                    {
                        broken = true;
                        break;
                    }
                    if (player.IsFinished())
                    {
                        foreach (var s in steps)
                        {
                            Console.WriteLine(s);
                        }
                        return;
                    }
                }
                // continue if broken
            }
        }
    }
}
