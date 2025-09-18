using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public class Solution
{
    private static int[] _finalAnswer;
    private static readonly Stopwatch _stopwatch = new Stopwatch();

    public static void Main(string[] args)
    {
        _stopwatch.Start();
        var numPuzzles = int.Parse(Console.ReadLine());
        var gridStrings = new string[numPuzzles];
        var cageValueStrings = new string[numPuzzles];
        for (var i = 0; i < numPuzzles; i++) gridStrings[i] = Console.ReadLine();
        for (var i = 0; i < numPuzzles; i++) cageValueStrings[i] = Console.ReadLine();
        
        _finalAnswer = new int[81];
        KillerSudokuSolver.InitializeStaticData();
        
        var totalCages = 0L;
        for (var i = 0; i < numPuzzles; i++)
        {
            var puzzleTime = Stopwatch.StartNew();
            var solver = new KillerSudokuSolver(gridStrings[i], cageValueStrings[i]);
            solver.SolvePuzzle();
            totalCages += solver.GetCageCount();
            puzzleTime.Stop();
            Console.Error.WriteLine($"[DEBUG] Puzzle {i} solved in {puzzleTime.ElapsedMilliseconds} ms. Cages: {solver.GetCageCount()}");
        }

        var sb = new StringBuilder();
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                sb.Append(_finalAnswer[r * 9 + c]);
                if (c < 8) sb.Append(" ");
            }
            sb.AppendLine();
        }
        Console.Write(sb.ToString());
        
        _stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Total execution time: {_stopwatch.ElapsedMilliseconds} ms. Total Cages: {totalCages}");
    }

    public static void AddToFinalAnswer(int[] solvedGrid)
    {
        for (var i = 0; i < 81; i++)
        {
            _finalAnswer[i] += solvedGrid[i];
        }
    }
}

public class KillerSudokuSolver
{
    private const int FULL_DIGITS = 511;
    private const int MAX_SEARCH_TURNS = 2;

    private static readonly int[] DIGIT_TO_BINARY = { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256 };
    private static readonly int[] BINARY_TO_DIGIT = new int[512];
    private static readonly int[][] DEFAULT_CAGES_POSITIONS;
    private static readonly int[,] REGIONS_LIMITS = {
        {0, 0, 2, 2}, {3, 0, 5, 2}, {6, 0, 8, 2},
        {0, 3, 2, 5}, {3, 3, 5, 5}, {6, 3, 8, 5},
        {0, 6, 2, 8}, {3, 6, 5, 8}, {6, 6, 8, 8},
    };
    private static readonly int[][] REGIONS_COMBI = {
        new[]{0}, new[]{1}, new[]{2}, new[]{3}, new[]{4}, new[]{5}, new[]{6}, new[]{7}, new[]{8}, 
        new[]{0, 1}, new[]{1, 2}, new[]{3, 4}, new[]{4, 5}, new[]{6, 7}, new[]{7, 8}, new[]{0, 3}, new[]{1, 4}, new[]{2, 5}, new[]{3, 6}, new[]{4, 7}, new[]{5, 8}, 
        new[]{3, 0, 1}, new[]{1, 2, 5}, new[]{5, 8, 7}, new[]{7, 6, 3}, new[]{4, 3, 0}, new[]{4, 3, 6}, new[]{4, 1, 0}, new[]{4, 1, 2}, new[]{4, 5, 2}, new[]{4, 5, 8}, new[]{4, 7, 8}, new[]{4, 7, 6}, 
        new[]{0, 1, 3, 4}, new[]{1, 2, 4, 5}, new[]{3, 4, 6, 7}, new[]{4, 5, 7 ,8}
    };
    private static readonly int[] MAX_MASKS = new int[10];
    private static readonly int[] MIN_MASKS = new int[10];
    private static MaskInfo[] _maskInfos;
    private static List<int>[] _affectedPositionsBase;
    private static Dictionary<(int sum, int count, int mask), (int sumDigits, int forcedDigits)> _sumPermutationsMemo;
    
