using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

public class Program
{
    public static void Main(string[] args)
    {
        var goalTile = new Tile(Console.ReadLine().Trim());

        int n = int.Parse(Console.ReadLine() ?? "0");
        var rows = new List<string[]>();
        for (int i = 0; i < n; i++)
        {
            rows.Add((Console.ReadLine() ?? "").Trim().Split(' '));
        }

        var stopwatch = Stopwatch.StartNew();
        var table = new Table(rows);
        var solver = new RummikubSolver();
        var bestSolution = solver.Solve(table, goalTile);
        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Total execution time: {stopwatch.ElapsedMilliseconds} ms");

        if (bestSolution != null)
        {
            var bestActions = bestSolution.Item2;
            var bestTable = bestSolution.Item1;

            var sb = new StringBuilder();
            foreach (var action in bestActions)
            {
                sb.AppendLine(action.ToString());
            }
            Console.Write(sb.ToString());
            bestTable.OutputRows();
        }
    }
}

public enum TileColor { B, G, R, Y, Joker }
public enum RowType { Set, Run }
public enum ActionType { Put, Take, Combine }
public enum ManipulationMethod { Find, Remove }

public class Action
{
    public ActionType Type { get; }
    public Tile Tile { get; }
    public int RowID1 { get; }
    public int RowID2 { get; }

    public Action(ActionType type, Tile tile, int rowId)
    {
        if (type == ActionType.Combine) throw new ArgumentException("Use a different constructor for Combine actions.");
        Type = type;
        Tile = tile;
        RowID1 = rowId;
        RowID2 = -1;
    }

    public Action(ActionType type, int rowId1, int rowId2)
    {
        if (type != ActionType.Combine) throw new ArgumentException("This constructor is only for Combine actions.");
        Type = type;
        Tile = null;
        RowID1 = rowId1;
        RowID2 = rowId2;
    }

    public override string ToString()
    {
        return Type switch
        {
            ActionType.Put => $"PUT {Tile.GetName()} {RowID1}",
            ActionType.Take => $"TAKE {Tile.GetName()} {RowID1}",
            ActionType.Combine => $"COMBINE {RowID1} {RowID2}",
            _ => throw new InvalidOperationException("Unknown action type.")
        };
    }
}


public class RummikubSolver
{
    private readonly Dictionary<string, List<Tuple<Table, List<Action>>>> _addTileCache = new Dictionary<string, List<Tuple<Table, List<Action>>>>();
    private readonly Dictionary<string, List<Tuple<Table, List<Action>>>> _findTileCache = new Dictionary<string, List<Tuple<Table, List<Action>>>>();

    private string GetCacheKey(Table table, Tile tile, IReadOnlyDictionary<int, int> forbiddenRows)
    {
        var sb = new StringBuilder();

        // Part 1: The tile being manipulated
        sb.Append(tile.GetName()).Append('|');

        // Part 2: The forbidden rows, sorted for canonical key representation
        var forbiddenKeys = forbiddenRows.Keys.OrderBy(k => k);
        foreach (var key in forbiddenKeys)
        {
            sb.Append(key).Append(',');
        }
        sb.Append('|');

        // Part 3: The state of the table, with rows sorted by ID for a canonical key
        var rowKeys = table.GetRows().Keys.OrderBy(k => k);
        foreach (var key in rowKeys)
        {
            sb.Append(key).Append(':').Append(table.GetRow(key).GetDisplay()).Append(';');
        }
        return sb.ToString();
    }

    private List<Tuple<Table, List<Action>>> CloneSolutions(IEnumerable<Tuple<Table, List<Action>>> solutions)
    {
        var clonedList = new List<Tuple<Table, List<Action>>>();
        foreach (var solution in solutions)
        {
            // Table needs a deep clone to ensure state isolation between search paths.
            // Action objects are immutable, so a new list wrapper is sufficient.
            clonedList.Add(Tuple.Create(solution.Item1.Clone(), new List<Action>(solution.Item2)));
        }
        return clonedList;
    }

    public Tuple<Table, List<Action>> Solve(Table initialTable, Tile goalTile)
    {
        var allSolutions = AddTile(initialTable, goalTile, new Dictionary<int, int>());
        return FindBestSolution(allSolutions);
    }

