using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

public class Solution
{
    public static void Main()
    {
        var stopwatch = Stopwatch.StartNew();

        var dims = Console.ReadLine().Split();
        var w = int.Parse(dims[0]);
        var h = int.Parse(dims[1]);
        Console.Error.WriteLine($"[DEBUG] Dimensions: {w}x{h}");

        var priceParts = Console.ReadLine().Split();
        var prices = new long[priceParts.Length];
        for (var i = 0; i < prices.Length; i++)
        {
            prices[i] = (long)Math.Round(double.Parse(priceParts[i], CultureInfo.InvariantCulture) * 100);
        }
        Console.Error.WriteLine($"[DEBUG] Prices (in cents): {string.Join(", ", prices)}");

        var floor = new char[h][];
        Console.Error.WriteLine("[DEBUG] Input Floor:");
        for (var i = 0; i < h; i++)
        {
            floor[i] = Console.ReadLine().ToCharArray();
            Console.Error.WriteLine($"[DEBUG] {new string(floor[i])}");
        }

        var solver = new TilingSolver();
        var finalSolutions = solver.Solve(w, h, prices, floor);

        if (finalSolutions.Count == 0)
        {
            Console.Error.WriteLine("[DEBUG] No solution found.");
            Console.WriteLine("0.00", CultureInfo.InvariantCulture);
            Console.WriteLine("0 0 0 0 0 0 0");
            Console.WriteLine(0);
        }
        else
        {
            var overallMinPrice = finalSolutions.Min(f => f.Value.price);
            var minPriceUsages = finalSolutions
                .Where(f => f.Value.price == overallMinPrice)
                .Select(f => (Usage: PieceCountEncoder.Unpack(f.Key), Ways: f.Value.ways))
                .ToList();
            var lexMinUsage = minPriceUsages.OrderBy(s => s.Usage, PieceCountComparer.Instance).First().Usage;
            var totalWays = minPriceUsages.Where(s => PieceCountComparer.Instance.Compare(s.Usage, lexMinUsage) == 0).Sum(s => s.Ways);

            Console.Error.WriteLine($"[DEBUG] Final solutions considered: {finalSolutions.Count}");
            Console.Error.WriteLine($"[DEBUG] Minimum Price (in cents): {overallMinPrice}");
            Console.Error.WriteLine($"[DEBUG] Optimal Piece Counts: {string.Join(" ", lexMinUsage)}");
            Console.Error.WriteLine($"[DEBUG] Number of Ways for Optimal Config: {totalWays}");

            Console.WriteLine($"{(overallMinPrice / 100.0):F2}", CultureInfo.InvariantCulture);
            Console.WriteLine(string.Join(" ", lexMinUsage));
            Console.WriteLine(totalWays);
        }

        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Total execution time: {stopwatch.ElapsedMilliseconds} ms");
    }
}

internal class TilingSolver
{
    private readonly Dictionary<ShapeMask, Dictionary<(long, int), (long price, long ways)>> _canonicalBlockMemo = new();
    private static readonly (int dx, int dy)[][] Tetrominoes =
    {
        new[] { (0, 0), (1, 0), (2, 0), (3, 0) }, new[] { (0, 0), (0, 1), (0, 2), (0, 3) }, null, null,
        new[] { (0, 0), (1, 0), (0, 1), (1, 1) }, null, null, null,
        new[] { (0, 0), (1, 0), (2, 0), (1, 1) }, new[] { (0, 0), (0, 1), (0, 2), (-1, 1) },
        new[] { (0, 0), (1, 0), (2, 0), (1, -1) }, new[] { (0, 0), (0, 1), (0, 2), (1, 1) },
        new[] { (0, 0), (1, 0), (2, 0), (0, 1) }, new[] { (0, 0), (1, 0), (1, 1), (1, 2) },
        new[] { (0, 0), (1, 0), (2, 0), (2, -1) }, new[] { (0, 0), (0, 1), (0, 2), (1, 2) },
        new[] { (0, 0), (1, 0), (2, 0), (2, 1) }, new[] { (0, 0), (1, 0), (1, -1), (1, -2) },
        new[] { (0, 0), (0, 1), (1, 1), (2, 1) }, new[] { (0, 0), (1, 0), (0, 1), (0, 2) },
        new[] { (0, 0), (1, 0), (1, 1), (2, 1) }, new[] { (0, 0), (0, 1), (-1, 1), (-1, 2) },
        null, null,
        new[] { (0, 0), (1, 0), (1, -1), (2, -1) }, new[] { (0, 0), (0, 1), (1, 1), (1, 2) }
    };