    private readonly string _gridString;
    private readonly string _cageValuesString;
    private List<Cage> _cages;
    private List<int>[] _cagesMatch;
    private HashSet<string> _knownCages;
    private List<int>[] _affectedPositions;
    
    public int GetCageCount() => _cages.Count;

    private class Cage { public int Sum; public int Count; public HashSet<int> Positions; public bool UniqueDigits; public Cage(int s, int c, IEnumerable<int> p, bool u) { Sum = s; Count = c; Positions = new HashSet<int>(p); UniqueDigits = u; } public Cage Clone() => new Cage(Sum, Count, Positions, UniqueDigits); }
    private struct MaskInfo { public int Count; public int Min; public int Max; public Dictionary<int, int> Digits; }
    private struct UnitCageInfo { public int TotalSum; public List<(int sum, HashSet<int> pos)> Cages; }
    
    static KillerSudokuSolver()
    {
        for (var i = 1; i <= 9; i++) BINARY_TO_DIGIT[DIGIT_TO_BINARY[i]] = i;
        MAX_MASKS[0] = 0;
        for (var i = 1; i <= 9; i++) MAX_MASKS[i] = MAX_MASKS[i-1] | DIGIT_TO_BINARY[i];
        MIN_MASKS[0] = FULL_DIGITS;
        for (var i = 1; i <= 9; i++) { var mask = 0; for(var j = i; j <= 9; j++) mask |= DIGIT_TO_BINARY[j]; MIN_MASKS[i] = mask; }
        var rows = Enumerable.Range(0, 9).Select(r => Enumerable.Range(0, 9).Select(c => r * 9 + c).ToArray()).ToArray();
        var cols = Enumerable.Range(0, 9).Select(c => Enumerable.Range(0, 9).Select(r => r * 9 + c).ToArray()).ToArray();
        var regions = new int[9][];
        for (var i = 0; i < 9; i++)
        {
            regions[i] = new int[9];
            var startRow = (i / 3) * 3; var startCol = (i % 3) * 3; var k = 0;
            for (var r = 0; r < 3; r++) for (var c = 0; c < 3; c++) regions[i][k++] = (startRow + r) * 9 + (startCol + c);
        }
        DEFAULT_CAGES_POSITIONS = rows.Concat(cols).Concat(regions).ToArray();
    }
    
    public static void InitializeStaticData()
    {
        if (_maskInfos != null) return;
        _maskInfos = new MaskInfo[FULL_DIGITS + 1];
        GenerateMaskInfos(0, 1, new Dictionary<int, int>());
        _affectedPositionsBase = new List<int>[81];
        for (var i = 0; i < 81; i++) _affectedPositionsBase[i] = new List<int>();
        GenerateAffectedPositionsBase();
        _sumPermutationsMemo = new Dictionary<(int, int, int), (int, int)>();
    }

    public KillerSudokuSolver(string gridString, string cageValuesString) { _gridString = gridString; _cageValuesString = cageValuesString; }
    
    public void SolvePuzzle()
    {
        var possibleDigits = new int[81]; Array.Fill(possibleDigits, FULL_DIGITS);
        var positionsToFind = new HashSet<int>(Enumerable.Range(0, 81));
        DeriveCages();
        foreach (var cage in _cages.Where(c => c.UniqueDigits && c.Count is > 0 and < 9))
        {
            var (sumDigits, _) = FindSumPermutations(cage.Sum, cage.Count, FULL_DIGITS);
            if (sumDigits is > 0 and not FULL_DIGITS) foreach (var pos in cage.Positions) possibleDigits[pos] &= sumDigits;
        }
        RecursiveSolve(new int[81], possibleDigits, _cages, positionsToFind);
    }