    private Tuple<Table, List<Action>> FindBestSolution(List<Tuple<Table, List<Action>>> solutions)
    {
        int bestCount = int.MaxValue;
        int bestFirstCombine = int.MaxValue;
        List<Action> bestActions = null;
        Table bestTable = null;

        int bestCombineR1 = 0;
        int bestCombineR2 = 0;

        foreach (var pair in solutions)
        {
            var tableCandidate = pair.Item1;
            var actions = new List<Action>(pair.Item2);

            int count = actions.Count;
            int firstCombine = int.MaxValue;

            for (int index = 0; index < actions.Count; index++)
            {
                if (actions[index].Type != ActionType.Combine) continue;

                int indexCombine = index;
                var combineAction = actions[indexCombine];

                while (indexCombine >= 2 && actions[indexCombine - 1].Type != ActionType.Combine)
                {
                    var a2Row = actions[indexCombine - 2].RowID1;
                    var a3Row = actions[indexCombine - 1].RowID1;

                    bool disjoint = (combineAction.RowID1 != a2Row) && (combineAction.RowID2 != a2Row) &&
                            (combineAction.RowID1 != a3Row) && (combineAction.RowID2 != a3Row);

                    if (!disjoint) break;

                    var prev1 = actions[indexCombine - 1];
                    var prev2 = actions[indexCombine - 2];
                    actions[indexCombine - 2] = combineAction;
                    actions[indexCombine - 1] = prev2;
                    actions[indexCombine] = prev1;

                    indexCombine -= 2;
                }
                firstCombine = Math.Min(firstCombine, indexCombine);
            }

            if (count > bestCount) continue;

            if (count == bestCount)
            {
                if (firstCombine > bestFirstCombine) continue;
                if (firstCombine == bestFirstCombine && firstCombine != int.MaxValue)
                {
                    var currCombine = actions[firstCombine];
                    int r1_curr = currCombine.RowID1;
                    int r2_curr = currCombine.RowID2;

                    if (bestCombineR1 < r1_curr || (bestCombineR1 == r1_curr && bestCombineR2 < r2_curr))
                    {
                        continue;
                    }
                }
            }

            bestCount = count;
            bestFirstCombine = firstCombine;
            bestActions = actions;
            bestTable = tableCandidate;
            if (bestFirstCombine != int.MaxValue && bestActions != null && bestFirstCombine < bestActions.Count)
            {
                var bestCombine = bestActions[bestFirstCombine];
                bestCombineR1 = bestCombine.RowID1;
                bestCombineR2 = bestCombine.RowID2;
            }
        }

        if (bestTable == null) return null;
        return Tuple.Create(bestTable, bestActions);
    }

    public List<Tuple<Table, List<Action>>> AddTile(Table tableInitial, Tile tile, Dictionary<int, int> forbiddenRows)
    {
        string cacheKey = GetCacheKey(tableInitial, tile, forbiddenRows);
        if (_addTileCache.TryGetValue(cacheKey, out var cachedResult))
        {
            return CloneSolutions(cachedResult);
        }

        var solvedSeries = new List<Tuple<Table, List<Action>>>();
        foreach (var kv in tableInitial.GetRows())
        {
            int rowID = kv.Key;
            if (forbiddenRows.ContainsKey(rowID)) continue;
            var tableUpdated = tableInitial.Clone();
            if (tableUpdated.Insert(rowID, tile).Count > 0)
            {
                solvedSeries.Add(Tuple.Create(tableUpdated, new List<Action> { new Action(ActionType.Put, tile, rowID) }));
            }
        }

        solvedSeries.AddRange(TryCombine(tableInitial, tile, ActionType.Put, forbiddenRows));
        foreach (var kv in tableInitial.GetRows())
        {
            int rowID = kv.Key;
            if (forbiddenRows.ContainsKey(rowID)) continue;

            var stack = new Stack<Tuple<Table, List<Action>>>();
            stack.Push(Tuple.Create(tableInitial.Clone(), new List<Action>()));

            while (stack.Count > 0)
            {
                var top = stack.Pop();
                var table = top.Item1;
                var actions = top.Item2;

                var rows = table.GetRows();
                if (!rows.ContainsKey(rowID)) continue;

                var row = rows[rowID];
                var could = row is Set set ? set.CouldInsert(tile)
                     : row is Run run ? run.CouldInsert(tile)
                     : new List<Tuple<ManipulationMethod, Tile>>();

                foreach (var step in could)
                {
                    var method = step.Item1;
                    var tileInfo = step.Item2;

                    if (method == ManipulationMethod.Find)
                    {
                        var forb = new Dictionary<int, int>(forbiddenRows);
                        forb[rowID] = 1;
                        var seriesUpdated = FindTile(table, tileInfo, forb);

                        foreach (var upd in seriesUpdated)
                        {
                            var tableUpdated = upd.Item1;
                            var actionsTile = new List<Action>(actions);
                            actionsTile.AddRange(upd.Item2);
                            actionsTile.Add(new Action(ActionType.Put, tileInfo, rowID));

                            var rowsAffected = tableUpdated.Insert(rowID, tileInfo);
                            bool addedGoal = false;
                            foreach (var rowID2 in rowsAffected)
                            {
                                if (tableUpdated.Insert(rowID2, tile).Count > 0)
                                {
                                    actionsTile.Add(new Action(ActionType.Put, tile, rowID2));
                                    solvedSeries.Add(Tuple.Create(tableUpdated, actionsTile));
                                    addedGoal = true;
                                    break;
                                }
                            }
                            if (addedGoal) continue;
                            stack.Push(Tuple.Create(tableUpdated, actionsTile));
                        }
                    }
                    else if (method == ManipulationMethod.Remove)
                    {
                        var tableCopy = table.Clone();
                        var forb = new Dictionary<int, int>(forbiddenRows);
                        forb[rowID] = 1;
                        var seriesUpdated = AddTile(tableCopy, tileInfo, forb);

                        foreach (var upd in seriesUpdated)
                        {
                            var tableUpdated = upd.Item1.Clone();
                            var actionsTile = new List<Action>(actions);
                            actionsTile.Add(new Action(ActionType.Take, tileInfo, rowID));
                            actionsTile.AddRange(upd.Item2);

                            if (!tableUpdated.Remove(rowID, tileInfo)) continue;

                            if (tableUpdated.Insert(rowID, tile).Count > 0)
                            {
                                actionsTile.Add(new Action(ActionType.Put, tile, rowID));
                                solvedSeries.Add(Tuple.Create(tableUpdated, actionsTile));
                            }
                            else
                            {
                                stack.Push(Tuple.Create(tableUpdated, actionsTile));
                            }
                        }
                    }
                }
            }
        }

        _addTileCache[cacheKey] = CloneSolutions(solvedSeries);
        return solvedSeries;
    }