    public Dictionary<(long, int), (long price, long ways)> Solve(int w, int h, long[] prices, char[][] floor)
    {
        var minPrice = prices.Length > 0 ? prices.Min() : 0;
        var (forcedPrice, forcedUsage) = ProcessForcedMoves(floor, w, h, prices);
        var visited = new bool[h, w];
        var dp = new Dictionary<(long, int), (long price, long ways)> { { (0L, 0), (0, 1) } };
        var componentIndex = 0;
        for (var r = 0; r < h; r++)
        {
            for (var c = 0; c < w; c++)
            {
                if (floor[r][c] != '.' || visited[r, c]) continue;
                var componentCells = FindComponent(floor, w, h, r, c, visited);
                if (componentCells.Count == 0) continue;
                componentIndex++;
                if (componentCells.Count % 4 != 0)
                {
                    dp.Clear();
                    break;
                }
                var (block, blockW, blockH) = CreateBlockFromComponent(componentCells);
                var blockMask = new ShapeMask(block.SelectMany(row => row).ToArray());
                if (!_canonicalBlockMemo.TryGetValue(blockMask, out var blockSolutions))
                {
                    blockSolutions = SolveAndMemoizeBlock(block, blockW, blockH, prices, minPrice);
                }
               
                if (dp.Any() && blockSolutions.Any())
                {
                    dp = CombineSolutions(dp, blockSolutions);
                }
                else if (blockSolutions.Count == 0 && componentCells.Any() && dp.Any())
                {
                    dp.Clear();
                }
            }
            if (!dp.Any()) break;
        }
        Console.Error.WriteLine("[DEBUG] All components processed. Aggregating final results.");
        return AggregateFinalResults(dp, forcedPrice, forcedUsage);
    }

    private Dictionary<(long, int), (long price, long ways)> SolveAndMemoizeBlock(char[][] block, int blockW, int blockH, long[] prices, long minPrice)
    {
        var (blkPieces, blkTypes, blkPosToPieces, blkPosCounts) = GetPieces(block, blockW, blockH);
        foreach (var k in blkPosToPieces.Keys)
        {
            blkPosToPieces[k] = blkPosToPieces[k].OrderBy(pid => prices[blkTypes[pid]]).ToList();
        }
        var allPlacements = blkPieces.Select((p, i) => (cells: p, type: blkTypes[i])).ToArray();
        var solver = new BlockSolver(allPlacements, prices, minPrice);
        var bestPrice = long.MaxValue;
        var blockKey = block.SelectMany(row => row).ToArray();
        var rawSolutions = solver.Solve(blkPosToPieces, blkPosCounts, blockKey, new int[7], 0, ref bestPrice);
        var bestSolutions = new Dictionary<(long, int), (long price, long ways)>();
        if (rawSolutions.Any())
        {
            var minP = rawSolutions.Min(s => s.Value.price);
            foreach (var sol in rawSolutions)
            {
                if (sol.Value.price == minP)
                    bestSolutions[sol.Key] = sol.Value;
            }
        }
        var rotated = block;
        for (var rot = 0; rot < 4; rot++)
        {
            var rotKey = rotated.SelectMany(row => row).ToArray();
            var rotMask = new ShapeMask(rotKey);
            if (!_canonicalBlockMemo.ContainsKey(rotMask))
            {
                _canonicalBlockMemo[rotMask] = bestSolutions;
            }
            if (rot < 3)
            {
                var hCurr = rotated.Length;
                var wCurr = rotated[0].Length;
                RotateLeft(rotated, out rotated, ref wCurr, ref hCurr);
            }
        }
        return _canonicalBlockMemo[new ShapeMask(blockKey)];
    }