    private void RecursiveSolve(int[] grid, int[] possibleDigits, List<Cage> cages, HashSet<int> positionsToFind)
    {
        var q = new Queue<int>();
        if (!Propagate(grid, possibleDigits, cages, positionsToFind, q)) return;
        
        if (positionsToFind.Any())
        {
            var bestPos = -1; var minOptions = 10;
            foreach (var pos in positionsToFind)
            {
                var count = _maskInfos[possibleDigits[pos]].Count;
                if (count < minOptions) { minOptions = count; bestPos = pos; }
            }
            var possibilities = possibleDigits[bestPos];
            foreach (var digit in _maskInfos[possibilities].Digits.Keys)
            {
                var nextGrid = (int[])grid.Clone(); var nextPossibleDigits = (int[])possibleDigits.Clone();
                var nextCages = cages.Select(c => c.Clone()).ToList(); var nextPositionsToFind = new HashSet<int>(positionsToFind);
                nextPossibleDigits[bestPos] = DIGIT_TO_BINARY[digit];
                RecursiveSolve(nextGrid, nextPossibleDigits, nextCages, nextPositionsToFind);
            }
        }
        else { Solution.AddToFinalAnswer(grid); }
    }

    private bool Propagate(int[] grid, int[] possibleDigits, List<Cage> cages, HashSet<int> positionsToFind, Queue<int> q)
    {
        var turn = 0;
        while (true)
        {
            var changed = true;
            while (changed)
            {
                changed = false;
                var foundPos = -1;
                foreach (var pos in positionsToFind) { if (_maskInfos[possibleDigits[pos]].Count == 1) { foundPos = pos; break; } }
                if (foundPos != -1)
                {
                    if (!ApplyValue(foundPos, BINARY_TO_DIGIT[possibleDigits[foundPos]], grid, possibleDigits, cages, positionsToFind, q)) return false;
                    changed = true; turn = 0;
                    while (q.Count > 0)
                    {
                        var updatedPos = q.Dequeue();
                        if (_maskInfos[possibleDigits[updatedPos]].Count == 1)
                        { if (!ApplyValue(updatedPos, BINARY_TO_DIGIT[possibleDigits[updatedPos]], grid, possibleDigits, cages, positionsToFind, q)) return false; }
                    }
                }
            }
            if (!positionsToFind.Any() || turn >= MAX_SEARCH_TURNS) break;
            var logicMadeChange = false;
            foreach (var cage in cages) { if (cage.Count == 0) continue; if (!ApplyCageLogic(cage, possibleDigits, ref logicMadeChange)) return false; }
            if (logicMadeChange) turn = 0; else turn++;
        }
        return true;
    }
    