    public List<Tuple<Table, List<Action>>> FindTile(Table tableInitial, Tile tile, Dictionary<int, int> forbiddenRows)
    {
        string cacheKey = GetCacheKey(tableInitial, tile, forbiddenRows);
        if (_findTileCache.TryGetValue(cacheKey, out var cachedResult))
        {
            return CloneSolutions(cachedResult);
        }

        var solvedSeries = new List<Tuple<Table, List<Action>>>();
        foreach (var kv in tableInitial.GetRows())
        {
            int rowID = kv.Key;
            if (forbiddenRows.ContainsKey(rowID)) continue;

            var tableUpdated = tableInitial.Clone();
            if (tableUpdated.Remove(rowID, tile))
                solvedSeries.Add(Tuple.Create(tableUpdated, new List<Action> { new Action(ActionType.Take, tile, rowID) }));
        }

        solvedSeries.AddRange(TryCombine(tableInitial, tile, ActionType.Take, forbiddenRows));
        foreach (var kv in tableInitial.GetRows())
        {
            int rowID = kv.Key;
            if (forbiddenRows.ContainsKey(rowID)) continue;

            var stack = new Stack<Tuple<Table, List<Action>>>();
            stack.Push(Tuple.Create(tableInitial.Clone(), new List<Action>()));

            while (stack.Count > 0)
            {
                var top = stack.Pop();
                var table = top.Item1;
                var actions = top.Item2;

                var rows = table.GetRows();
                if (!rows.ContainsKey(rowID)) continue;

                var row = rows[rowID];
                var could = row is Set set ? set.CouldTake(tile)
                     : row is Run run ? run.CouldTake(tile)
                     : new List<Tuple<ManipulationMethod, Tile>>();

                foreach (var step in could)
                {
                    var method = step.Item1;
                    var tileInfo = step.Item2;

                    if (method == ManipulationMethod.Find)
                    {
                        var forb = new Dictionary<int, int>(forbiddenRows);
                        forb[rowID] = 1;
                        var seriesUpdated = FindTile(table, tileInfo, forb);

                        foreach (var upd in seriesUpdated)
                        {
                            var tableUpdated = upd.Item1;
                            var actionsTile = new List<Action>(actions);
                            actionsTile.AddRange(upd.Item2);
                            actionsTile.Add(new Action(ActionType.Put, tileInfo, rowID));

                            var rowsAffected = tableUpdated.Insert(rowID, tileInfo);
                            bool tookGoal = false;
                            foreach (var rowID2 in rowsAffected)
                            {
                                if (tableUpdated.Remove(rowID2, tile))
                                {
                                    actionsTile.Add(new Action(ActionType.Take, tile, rowID2));
                                    solvedSeries.Add(Tuple.Create(tableUpdated, actionsTile));
                                    tookGoal = true;
                                    break;
                                }
                            }
                            if (tookGoal) continue;
                            stack.Push(Tuple.Create(tableUpdated, actionsTile));
                        }
                    }
                    else if (method == ManipulationMethod.Remove)
                    {
                        var tableCopy = table.Clone();
                        var forb = new Dictionary<int, int>(forbiddenRows);
                        forb[rowID] = 1;
                        var seriesUpdated = AddTile(tableCopy, tileInfo, forb);

                        foreach (var upd in seriesUpdated)
                        {
                            var tableUpdated = upd.Item1.Clone();
                            var actionsTile = new List<Action>(actions);
                            actionsTile.Add(new Action(ActionType.Take, tileInfo, rowID));
                            actionsTile.AddRange(upd.Item2);

                            if (!tableUpdated.Remove(rowID, tileInfo)) continue;

                            if (tableUpdated.Remove(rowID, tile))
                            {
                                actionsTile.Add(new Action(ActionType.Take, tile, rowID));
                                solvedSeries.Add(Tuple.Create(tableUpdated, actionsTile));
                            }
                            else
                            {
                                stack.Push(Tuple.Create(tableUpdated, actionsTile));
                            }
                        }
                    }
                }
            }
        }

        _findTileCache[cacheKey] = CloneSolutions(solvedSeries);
        return solvedSeries;
    }