    private static Dictionary<(long, int), (long price, long ways)> CombineSolutions(Dictionary<(long, int), (long price, long ways)> dp, Dictionary<(long, int), (long price, long ways)> blockSolutions)
    {
        var nextDp = new Dictionary<(long, int), (long price, long ways)>();
        foreach (var (prevUsageKey, prevVal) in dp)
        {
            foreach (var (blockUsageKey, blockVal) in blockSolutions)
            {
                var prevU = PieceCountEncoder.Unpack(prevUsageKey);
                var blkU = PieceCountEncoder.Unpack(blockUsageKey);
                var totalUsage = new int[7];
                for (var i = 0; i < 7; i++) totalUsage[i] = prevU[i] + blkU[i];
                var combinedKey = PieceCountEncoder.Pack(totalUsage);
                var combinedPrice = prevVal.price + blockVal.price;
                var combinedWays = prevVal.ways * blockVal.ways;
                if (!nextDp.TryGetValue(combinedKey, out var existing))
                    nextDp[combinedKey] = (combinedPrice, combinedWays);
                else if (combinedPrice < existing.price)
                    nextDp[combinedKey] = (combinedPrice, combinedWays);
                else if (combinedPrice == existing.price)
                    nextDp[combinedKey] = (combinedPrice, existing.ways + combinedWays);
            }
        }
        return nextDp;
    }

    private static Dictionary<(long, int), (long price, long ways)> AggregateFinalResults(Dictionary<(long, int), (long price, long ways)> dp, long forcedPrice, int[] forcedUsage)
    {
        var final = new Dictionary<(long, int), (long price, long ways)>();
        foreach (var kvp in dp)
        {
            var usage = PieceCountEncoder.Unpack(kvp.Key);
            for (var i = 0; i < 7; i++) usage[i] += forcedUsage[i];
            var finalKey = PieceCountEncoder.Pack(usage);
            var priceF = kvp.Value.price + forcedPrice;
            if (!final.TryGetValue(finalKey, out var ex) || priceF < ex.price)
                final[finalKey] = (priceF, kvp.Value.ways);
            else if (priceF == ex.price)
                final[finalKey] = (priceF, ex.ways + kvp.Value.ways);
        }
        return final;
    }

    private static (List<int[]> pieces, List<int> types, Dictionary<int, List<int>> posToPieces, Dictionary<int, int> posCounts) GetPieces(char[][] floor, int w, int h)
    {
        var pList = new List<int[]>();
        var pTypes = new List<int>();
        var pToPieces = new Dictionary<int, List<int>>();
        var pCounts = new Dictionary<int, int>();
        var pId = 0;
        for (var r = 0; r < h; r++)
        {
            for (var c = 0; c < w; c++)
            {
                if (floor[r][c] == '#') continue;
                for (var t = 0; t < Tetrominoes.Length; t++)
                {
                    var moves = Tetrominoes[t];
                    if (moves == null) continue;
                    var posIdx = new int[moves.Length];
                    var canPlace = true;
                    for (var m = 0; m < moves.Length; m++)
                    {
                        var (dx, dy) = moves[m];
                        var nr = r + dy;
                        var nc = c + dx;
                        if (nr < 0 || nr >= h || nc < 0 || nc >= w || floor[nr][nc] != '.')
                        {
                            canPlace = false;
                            break;
                        }
                        posIdx[m] = nr * w + nc;
                    }
                    if (!canPlace) continue;
                    pList.Add(posIdx);
                    pTypes.Add(t / 4);
                    foreach (var cell in posIdx)
                    {
                        if (!pToPieces.TryGetValue(cell, out var list)) pToPieces[cell] = list = new List<int>();
                        list.Add(pId);
                        pCounts[cell] = pCounts.GetValueOrDefault(cell, 0) + 1;
                    }
                    pId++;
                }
            }
        }
        return (pList, pTypes, pToPieces, pCounts);
    }

    private static (long forcedPrice, int[] forcedUsage) ProcessForcedMoves(char[][] floor, int w, int h, long[] prices)
    {
        var forcedPrice = 0L;
        var forcedUsage = new int[7];
        bool added;
        do
        {
            added = false;
            var (pieces, pieceTypes, posToPieces, posCounts) = GetPieces(floor, w, h);
            var cellToForce = -1;
            foreach (var kvp in posCounts)
            {
                if (kvp.Value == 1)
                {
                    cellToForce = kvp.Key;
                    break;
                }
            }
            if (cellToForce != -1)
            {
                var pieceId = posToPieces[cellToForce][0];
                var pt = pieceTypes[pieceId];
                forcedUsage[pt]++;
                forcedPrice += prices[pt];
                foreach (var cell in pieces[pieceId])
                {
                    floor[cell / w][cell % w] = '#';
                }
                added = true;
            }
        } while (added);
        return (forcedPrice, forcedUsage);
    }