    private bool ApplyCageLogic(Cage cage, int[] possibleDigits, ref bool logicMadeChange)
    {
        if (cage.Count == 1)
        {
            var pos = cage.Positions.First(); if (cage.Sum is < 1 or > 9 || (possibleDigits[pos] & DIGIT_TO_BINARY[cage.Sum]) == 0) return false;
            if (possibleDigits[pos] != DIGIT_TO_BINARY[cage.Sum]) { possibleDigits[pos] = DIGIT_TO_BINARY[cage.Sum]; logicMadeChange = true; } return true;
        }
        foreach (var pos1 in cage.Positions)
        {
            var sumMin = cage.Sum; var sumMax = cage.Sum;
            foreach (var pos2 in cage.Positions.Where(p => p != pos1)) { sumMin -= _maskInfos[possibleDigits[pos2]].Max; sumMax -= _maskInfos[possibleDigits[pos2]].Min; }
            if (sumMax < 1 || sumMin > 9) return false;
            var newMask = possibleDigits[pos1]; if (sumMax <= 9) newMask &= MAX_MASKS[sumMax]; if (sumMin >= 1) newMask &= MIN_MASKS[sumMin];
            if (newMask == 0) return false; if (possibleDigits[pos1] == newMask) continue;
            possibleDigits[pos1] = newMask; logicMadeChange = true;
        }
        if (!cage.UniqueDigits) return true;
        var availableDigits = 0; var dispersion = new Dictionary<int, List<int>>();
        foreach (var pos in cage.Positions)
        {
            var mask = possibleDigits[pos]; availableDigits |= mask; if (!dispersion.ContainsKey(mask)) dispersion[mask] = new List<int>(); dispersion[mask].Add(pos);
        }
        foreach (var (mask, posList) in dispersion)
        {
            if (_maskInfos[mask].Count != posList.Count) continue;
            foreach (var pos in cage.Positions.Except(posList))
            { if ((possibleDigits[pos] & mask) == 0) continue; possibleDigits[pos] &= ~mask; if (possibleDigits[pos] == 0) return false; logicMadeChange = true; }
        }
        var (digitsSum, forcedDigits) = FindSumPermutations(cage.Sum, cage.Count, availableDigits);
        if (digitsSum == 0 && cage.Sum > 0) return false;
        if (forcedDigits != 0)
        {
            foreach (var digit in _maskInfos[forcedDigits].Digits.Keys)
            {
                int? uniquePos = null; var canPlace = true;
                foreach (var pos in cage.Positions.Where(pos => (possibleDigits[pos] & DIGIT_TO_BINARY[digit]) != 0))
                { if (uniquePos.HasValue) { canPlace = false; break; } uniquePos = pos; }
                if (!canPlace || !uniquePos.HasValue || possibleDigits[uniquePos.Value] == DIGIT_TO_BINARY[digit]) continue;
                possibleDigits[uniquePos.Value] = DIGIT_TO_BINARY[digit]; logicMadeChange = true;
            }
        }
        if (digitsSum is > 0 and not FULL_DIGITS)
        { foreach (var pos in cage.Positions) { if ((possibleDigits[pos] & ~digitsSum) == 0) continue; possibleDigits[pos] &= digitsSum; if (possibleDigits[pos] == 0) return false; logicMadeChange = true; } }
        return true;
    }

    private bool ApplyValue(int pos, int val, int[] grid, int[] possibleDigits, List<Cage> cages, HashSet<int> positionsToFind, Queue<int> q)
    {
        grid[pos] = val; positionsToFind.Remove(pos);
        var binaryVal = DIGIT_TO_BINARY[val]; var invBinaryVal = ~binaryVal;
        foreach (var peer in _affectedPositions[pos])
        {
            if (!positionsToFind.Contains(peer) || (possibleDigits[peer] & binaryVal) == 0) continue;
            possibleDigits[peer] &= invBinaryVal; if (possibleDigits[peer] == 0) return false;
            if (_maskInfos[possibleDigits[peer]].Count == 1) q.Enqueue(peer);
        }
        foreach (var cageIdx in _cagesMatch[pos])
        {
            var cage = cages[cageIdx]; if (!cage.Positions.Contains(pos)) continue;
            cage.Sum -= val; cage.Count--; cage.Positions.Remove(pos);
            if (cage.Count == 0 && cage.Sum != 0) return false; if (cage.Count > 0 && cage.Sum <= 0) return false;
        }
        return true;
    }
    