    public List<Tuple<Table, List<Action>>> TryCombine(Table tableInitial, Tile tile, ActionType method, Dictionary<int, int> forbiddenRows)
    {
        var intermediate = new List<Tuple<Table, List<Action>, int>>();

        foreach (var kv in tableInitial.GetRows())
        {
            int rowID = kv.Key;
            var row = kv.Value;
            if (forbiddenRows.ContainsKey(rowID)) continue;
            if (row.Type == RowType.Set) continue;

            var series = new Stack<Tuple<Table, List<Action>, int, bool, bool>>();
            Tile tileGoal = tile;

            if (tile.IsJoker)
            {
                if (row.HasJoker)
                {
                    var runRow = (Run)row;
                    if (runRow.Min > 3) series.Push(Tuple.Create(tableInitial.Clone(), new List<Action>(), rowID, true, false));
                    if (runRow.Max < 11) series.Push(Tuple.Create(tableInitial.Clone(), new List<Action>(), rowID, false, true));
                }
                else
                {
                    foreach (var t in ((Run)row).GetTiles())
                    {
                        if (t.IsJoker)
                        {
                            tileGoal = t;
                            break;
                        }
                    }
                }
            }

            if (tileGoal.Value.HasValue && ((Run)row).Color == tileGoal.Color)
            {
                var run = (Run)row;
                if (method == ActionType.Put)
                {
                    if (run.Min <= tileGoal.Value.Value && run.Max >= tileGoal.Value.Value)
                    {
                        bool needStart = (tileGoal.Value.Value - run.Min < 2);
                        bool needEnd = (run.Max - tileGoal.Value.Value < 2);
                        series.Push(Tuple.Create(tableInitial.Clone(), new List<Action>(), rowID, needStart, needEnd));
                    }
                }
                else
                {
                    if (run.Max == tileGoal.Value.Value && run.Min > 3)
                        series.Push(Tuple.Create(tableInitial.Clone(), new List<Action>(), rowID, true, false));
                    if (run.Min == tileGoal.Value.Value && run.Max < 11)
                        series.Push(Tuple.Create(tableInitial.Clone(), new List<Action>(), rowID, false, true));
                    if (run.Min < tileGoal.Value.Value && run.Max > tileGoal.Value.Value)
                    {
                        bool needStart = (tileGoal.Value.Value - run.Min < 3);
                        bool needEnd = (run.Max - tileGoal.Value.Value < 3);
                        series.Push(Tuple.Create(tableInitial.Clone(), new List<Action>(), rowID, needStart, needEnd));
                    }
                }
            }

            while (series.Count > 0)
            {
                var elem = series.Pop();
                var tableCombine = elem.Item1;
                var actions = elem.Item2;
                int curId = elem.Item3;
                bool start = elem.Item4;
                bool end = elem.Item5;

                var row1 = tableCombine.GetRow(curId) as Run;
                if (row1 == null) continue;

                foreach (var kv2 in tableCombine.GetRows())
                {
                    int rowID2 = kv2.Key;
                    var row2 = kv2.Value as Run;
                    if (row2 == null) continue;
                    if (forbiddenRows.ContainsKey(rowID2)) continue;
                    if (row2.Color != row1.Color) continue;

                    if (start)
                    {
                        if (row2.Max == row1.Min - 1)
                        {
                            int r1 = curId;
                            int r2 = rowID2;
                            if (r2 < r1) { int tmp = r1; r1 = r2; r2 = tmp; }

                            var tUpd = tableCombine.Clone();
                            tUpd.Combine(r1, r2);
                            var aUpd = new List<Action>(actions) { new Action(ActionType.Combine, r1, r2) };

                            if (end) series.Push(Tuple.Create(tUpd, aUpd, r1, false, true));
                            else intermediate.Add(Tuple.Create(tUpd, aUpd, r1));
                        }

                        if (row1.Min <= row2.Max && row1.Max - row2.Max >= 3)
                        {
                            var tileInfo = new Tile($"{row1.Min}{Tile.ColorToString(row1.Color)}");
                            var forb = new Dictionary<int, int>(forbiddenRows) { [curId] = 1, [rowID2] = 1 };
                            var seriesUpdated = AddTile(tableCombine, tileInfo, forb);

                            foreach (var upd in seriesUpdated)
                            {
                                var tUpd = upd.Item1.Clone();
                                if (tUpd.Remove(curId, tileInfo))
                                {
                                    var aFinal = new List<Action>(actions);
                                    var aWithTake = new List<Action>(upd.Item2);
                                    aWithTake.Insert(aWithTake.Count > 0 ? aWithTake.Count - 1 : 0, new Action(ActionType.Take, tileInfo, curId));
                                    aFinal.AddRange(aWithTake);
                                    series.Push(Tuple.Create(tUpd, aFinal, curId, start, end));
                                }
                            }
                        }
                    }

                    if (end)
                    {
                        if (row2.Min == row1.Max + 1)
                        {
                            int r1 = curId;
                            int r2 = rowID2;
                            if (r2 < r1) { int tmp = r1; r1 = r2; r2 = tmp; }

                            var tUpd = tableCombine.Clone();
                            tUpd.Combine(r1, r2);
                            var aUpd = new List<Action>(actions) { new Action(ActionType.Combine, r1, r2) };

                            if (start) series.Push(Tuple.Create(tUpd, aUpd, r1, true, false));
                            else intermediate.Add(Tuple.Create(tUpd, aUpd, r1));
                        }

                        if (row1.Max >= row2.Min && row2.Min - row1.Min >= 3)
                        {
                            var tileInfo = new Tile($"{row1.Max}{Tile.ColorToString(row1.Color)}");
                            var forb = new Dictionary<int, int>(forbiddenRows) { [curId] = 1, [rowID2] = 1 };
                            var seriesUpdated = AddTile(tableCombine, tileInfo, forb);

                            foreach (var upd in seriesUpdated)
                            {
                                var tUpd = upd.Item1.Clone();
                                if (tUpd.Remove(curId, tileInfo))
                                {
                                    var aFinal = new List<Action>(actions);
                                    var aWithTake = new List<Action>(upd.Item2);
                                    aWithTake.Insert(aWithTake.Count > 0 ? aWithTake.Count - 1 : 0, new Action(ActionType.Take, tileInfo, curId));
                                    aFinal.AddRange(aWithTake);
                                    series.Push(Tuple.Create(tUpd, aFinal, curId, start, end));
                                }
                            }
                        }
                    }
                }
            }
        }

        var solvedSeries = new List<Tuple<Table, List<Action>>>();
        foreach (var tuple in intermediate)
        {
            var tableCombine = tuple.Item1.Clone();
            var actions = new List<Action>(tuple.Item2);
            int rowID = tuple.Item3;

            if (method == ActionType.Put)
            {
                tableCombine.Insert(rowID, tile);
                actions.Add(new Action(ActionType.Put, tile, rowID));
            }
            else
            {
                tableCombine.Remove(rowID, tile);
                actions.Add(new Action(ActionType.Take, tile, rowID));
            }

            solvedSeries.Add(Tuple.Create(tableCombine, actions));
        }

        return solvedSeries;
    }
}