    private static List<(int x, int y)> FindComponent(char[][] floor, int w, int h, int r, int c, bool[,] visited)
    {
        var stack = new Stack<(int x, int y)>();
        stack.Push((c, r));
        var cells = new List<(int x, int y)>();
        while (stack.Count > 0)
        {
            var (cx, cy) = stack.Pop();
            if (cx < 0 || cy < 0 || cx >= w || cy >= h || floor[cy][cx] == '#' || visited[cy, cx]) continue;
            visited[cy, cx] = true;
            cells.Add((cx, cy));
            stack.Push((cx + 1, cy));
            stack.Push((cx - 1, cy));
            stack.Push((cx, cy + 1));
            stack.Push((cx, cy - 1));
        }
        return cells;
    }

    private static (char[][] block, int w, int h) CreateBlockFromComponent(List<(int x, int y)> cells)
    {
        var minR = int.MaxValue;
        var maxR = int.MinValue;
        var minC = int.MaxValue;
        var maxC = int.MinValue;
        foreach (var (x, y) in cells)
        {
            if (y < minR) minR = y;
            if (y > maxR) maxR = y;
            if (x < minC) minC = x;
            if (x > maxC) maxC = x;
        }
        var blockH = (maxR - minR + 1) + 2;
        var blockW = (maxC - minC + 1) + 2;
        var block = new char[blockH][];
        for (var i = 0; i < blockH; i++)
        {
            block[i] = new char[blockW];
            Array.Fill(block[i], '#');
        }
        foreach (var (cx, cy) in cells)
            block[cy - minR + 1][cx - minC + 1] = '.';
        return (block, blockW, blockH);
    }

    private static void RotateLeft(char[][] f, out char[][] rotated, ref int w, ref int h)
    {
        rotated = new char[w][];
        for (var x = 0; x < w; x++)
        {
            rotated[x] = new char[h];
            for (var y = 0; y < h; y++)
                rotated[x][y] = f[h - 1 - y][x];
        }
        (w, h) = (h, w);
    }
}

internal class BlockSolver
{
    private readonly (int[] cells, int type)[] _allPieces;
    private readonly long[] _piecePrices;
    private readonly long _minPiecePrice;
    private readonly Dictionary<(ShapeMask, (long, int)), Dictionary<(long, int), (long price, long ways)>> _memo;

    public BlockSolver((int[], int)[] placements, long[] prices, long minPrice)
    {
        _allPieces = placements;
        _piecePrices = prices;
        _minPiecePrice = minPrice;
        _memo = new Dictionary<(ShapeMask, (long, int)), Dictionary<(long, int), (long, long)>>();
    }