    private void DeriveCages()
    {
        _cages = new List<Cage>(); _cagesMatch = new List<int>[81]; for(var i=0; i<81; i++) _cagesMatch[i] = new List<int>();
        _knownCages = new HashSet<string>(); _affectedPositions = new List<int>[81]; for (var i = 0; i < 81; i++) _affectedPositions[i] = new List<int>(_affectedPositionsBase[i]);
        foreach (var positions in DEFAULT_CAGES_POSITIONS) CreateCage(positions, 45, true);
        var cagePositionsByName = new Dictionary<char, List<int>>();
        for (var i = 0; i < 81; i++) { var name = _gridString[i]; if (!cagePositionsByName.ContainsKey(name)) cagePositionsByName[name] = new List<int>(); cagePositionsByName[name].Add(i); }
        var cageSums = new Dictionary<char, int>();
        foreach (var part in _cageValuesString.Split(' ', StringSplitOptions.RemoveEmptyEntries)) { var kvp = part.Split('='); cageSums[kvp[0][0]] = int.Parse(kvp[1]); }
        foreach (var (name, sum) in cageSums)
        {
            var positions = cagePositionsByName[name]; CreateCage(positions, sum, true);
            foreach(var p1 in positions) foreach(var p2 in positions) { if (p1 != p2 && !_affectedPositions[p1].Contains(p2)) _affectedPositions[p1].Add(p2); }
        }
        var cageData = cageSums.ToDictionary(kv => kv.Key, kv => (kv.Value, new HashSet<int>(cagePositionsByName[kv.Key])));
        DeriveCagesForUnit(9, i => Enumerable.Range(0, 9).Select(j => i * 9 + j), cageData);
        DeriveCagesForUnit(9, i => Enumerable.Range(0, 9).Select(j => j * 9 + i), cageData);
        DeriveCagesForRegions(cageData);
    }
    
    private void DeriveCagesForUnit(int unitCount, Func<int, IEnumerable<int>> getUnitPositions, Dictionary<char, (int sum, HashSet<int> pos)> cageData)
    {
        var unitPositions = Enumerable.Range(0, unitCount).Select(i => new HashSet<int>(getUnitPositions(i))).ToArray();
        var fullCagesByUnit = new UnitCageInfo[unitCount]; var partialCagesByUnit = new Dictionary<char, (int sum, HashSet<int> pos)>[unitCount];
        for (var i = 0; i < unitCount; i++)
        {
            fullCagesByUnit[i] = new UnitCageInfo { Cages = new List<(int, HashSet<int>)>() }; partialCagesByUnit[i] = new Dictionary<char, (int, HashSet<int>)>();
            var uniqueCageNames = new HashSet<char>(unitPositions[i].Select(p => _gridString[p]));
            foreach (var name in uniqueCageNames)
            {
                var (sum, pos) = cageData[name];
                if (pos.IsSubsetOf(unitPositions[i])) { fullCagesByUnit[i].Cages.Add((sum, pos)); fullCagesByUnit[i].TotalSum += sum; }
                else { partialCagesByUnit[i][name] = (sum, pos); }
            }
            if (fullCagesByUnit[i].Cages.Count <= 1) continue;
            for (var size = 1; size < fullCagesByUnit[i].Cages.Count; size++) for (var start = 0; start <= fullCagesByUnit[i].Cages.Count - size; start++)
            {
                var usedCages = fullCagesByUnit[i].Cages.Skip(start).Take(size); var listPositions = new HashSet<int>(); var sumCages = 0;
                foreach (var (s, p) in usedCages) { listPositions.UnionWith(p); sumCages += s; }
                CreateCage(unitPositions[i].Except(listPositions), 45 - sumCages, true);
            }
        }
        foreach (var direction in new[] { 1, -1 })
        {
            var startIdx = direction == 1 ? 0 : unitCount - 1; var endIdx = direction == 1 ? unitCount : -1;
            for (var i = startIdx; i != endIdx; i += direction)
            {
                var fullCagesSum = 0; var partialCagesGroup = new Dictionary<char, (int sum, HashSet<int> pos)>(); var groupPositions = new HashSet<int>(); var totalSum = 0;
                for (var j = startIdx; (direction == 1 ? j <= i : j >= i); j += direction)
                {
                    fullCagesSum += fullCagesByUnit[j].TotalSum; foreach(var (k, v) in partialCagesByUnit[j]) partialCagesGroup[k] = v;
                    groupPositions.UnionWith(unitPositions[j]); totalSum += 45;
                }
                var newlyFull = partialCagesGroup.Keys.Where(name => cageData[name].pos.IsSubsetOf(groupPositions)).ToList();
                foreach(var name in newlyFull) { fullCagesSum += partialCagesGroup[name].sum; partialCagesGroup.Remove(name); }
                if (!partialCagesGroup.Any()) continue;
                var partialPositions = new HashSet<int>(); var partialSum = 0; foreach (var (s, p) in partialCagesGroup.Values) { partialPositions.UnionWith(p); partialSum += s; }
                var outside = new HashSet<int>(partialPositions); outside.ExceptWith(groupPositions); CreateCage(outside, partialSum - totalSum + fullCagesSum, partialCagesGroup.Count == 1 || CheckUniqueDigits(outside));
                var inside = new HashSet<int>(partialPositions); inside.IntersectWith(groupPositions); CreateCage(inside, totalSum - fullCagesSum, partialCagesGroup.Count == 1 || CheckUniqueDigits(inside));
            }
        }
    }