public abstract class Row
{
    public bool HasJoker { get; set; }
    public RowType Type { get; protected set; }
    public abstract int GetCount();
    public abstract string GetDisplay();
    public abstract Row Clone();
}

public class Set : Row
{
    public Dictionary<string, Tile> Tiles { get; private set; } = new Dictionary<string, Tile>();
    public HashSet<TileColor> Colors { get; private set; } = new HashSet<TileColor>();
    public int Value { get; private set; }

    public Set(IEnumerable<Tile> tiles, bool hasJoker)
    {
        Type = RowType.Set;
        HasJoker = hasJoker;
        foreach (var tile in tiles)
        {
            Value = tile.Value ?? 0;
            Colors.Add(tile.Color);
            Tiles[tile.GetName()] = tile;
        }
    }

    public Set(Set other)
    {
        this.Type = RowType.Set;
        this.HasJoker = other.HasJoker;
        this.Value = other.Value;
        this.Tiles = new Dictionary<string, Tile>(other.Tiles.Count);
        foreach (var kvp in other.Tiles)
        {
            this.Tiles.Add(kvp.Key, new Tile(kvp.Value));
        }
        this.Colors = new HashSet<TileColor>(other.Colors);
    }

    public override Row Clone() => new Set(this);
    public override int GetCount() => Tiles.Count + (HasJoker ? 1 : 0);

    public override string GetDisplay()
    {
        var output = new List<string>();
        foreach (var color in new[] { TileColor.B, TileColor.G, TileColor.R, TileColor.Y })
        {
            if (Colors.Contains(color)) output.Add($"{Value}{Tile.ColorToString(color)}");
        }
        if (HasJoker) output.Add("J");
        return string.Join(" ", output);
    }

    public List<Tuple<ManipulationMethod, Tile>> CouldInsert(Tile tile)
    {
        var list = new List<Tuple<ManipulationMethod, Tile>>();
        if (Tiles.Count == 0) return list;

        if ((tile.Value ?? -1) == Value && GetCount() == 4 && HasJoker && !Colors.Contains(tile.Color))
        {
            list.Add(Tuple.Create(ManipulationMethod.Remove, new Tile("J", true)));
            foreach (var color in Colors.ToList())
                list.Add(Tuple.Create(ManipulationMethod.Remove, new Tile($"{Value}{Tile.ColorToString(color)}")));
        }
        return list;
    }

    public List<Tuple<ManipulationMethod, Tile>> CouldTake(Tile tile)
    {
        var tiles = new List<Tuple<ManipulationMethod, Tile>>();
        if (GetCount() == 4 || Tiles.Count == 0) return tiles;

        if (Value == (tile.Value ?? -1) && Colors.Contains(tile.Color))
        {
            if (!HasJoker) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile("J", true)));

            foreach (var color in new[] { TileColor.B, TileColor.G, TileColor.R, TileColor.Y })
            {
                if (!Colors.Contains(color)) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Value}{Tile.ColorToString(color)}")));
            }
        }

        if (tile.IsJoker && HasJoker)
        {
            foreach (var color in new[] { TileColor.B, TileColor.G, TileColor.R, TileColor.Y })
            {
                if (!Colors.Contains(color)) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Value}{Tile.ColorToString(color)}")));
            }
        }

        return tiles;
    }

    public void Insert(Tile tile)
    {
        Colors.Add(tile.Color);
        Tiles[tile.GetName()] = tile;
    }

    public void Remove(Tile tile)
    {
        Colors.Remove(tile.Color);
        Tiles.Remove(tile.GetName());
    }
}

public class Run : Row
{
    private LinkedList<Tile> tileList = new LinkedList<Tile>();
    private Dictionary<string, LinkedListNode<Tile>> tileMap = new Dictionary<string, LinkedListNode<Tile>>();

    public int Min { get; set; } = int.MaxValue;
    public int Max { get; set; } = int.MinValue;
    public TileColor Color { get; set; }