    public Dictionary<(long, int), (long price, long ways)> Solve(
        Dictionary<int, List<int>> openPositions,
        Dictionary<int, int> cellCoverageCounts,
        char[] floor,
        int[] usage,
        long currentPrice,
        ref long bestBlockPrice)
    {
        var usageKey = PieceCountEncoder.Pack(usage);
        var boardKey = new ShapeMask(floor);
        var memoKey = (boardKey, usageKey);
        if (_memo.TryGetValue(memoKey, out var memoizedResult))
        {
            return memoizedResult;
        }
        if (openPositions.Count == 0)
        {
            if (currentPrice < bestBlockPrice) bestBlockPrice = currentPrice;
            return new Dictionary<(long, int), (long price, long ways)> { { usageKey, (price: currentPrice, ways: 1) } };
        }
        if (currentPrice + _minPiecePrice * (openPositions.Count / 4) > bestBlockPrice)
            return new Dictionary<(long, int), (long, long)>();
        var cellToFill = -1;
        var minCoverage = int.MaxValue;
        foreach (var kvp in cellCoverageCounts)
        {
            if (kvp.Value < minCoverage)
            {
                minCoverage = kvp.Value;
                cellToFill = kvp.Key;
            }
        }
        if (minCoverage == 0 || (cellToFill == -1 && openPositions.Any()))
            return new Dictionary<(long, int), (long, long)>();
        var results = new Dictionary<(long, int), (long price, long ways)>();
        var placementsToTry = openPositions[cellToFill];
        foreach (var placementIdToTry in placementsToTry)
        {
            var (cells, type) = _allPieces[placementIdToTry];
            usage[type]++;
            var nextPositions = new Dictionary<int, List<int>>(openPositions.Count);
            foreach (var kvp in openPositions) nextPositions[kvp.Key] = new List<int>(kvp.Value);
            var nextCounts = new Dictionary<int, int>(cellCoverageCounts);
            var nextFloor = (char[])floor.Clone();
            foreach (var cell in cells)
            {
                nextFloor[cell] = '#';
                if (!nextPositions.TryGetValue(cell, out var affectedList)) continue;
                foreach (var affectedPieceId in affectedList.ToArray())
                {
                    foreach (var affectedCell in _allPieces[affectedPieceId].cells)
                    {
                        if (nextCounts.ContainsKey(affectedCell)) nextCounts[affectedCell]--;
                        if (nextPositions.TryGetValue(affectedCell, out var pieceList)) pieceList.Remove(affectedPieceId);
                    }
                }
                nextPositions.Remove(cell);
                nextCounts.Remove(cell);
            }
            var recursiveSolutions = Solve(nextPositions, nextCounts, nextFloor, usage, currentPrice + _piecePrices[type], ref bestBlockPrice);
            foreach (var solPair in recursiveSolutions)
            {
                if (!results.TryGetValue(solPair.Key, out var existing))
                    results[solPair.Key] = solPair.Value;
                else if (solPair.Value.price < existing.price)
                    results[solPair.Key] = solPair.Value;
                else if (solPair.Value.price == existing.price)
                    results[solPair.Key] = (solPair.Value.price, existing.ways + solPair.Value.ways);
            }
            usage[type]--;
        }
        var optimalResults = new Dictionary<(long, int), (long price, long ways)>();
        if (results.Any())
        {
            var minPriceForBranch = results.Min(r => r.Value.price);
            foreach (var kvp in results)
                if (kvp.Value.price == minPriceForBranch && kvp.Value.price <= bestBlockPrice)
                    optimalResults[kvp.Key] = kvp.Value;
        }
        _memo[memoKey] = optimalResults;
        return results;
    }
}

internal static class PieceCountEncoder
{
    internal static (long, int) Pack(int[] usage)
    {
        var key1 = 0L;
        for (var i = 0; i < 6; i++)
            key1 = (key1 << 10) | (uint)usage[i];
        return (key1, usage[6]);
    }
    internal static int[] Unpack((long, int) key)
    {
        var (key1, key2) = key;
        var usage = new int[7];
        usage[6] = key2;
        for (var i = 5; i >= 0; i--)
        {
            usage[i] = (int)(key1 & 1023);
            key1 >>= 10;
        }
        return usage;
    }
}

internal readonly struct ShapeMask : IEquatable<ShapeMask>
{
    internal readonly ulong p1;
    internal readonly ulong p2;
    internal readonly ulong p3;
    internal readonly ulong p4;
    public ShapeMask(char[] block)
    {
        p1 = p2 = p3 = p4 = 0;
        for (var bit = 0; bit < block.Length; bit++)
        {
            if (block[bit] == '.')
            {
                if (bit < 64) p1 |= 1UL << bit;
                else if (bit < 128) p2 |= 1UL << (bit - 64);
                else if (bit < 192) p3 |= 1UL << (bit - 128);
                else p4 |= 1UL << (bit - 192);
            }
        }
    }
    public bool Equals(ShapeMask other) => p1 == other.p1 && p2 == other.p2 && p3 == other.p3 && p4 == other.p4;
    public override bool Equals(object obj) => obj is ShapeMask other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(p1, p2, p3, p4);
}

internal class PieceCountComparer : IComparer<int[]>
{
    public static readonly PieceCountComparer Instance = new();
    public int Compare(int[] x, int[] y)
    {
        for (var i = 0; i < x.Length; i++)
            if (x[i] != y[i]) return x[i].CompareTo(y[i]);
        return 0;
    }
}