    private void DeriveCagesForRegions(Dictionary<char, (int sum, HashSet<int> pos)> cageData)
    {
        var regionsPos = new HashSet<int>[9];
        for(var i=0; i<9; i++) { regionsPos[i] = new HashSet<int>(); for (var r = REGIONS_LIMITS[i, 1]; r <= REGIONS_LIMITS[i, 3]; r++) for (var c = REGIONS_LIMITS[i, 0]; c <= REGIONS_LIMITS[i, 2]; c++) regionsPos[i].Add(r * 9 + c); }
        var fullCagesByRegion = new UnitCageInfo[9]; var partialCagesByRegion = new Dictionary<char, (int sum, HashSet<int> pos)>[9];
        for(var i=0; i<9; i++)
        {
            fullCagesByRegion[i] = new UnitCageInfo { Cages = new List<(int sum, HashSet<int> pos)>() }; partialCagesByRegion[i] = new Dictionary<char, (int, HashSet<int>)>();
            var uniqueCageNames = new HashSet<char>(regionsPos[i].Select(p => _gridString[p]));
            foreach (var name in uniqueCageNames)
            {
                var (sum, pos) = cageData[name];
                if (pos.IsSubsetOf(regionsPos[i])) { fullCagesByRegion[i].Cages.Add((sum,pos)); fullCagesByRegion[i].TotalSum += sum; } else partialCagesByRegion[i][name] = (sum, pos);
            }
            if (fullCagesByRegion[i].Cages.Count <= 1) continue;
            for (var size = 1; size < fullCagesByRegion[i].Cages.Count; size++) for (var start = 0; start <= fullCagesByRegion[i].Cages.Count - size; start++)
            { var usedCages = fullCagesByRegion[i].Cages.Skip(start).Take(size); var listPositions = new HashSet<int>(); var sumCages = 0; foreach (var (s, p) in usedCages) { listPositions.UnionWith(p); sumCages += s; } CreateCage(regionsPos[i].Except(listPositions), 45 - sumCages, true); }
        }
        foreach (var combi in REGIONS_COMBI)
        {
            var fullCagesSum = 0; var partialCagesGroup = new Dictionary<char, (int sum, HashSet<int> pos)>(); var groupPositions = new HashSet<int>(); var totalSum = 0;
            foreach(var r in combi) { fullCagesSum += fullCagesByRegion[r].TotalSum; foreach(var (k, v) in partialCagesByRegion[r]) partialCagesGroup[k] = v; groupPositions.UnionWith(regionsPos[r]); totalSum += 45; }
            var newlyFull = partialCagesGroup.Keys.Where(name => cageData[name].pos.IsSubsetOf(groupPositions)).ToList();
            foreach (var name in newlyFull) { fullCagesSum += partialCagesGroup[name].sum; partialCagesGroup.Remove(name); }
            if(!partialCagesGroup.Any()) continue;
            var partialPositions = new HashSet<int>(); var partialSum = 0; foreach (var (s, p) in partialCagesGroup.Values) { partialPositions.UnionWith(p); partialSum += s; }
            var outside = new HashSet<int>(partialPositions); outside.ExceptWith(groupPositions); CreateCage(outside, partialSum - totalSum + fullCagesSum, partialCagesGroup.Count == 1 || CheckUniqueDigits(outside));
            var inside = new HashSet<int>(partialPositions); inside.IntersectWith(groupPositions); CreateCage(inside, totalSum - fullCagesSum, partialCagesGroup.Count == 1 || combi.Length == 1 || CheckUniqueDigits(inside));
        }
    }