    public Run(IEnumerable<Tile> tiles, bool hasJoker)
    {
        Type = RowType.Run;
        HasJoker = hasJoker;
        bool first = true;
        foreach (var tile in tiles.OrderBy(t => t.Value))
        {
            var node = tileList.AddLast(tile);
            tileMap[tile.GetName()] = node;
            if (first)
            {
                Color = tile.Color;
                first = false;
            }
            Min = Math.Min(Min, tile.Value ?? Min);
            Max = Math.Max(Max, tile.Value ?? Max);
        }
    }

    public Run(Run other)
    {
        this.Type = RowType.Run;
        this.HasJoker = other.HasJoker;
        this.Min = other.Min;
        this.Max = other.Max;
        this.Color = other.Color;
        this.tileMap = new Dictionary<string, LinkedListNode<Tile>>(other.tileMap.Count);
        this.tileList = new LinkedList<Tile>();
        foreach (var tile in other.tileList)
        {
            var newTile = new Tile(tile);
            var node = this.tileList.AddLast(newTile);
            this.tileMap[newTile.GetName()] = node;
        }
    }
    public override Row Clone() => new Run(this);
    public override int GetCount() => tileList.Count + (HasJoker ? 1 : 0);
    public IEnumerable<Tile> GetTiles() => tileList;

    private Run() { Type = RowType.Run; }
    public override string GetDisplay()
    {
        var output = new List<string>();
        foreach (var tile in tileList)
        {
            output.Add(tile.GetName());
        }
        if (HasJoker) output.Add("J");
        return string.Join(" ", output);
    }
    public void InsertEnd(Tile tile)
    {
        Max = Math.Max(Max, tile.Value ?? Max);
        var node = tileList.AddLast(tile);
        tileMap[tile.GetName()] = node;
    }
    public void InsertStart(Tile tile)
    {
        Min = Math.Min(Min, tile.Value ?? Min);
        var node = tileList.AddFirst(tile);
        tileMap[tile.GetName()] = node;
    }
    public Tile RemoveEnd()
    {
        Max--;
        var last = tileList.Last.Value;
        tileMap.Remove(tileList.Last.Value.GetName());
        tileList.RemoveLast();

        if (tileList.Count > 0)
        {
            var newLast = tileList.Last.Value;
            if (newLast.IsJoker)
            {
                HasJoker = true;
                Max--;
                tileMap.Remove(newLast.GetName());
                tileList.RemoveLast();
            }
        }
        return last;
    }
    public Tile RemoveStart()
    {
        Min++;
        var first = tileList.First.Value;
        tileMap.Remove(tileList.First.Value.GetName());
        tileList.RemoveFirst();

        if (tileList.Count > 0)
        {
            var newFirst = tileList.First.Value;
            if (newFirst.IsJoker)
            {
                HasJoker = true;
                Min++;
                tileMap.Remove(newFirst.GetName());
                tileList.RemoveFirst();
            }
        }
        return first;
    }
    public List<Tuple<ManipulationMethod, Tile>> CouldInsert(Tile tile)
    {
        var tiles = new List<Tuple<ManipulationMethod, Tile>>();
        if (Color != tile.Color) return tiles;
        if (tileList.Count == 0) return tiles;

        if (HasJoker)
        {
            if ((tile.Value ?? int.MinValue) < Min - 2) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Min - 2}{Tile.ColorToString(tile.Color)}")));
            if ((tile.Value ?? int.MaxValue) > Max + 2) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Max + 2}{Tile.ColorToString(tile.Color)}")));
        }
        else
        {
            bool hasInsideJoker = tileList.Any(t => t.IsJoker);
            if (!hasInsideJoker) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile("J", true)));
        }

        if ((tile.Value ?? int.MinValue) < Min - 1) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Min - 1}{Tile.ColorToString(tile.Color)}")));
        if ((tile.Value ?? int.MaxValue) > Max + 1) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Max + 1}{Tile.ColorToString(tile.Color)}")));

        if (tile.Value.HasValue && tile.Value.Value >= Min && tile.Value.Value <= Max &&
          Math.Min(2, tile.Value.Value - Min) + Math.Min(2, Max - tile.Value.Value) + (HasJoker ? 1 : 0) < 4)
        {
            int lo = Math.Max(3, Math.Max(tile.Value.Value - (HasJoker ? 2 : 1), Min));
            int hi = Math.Min(11, Math.Min(tile.Value.Value + (HasJoker ? 2 : 1), Max));
            for (int i = lo; i <= hi; i++)
            {
                int left = Math.Min(2, i - Min);
                int right = Math.Min(2, Max - i);

                if (left + right + (HasJoker ? 1 : 0) >= 4)
                    tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{i}{Tile.ColorToString(tile.Color)}")));
                else
                {
                    if (left != 2 && i - left - 1 > 0) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{i - left - 1}{Tile.ColorToString(tile.Color)}")));
                    if (right != 2 && i + right + 1 < 14) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{i + right + 1}{Tile.ColorToString(tile.Color)}")));
                }
            }
        }

        return tiles;
    }
    public List<Tuple<ManipulationMethod, Tile>> CouldTake(Tile tile)
    {
        var tiles = new List<Tuple<ManipulationMethod, Tile>>();
        if (tileList.Count == 0) return tiles;

        if (tile.IsJoker)
        {
            if (HasJoker)
            {
                if (GetCount() == 3)
                {
                    if (Max != 13) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Max + 1}{Tile.ColorToString(Color)}")));
                    if (Min != 1) tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Min - 1}{Tile.ColorToString(Color)}")));
                }
            }
            else
            {
                foreach (var runTile in GetTiles())
                {
                    if (runTile.IsJoker)
                    {
                        tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{runTile.Value}{Tile.ColorToString(runTile.Color)}")));
                        if (Max - (runTile.Value ?? Max) >= 3) tiles.Add(Tuple.Create(ManipulationMethod.Remove, new Tile($"{Min}{Tile.ColorToString(runTile.Color)}")));
                        if ((runTile.Value ?? Min) - Min >= 3) tiles.Add(Tuple.Create(ManipulationMethod.Remove, new Tile($"{Max}{Tile.ColorToString(runTile.Color)}")));
                    }
                }
            }
        }
        else if (Color == tile.Color)
        {
            if ((tile.Value ?? int.MinValue) == Min && GetCount() == 3 && Max != 13)
                tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Max + 1}{Tile.ColorToString(Color)}")));
            if ((tile.Value ?? int.MinValue) == Max && GetCount() == 3 && Min != 1)
                tiles.Add(Tuple.Create(ManipulationMethod.Find, new Tile($"{Min - 1}{Tile.ColorToString(Color)}")));

            if (Min < (tile.Value ?? int.MinValue) && Max > (tile.Value ?? int.MaxValue))
            {
                if (Max - (tile.Value ?? Max) >= 3) tiles.Add(Tuple.Create(ManipulationMethod.Remove, new Tile($"{Min}{Tile.ColorToString(tile.Color)}")));
                if ((tile.Value ?? Min) - Min >= 3) tiles.Add(Tuple.Create(ManipulationMethod.Remove, new Tile($"{Max}{Tile.ColorToString(tile.Color)}")));
            }
        }

        return tiles;
    }
}

