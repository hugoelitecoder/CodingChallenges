using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

class Program
{
    class State
    {
        public Dictionary<int,List<string>> Table;
        public int MaxRowId;
    }

    static string GetRowType(List<string> row)
    {
        var numbers = row.Select(t => int.Parse(t[..^1])).ToList();
        var colors = row.Select(t => t[^1]).ToList();
        int len = row.Count;
        if (numbers.Distinct().Count() == 1)
        {
            bool distinctColors = colors.Distinct().Count() == colors.Count;
            return (distinctColors && len >= 3 && len <= 4) ? "set" : "invalid";
        }
        else if (colors.Distinct().Count() == 1)
        {
            bool distinctNums = numbers.Distinct().Count() == len;
            return (distinctNums && len >= 3 && len <= 13 && numbers.Max() - numbers.Min() == len - 1) ? "run" : "invalid";
        }
        else return "invalid";
    }

    static List<List<string>> TryTake(string tile, List<string> row)
    {
        if (!row.Contains(tile)) return null;
        var idx = row.IndexOf(tile);
        var rem = new List<string>(row);
        rem.RemoveAt(idx);
        if (GetRowType(row) == "run")
        {
            int left = idx;
            int right = row.Count - idx - 1;
            if (left >= 3 && right >= 3)
            {
                var part1 = row.Take(idx).ToList();
                var part2 = row.Skip(idx + 1).ToList();
                if (GetRowType(part1) == "run" && GetRowType(part2) == "run")
                    return new List<List<string>> { part1, part2 };
            }
        }
        if (GetRowType(rem) != "invalid")
            return new List<List<string>> { rem };
        return null;
    }

    static List<List<string>> TryPut(string tile, List<string> row)
    {
        var cand = row.Append(tile).ToList();
        var type = GetRowType(cand);
        if (type != "invalid")
        {
            List<string> sorted;
            if (type == "set")
            {
                var order = new Dictionary<char,int> {{'B',0},{'G',1},{'R',2},{'Y',3}};
                sorted = cand.OrderBy(t => order[t[^1]]).ToList();
            }
            else
                sorted = cand.OrderBy(t => int.Parse(t[..^1])).ToList();
            return new List<List<string>> { sorted };
        }
        int idx = row.IndexOf(tile);
        if (idx >= 2 && row.Count - idx >= 2)
        {
            var first = row.Take(idx+1).ToList();
            var second = row.Skip(idx).ToList();
            if (GetRowType(first) == "run" && GetRowType(second) == "run")
                return new List<List<string>> { first, second };
        }
        return null;
    }

    static List<List<string>> TryCombine(List<string> a, List<string> b)
    {
        var merged = a.Concat(b).OrderBy(t => int.Parse(t[..^1])).ToList();
        return GetRowType(merged) == "run" ? new List<List<string>> { merged } : null;
    }

    static void NextSteps(string tile, string path, State state, Dictionary<string, State> outStates, bool goalPhase)
    {
        if (goalPhase)
        {
            foreach (var kv in state.Table)
            {
                var rid = kv.Key;
                var rows = TryPut(tile, kv.Value);
                if (rows == null) continue;
                int baseMax = state.MaxRowId;
                var tbl2 = Clone(state.Table);
                tbl2[rid] = rows[0];
                if (rows.Count == 2)
                {
                    baseMax++;
                    tbl2[baseMax] = rows[1];
                }
                var key2 = $"{path};PUT {tile} {rid}";
                outStates[key2] = new State { Table = tbl2, MaxRowId = baseMax };
            }
            return;
        }
        foreach (var kv in state.Table)
        {
            var rid = kv.Key;
            var took = TryTake(tile, kv.Value);
            if (took == null) continue;
            int baseMax = state.MaxRowId;
            var tbl1 = Clone(state.Table);
            tbl1[rid] = took[0];
            var newIds = new HashSet<int> { rid };
            if (took.Count == 2)
            {
                baseMax++;
                tbl1[baseMax] = took[1];
                newIds.Add(baseMax);
            }
            var step1 = $"{path};TAKE {tile} {rid}";
            foreach (var kv2 in tbl1)
            {
                if (newIds.Contains(kv2.Key)) continue;
                var putRows = TryPut(tile, kv2.Value);
                if (putRows == null) continue;
                int newMax = baseMax;
                var tbl2 = Clone(tbl1);
                tbl2[kv2.Key] = putRows[0];
                if (putRows.Count == 2)
                {
                    newMax++;
                    tbl2[newMax] = putRows[1];
                }
                var key2 = step1 + $";PUT {tile} {kv2.Key}";
                outStates[key2] = new State { Table = tbl2, MaxRowId = newMax };
            }
        }
        var ids = state.Table.Keys.OrderBy(i => i).ToList();
        for (int i = 0; i < ids.Count; i++)
        {
            for (int j = i+1; j < ids.Count; j++)
            {
                var a = state.Table[ids[i]];
                var b = state.Table[ids[j]];
                var comb = TryCombine(a, b);
                if (comb == null) continue;
                var tbl2 = Clone(state.Table);
                tbl2[ids[i]] = comb[0];
                tbl2.Remove(ids[j]);
                var key2 = $"{path};COMBINE {ids[i]} {ids[j]}";
                outStates[key2] = new State { Table = tbl2, MaxRowId = state.MaxRowId };
            }
        }
    }

    static Dictionary<int,List<string>> Clone(Dictionary<int,List<string>> src)
        => src.ToDictionary(kv => kv.Key, kv => new List<string>(kv.Value));

    static void Main()
    {
        var goal = Console.ReadLine().Trim();
        int n = int.Parse(Console.ReadLine());
        var initial = new Dictionary<int,List<string>>();
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            int id = int.Parse(parts[0]);
            initial[id] = parts.Skip(1).ToList();
        }

        var allStates = new Dictionary<string, State>();
        var queue = new Queue<string>();
        allStates[""] = new State { Table = Clone(initial), MaxRowId = initial.Keys.Max() };
        queue.Enqueue("");
        var results = new Dictionary<string, State>();
        int bestLen = int.MaxValue;

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var st = allStates[path];
            allStates.Remove(path);
            var next1 = new Dictionary<string, State>();
            NextSteps(goal, path, st, next1, true);
            foreach (var row in st.Table)
                foreach (var tile in row.Value)
                    NextSteps(tile, path, st, next1, false);

            foreach (var kv in next1)
            {
                int len = kv.Key.Split(' ').Length;
                if (kv.Key.Contains($"PUT {goal}") && len <= bestLen)
                {
                    bestLen = len;
                    results[kv.Key] = kv.Value;
                }
                else if (len < bestLen)
                {
                    allStates[kv.Key] = kv.Value;
                    queue.Enqueue(kv.Key);
                }
            }
        }

        var bestPath = results.Keys.First();
        var finalSt = results[bestPath];
        foreach (var step in bestPath.Trim(';').Split(';'))
            Console.WriteLine(step);
        foreach (var kv in finalSt.Table.OrderBy(kv => kv.Key).Where(kv => kv.Value.Count > 0))
            Console.WriteLine(kv.Key + " " + string.Join(' ', kv.Value));
    }
}