    private void CreateCage(IEnumerable<int> positions, int sum, bool uniqueDigits)
    {
        var posList = positions.ToList(); if (!posList.Any() || sum <= 0) return;
        posList.Sort(); var key = string.Join("-", posList); if (_knownCages.Contains(key)) return;
        _knownCages.Add(key); var cageIndex = _cages.Count;
        foreach (var pos in posList) _cagesMatch[pos].Add(cageIndex);
        _cages.Add(new Cage(sum, posList.Count, posList, uniqueDigits));
    }
    
    private static (int sumDigits, int forcedDigits) FindSumPermutations(int sum, int count, int mask)
    {
        if (sum <= 0 || count == 0 || mask == 0) return (0, 0);
        var key = (sum, count, mask); if (_sumPermutationsMemo.TryGetValue(key, out var cached)) return cached;
        if (count == 1) { if (sum > 9 || sum < 1) return (0,0); var b2 = DIGIT_TO_BINARY[sum]; return (mask & b2) != 0 ? (b2, b2) : (0, 0); }
        var d = _maskInfos[mask].Min; var b = DIGIT_TO_BINARY[d]; var updatedMask = mask & ~b;
        var (s1, f1) = FindSumPermutations(sum, count, updatedMask);
        var (s2, f2) = FindSumPermutations(sum - d, count - 1, updatedMask);
        var resSum = s1; var resForced = s1 != 0 ? f1 : FULL_DIGITS;
        if (s2 != 0) { resSum |= (s2 | b); resForced &= (f2 | b); } else if (s1 == 0) { resForced = 0; }
        return _sumPermutationsMemo[key] = (resSum, resForced);
    }
    
    private static bool CheckUniqueDigits(IEnumerable<int> positions)
    {
        var list = positions.ToList(); if (list.Count <= 1) return true;
        var first = list[0]; var r = first / 9; var c = first % 9; return list.All(p => p / 9 == r) || list.All(p => p % 9 == c);
    }

    private static void GenerateMaskInfos(int mask, int digit, Dictionary<int, int> digits)
    {
        if (digit > 9) { _maskInfos[mask] = new MaskInfo { Count = digits.Count, Min = digits.Any() ? digits.Keys.Min() : 0, Max = digits.Any() ? digits.Keys.Max() : 0, Digits = new Dictionary<int, int>(digits) }; return; }
        GenerateMaskInfos(mask, digit + 1, digits);
        digits[digit] = DIGIT_TO_BINARY[digit]; GenerateMaskInfos(mask | DIGIT_TO_BINARY[digit], digit + 1, digits); digits.Remove(digit);
    }
    
    private static void GenerateAffectedPositionsBase()
    {
        for (var y = 0; y < 9; y++) for (var x = 0; x < 9; x++)
        {
            var index = y * 9 + x; var seen = new bool[81]; seen[index] = true;
            for (var i = 0; i < 9; i++)
            {
                var rPos = i * 9 + x; var cPos = y * 9 + i;
                if (!seen[rPos]) { _affectedPositionsBase[index].Add(rPos); seen[rPos] = true; }
                if (!seen[cPos]) { _affectedPositionsBase[index].Add(cPos); seen[cPos] = true; }
            }
            var startY = y / 3 * 3; var startX = x / 3 * 3;
            for (var y2 = startY; y2 < startY + 3; y2++) for (var x2 = startX; x2 < startX + 3; x2++)
            { var regPos = y2 * 9 + x2; if (!seen[regPos]) { _affectedPositionsBase[index].Add(regPos); seen[regPos] = true; } }
        }
    }
}