public class Table
{
    private Dictionary<int, Row> rows = new Dictionary<int, Row>();
    private int nextRow = 0;

    public Table(List<string[]> initialRows)
    {
        foreach (var rowData in initialRows)
        {
            int rowID = int.Parse(rowData[0]);
            var tileNames = rowData.Skip(1).ToList();

            var tiles = new List<Tile>();
            var colors = new HashSet<TileColor>();
            Tile previousTile = null;
            bool hasJoker = false;

            for (int i = 0; i < tileNames.Count; i++)
            {
                var tileName = tileNames[i];
                Tile tile;

                if (tileName == "J")
                {
                    if (i == tileNames.Count - 1)
                    {
                        hasJoker = true;
                        break;
                    }
                    else
                    {
                        tile = new Tile($"{(previousTile.Value.GetValueOrDefault() + 1)}{Tile.ColorToString(previousTile.Color)}", true);
                    }
                }
                else
                {
                    tile = new Tile(tileName);
                }

                colors.Add(tile.Color);
                previousTile = tile;
                tiles.Add(tile);
            }

            rows[rowID] = (colors.Count == 1) ? (Row)new Run(tiles, hasJoker) : new Set(tiles, hasJoker);
            nextRow = Math.Max(nextRow, rowID + 1);
        }
    }

    private Table() { }
    public Table Clone()
    {
        var t = new Table { nextRow = this.nextRow };
        t.rows = new Dictionary<int, Row>(this.rows.Count);
        foreach (var kv in this.rows)
        {
            t.rows[kv.Key] = kv.Value.Clone();
        }
        return t;
    }

    public IReadOnlyDictionary<int, Row> GetRows() => rows;
    public Row GetRow(int rowID) => rows.TryGetValue(rowID, out var row) ? row : null;

    public void Combine(int r1, int r2)
    {
        var row1 = rows[r1] as Run;
        var row2 = rows[r2] as Run;

        if (row1.Max == row2.Min - 1)
        {
            foreach (var tile in row2.GetTiles()) row1.InsertEnd(tile);
        }
        else
        {
            foreach (var tile in row2.GetTiles().Reverse()) row1.InsertStart(tile);
        }
        rows.Remove(r2);
    }
    public List<int> Insert(int rowID, Tile tile)
    {
        var result = new List<int>();
        if (!rows.ContainsKey(rowID)) return result;
        var row = rows[rowID];

        if (tile.IsJoker)
        {
            if (row.GetCount() != (row.Type == RowType.Set ? 4 : 13))
            {
                row.HasJoker = true;
                result.Add(rowID);
            }
            return result;
        }

        if (row is Set set)
        {
            if (set.GetCount() != 4 && (tile.Value ?? -1) == set.Value && !set.Colors.Contains(tile.Color))
            {
                set.Insert(tile);
                result.Add(rowID);
            }
        }
        else if (row is Run run && run.Color == tile.Color)
        {
            if ((tile.Value ?? int.MinValue) == run.Min - 1)
            {
                run.InsertStart(tile);
                result.Add(rowID);
            }
            else if ((tile.Value ?? int.MaxValue) == run.Max + 1)
            {
                run.InsertEnd(tile);
                result.Add(rowID);
            }
            else if (run.HasJoker && (tile.Value ?? int.MinValue) == run.Min - 2)
            {
                var jokerTile = new Tile($"{(tile.Value.Value + 1)}{Tile.ColorToString(tile.Color)}", true);
                run.InsertStart(jokerTile);
                run.InsertStart(tile);
                run.HasJoker = false;
                result.Add(rowID);
            }
            else if (run.HasJoker && (tile.Value ?? int.MaxValue) == run.Max + 2)
            {
                var jokerTile = new Tile($"{(tile.Value.Value - 1)}{Tile.ColorToString(tile.Color)}", true);
                run.InsertEnd(jokerTile);
                run.InsertEnd(tile);
                run.HasJoker = false;
                result.Add(rowID);
            }
            else
            {
                var inside = run.GetTiles().FirstOrDefault(t => t.IsJoker && t.Value == tile.Value);
                if (inside != null)
                {
                    inside.IsJoker = false;
                    run.HasJoker = true;
                    result.Add(rowID);
                }
                else if ((tile.Value ?? int.MinValue) > run.Min && (tile.Value ?? int.MaxValue) < run.Max &&
                    (Math.Min((tile.Value ?? 0) - run.Min, 2) + Math.Min(run.Max - (tile.Value ?? 0), 2) + (run.HasJoker ? 1 : 0) == 4))
                {
                    var tilesRemoved = new List<Tile>();
                    while (run.Max >= (tile.Value ?? 0))
                        tilesRemoved.Add(run.RemoveEnd());
                    run.InsertEnd(tile);
                    var newRun = new Run(tilesRemoved.AsEnumerable().Reverse(), false);
                    if (run.HasJoker && tilesRemoved.Count == 2)
                    {
                        run.HasJoker = false;
                        newRun.HasJoker = true;
                    }
                    int newRowID = nextRow++;
                    rows[newRowID] = newRun;
                    result.Add(rowID);
                    result.Add(newRowID);
                }
            }
        }
        return result;
    }

    public void OutputRows()
    {
        foreach (var kv in rows.OrderBy(k => k.Key))
        {
            Console.WriteLine($"{kv.Key} {kv.Value.GetDisplay()}");
        }
    }
    public bool Remove(int rowID, Tile tile)
    {
        if (!rows.ContainsKey(rowID)) return false;
        var row = rows[rowID];

        if (tile.IsJoker)
        {
            if (row.HasJoker && row.GetCount() >= 4)
            {
                row.HasJoker = false;
                return true;
            }
            else if (row is Run run && row.GetCount() >= 7)
            {
                foreach (var rt in run.GetTiles())
                {
                    if (rt.IsJoker)
                    {
                        int val = rt.Value ?? 0;
                        if (val - 3 >= run.Min && val + 3 <= run.Max)
                        {
                            var tiles = new List<Tile>();
                            while (run.Max > (rt.Value ?? 0)) tiles.Add(run.RemoveEnd());
                            run.HasJoker = false;
                            rows[nextRow++] = new Run(tiles.AsEnumerable().Reverse(), false);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        if (row is Set set)
        {
            if (set.GetCount() == 4 && set.Value == (tile.Value ?? -1) && set.Colors.Contains(tile.Color))
            {
                set.Remove(tile);
                return true;
            }
        }
        else if (row is Run run2 && run2.Color == tile.Color)
        {
            if (row.GetCount() >= 4 && (tile.Value ?? int.MinValue) == run2.Min)
            {
                run2.RemoveStart();
                return true;
            }
            else if (row.GetCount() >= 4 && (tile.Value ?? int.MinValue) == run2.Max)
            {
                run2.RemoveEnd();
                return true;
            }
            else if (row.GetCount() >= 7 &&
                (tile.Value ?? 0) - 3 >= run2.Min &&
                (tile.Value ?? 0) + 3 <= run2.Max)
            {
                var tiles = new List<Tile>();
                while (run2.Max > (tile.Value ?? 0)) tiles.Add(run2.RemoveEnd());

                run2.RemoveEnd();

                rows[nextRow++] = new Run(tiles.AsEnumerable().Reverse(), false);
                return true;
            }
        }
        return false;
    }
}

public class Tile
{
    public int? Value { get; set; }
    public TileColor Color { get; set; }
    public bool IsJoker { get; set; }

    public Tile(string tileStr, bool isJoker = false)
    {
        IsJoker = isJoker;
        if (tileStr == "J")
        {
            Value = null;
            Color = TileColor.Joker;
            IsJoker = true;
            return;
        }

        char lastChar = tileStr[tileStr.Length - 1];
        string valueStr = tileStr.Substring(0, tileStr.Length - 1);

        Value = int.Parse(valueStr);
        Color = StringToColor(lastChar.ToString());
    }

    public Tile(Tile other)
    {
        this.Value = other.Value;
        this.Color = other.Color;
        this.IsJoker = other.IsJoker;
    }

    public string GetName()
    {
        if (this.IsJoker) return "J";
        return $"{Value}{ColorToString(Color)}";
    }

    public static string ColorToString(TileColor color) => color switch
    {
        TileColor.B => "B",
        TileColor.G => "G",
        TileColor.R => "R",
        TileColor.Y => "Y",
        _ => ""
    };

    public static TileColor StringToColor(string s) => s switch
    {
        "B" => TileColor.B,
        "G" => TileColor.G,
        "R" => TileColor.R,
        "Y" => TileColor.Y,
        _ => TileColor.Joker
    };
}