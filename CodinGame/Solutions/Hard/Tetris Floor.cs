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
        var dp = new Dictionary<(long, int), (long price, long ways)>
        {
            { (0L, 0), (0, 1) }
        };

        for (var r = 0; r < h; r++)
        {
            for (var c = 0; c < w; c++)
            {
                if (floor[r][c] != '.' || visited[r, c]) continue;

                var componentCells = FindComponent(floor, w, h, r, c, visited);
                if (componentCells.Count == 0) continue;

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

                if (dp.Count != 0 && blockSolutions.Count != 0)
                {
                    dp = CombineSolutions(dp, blockSolutions);
                }
                else if (blockSolutions.Count == 0 && componentCells.Count != 0 && dp.Count != 0)
                {
                    dp.Clear();
                }
            }

            if (dp.Count == 0) break;
        }

        Console.Error.WriteLine("[DEBUG] All components processed. Aggregating final results.");
        return AggregateFinalResults(dp, forcedPrice, forcedUsage);
    }

    private Dictionary<(long, int), (long price, long ways)> SolveAndMemoizeBlock(char[][] block, int blockW, int blockH, long[] prices, long minPrice)
    {
        var (blkPieces, blkTypes, blkPosToPieces, blkPosCounts) = GetPieces(block, blockW, blockH);

        foreach (var key in blkPosToPieces.Keys)
        {
            blkPosToPieces[key] = blkPosToPieces[key].OrderBy(pid => prices[blkTypes[pid]]).ToList();
        }

        var allPlacements = blkPieces.Select((p, i) => (cells: p, type: blkTypes[i])).ToArray();
        var solver = new BlockSolver(allPlacements, prices, minPrice);
        var bestPrice = long.MaxValue;
        var blockKey = block.SelectMany(row => row).ToArray();
        var rawSolutions = solver.Solve(blkPosToPieces, blkPosCounts, new ShapeMask(blockKey), new int[7], 0, ref bestPrice);
        var bestSolutions = new Dictionary<(long, int), (long price, long ways)>();

        if (rawSolutions.Count != 0)
        {
            var minP = rawSolutions.Min(s => s.Value.price);

            foreach (var solution in rawSolutions)
            {
                if (solution.Value.price == minP)
                {
                    bestSolutions[solution.Key] = solution.Value;
                }
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

    private static Dictionary<(long, int), (long price, long ways)> CombineSolutions(
        Dictionary<(long, int), (long price, long ways)> dp,
        Dictionary<(long, int), (long price, long ways)> blockSolutions)
    {
        var nextDp = new Dictionary<(long, int), (long price, long ways)>();

        foreach (var (prevUsageKey, prevVal) in dp)
        {
            foreach (var (blockUsageKey, blockVal) in blockSolutions)
            {
                var prevUsage = PieceCountEncoder.Unpack(prevUsageKey);
                var blockUsage = PieceCountEncoder.Unpack(blockUsageKey);
                var totalUsage = new int[7];

                for (var i = 0; i < 7; i++)
                {
                    totalUsage[i] = prevUsage[i] + blockUsage[i];
                }

                var combinedKey = PieceCountEncoder.Pack(totalUsage);
                var combinedPrice = prevVal.price + blockVal.price;
                var combinedWays = prevVal.ways * blockVal.ways;

                if (!nextDp.TryGetValue(combinedKey, out var existing))
                {
                    nextDp[combinedKey] = (combinedPrice, combinedWays);
                }
                else if (combinedPrice < existing.price)
                {
                    nextDp[combinedKey] = (combinedPrice, combinedWays);
                }
                else if (combinedPrice == existing.price)
                {
                    nextDp[combinedKey] = (combinedPrice, existing.ways + combinedWays);
                }
            }
        }

        return nextDp;
    }

    private static Dictionary<(long, int), (long price, long ways)> AggregateFinalResults(
        Dictionary<(long, int), (long price, long ways)> dp,
        long forcedPrice,
        int[] forcedUsage)
    {
        var final = new Dictionary<(long, int), (long price, long ways)>();

        foreach (var kvp in dp)
        {
            var usage = PieceCountEncoder.Unpack(kvp.Key);

            for (var i = 0; i < 7; i++)
            {
                usage[i] += forcedUsage[i];
            }

            var finalKey = PieceCountEncoder.Pack(usage);
            var price = kvp.Value.price + forcedPrice;

            if (!final.TryGetValue(finalKey, out var existing) || price < existing.price)
            {
                final[finalKey] = (price, kvp.Value.ways);
            }
            else if (price == existing.price)
            {
                final[finalKey] = (price, existing.ways + kvp.Value.ways);
            }
        }

        return final;
    }

    private static (List<int[]> pieces, List<int> types, Dictionary<int, List<int>> posToPieces, Dictionary<int, int> posCounts) GetPieces(char[][] floor, int w, int h)
    {
        var pieces = new List<int[]>();
        var types = new List<int>();
        var posToPieces = new Dictionary<int, List<int>>();
        var posCounts = new Dictionary<int, int>();
        var pieceId = 0;

        for (var r = 0; r < h; r++)
        {
            for (var c = 0; c < w; c++)
            {
                if (floor[r][c] == '#') continue;

                for (var t = 0; t < Tetrominoes.Length; t++)
                {
                    var moves = Tetrominoes[t];
                    if (moves == null) continue;

                    var positions = new int[moves.Length];
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

                        positions[m] = nr * w + nc;
                    }

                    if (!canPlace) continue;

                    pieces.Add(positions);
                    types.Add(t / 4);

                    foreach (var cell in positions)
                    {
                        if (!posToPieces.TryGetValue(cell, out var list))
                        {
                            posToPieces[cell] = list = new List<int>();
                        }

                        list.Add(pieceId);
                        posCounts[cell] = posCounts.GetValueOrDefault(cell, 0) + 1;
                    }

                    pieceId++;
                }
            }
        }

        return (pieces, types, posToPieces, posCounts);
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
                var type = pieceTypes[pieceId];

                forcedUsage[type]++;
                forcedPrice += prices[type];

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
        var cells = new List<(int x, int y)>();

        stack.Push((c, r));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            if (x < 0 || y < 0 || x >= w || y >= h || floor[y][x] == '#' || visited[y, x])
            {
                continue;
            }

            visited[y, x] = true;
            cells.Add((x, y));

            stack.Push((x + 1, y));
            stack.Push((x - 1, y));
            stack.Push((x, y + 1));
            stack.Push((x, y - 1));
        }

        return cells;
    }

    private static (char[][] block, int w, int h) CreateBlockFromComponent(List<(int x, int y)> cells)
    {
        var minRow = int.MaxValue;
        var maxRow = int.MinValue;
        var minCol = int.MaxValue;
        var maxCol = int.MinValue;

        foreach (var (x, y) in cells)
        {
            if (y < minRow) minRow = y;
            if (y > maxRow) maxRow = y;
            if (x < minCol) minCol = x;
            if (x > maxCol) maxCol = x;
        }

        var blockH = maxRow - minRow + 3;
        var blockW = maxCol - minCol + 3;
        var block = new char[blockH][];

        for (var r = 0; r < blockH; r++)
        {
            block[r] = new char[blockW];
            Array.Fill(block[r], '#');
        }

        foreach (var (x, y) in cells)
        {
            block[y - minRow + 1][x - minCol + 1] = '.';
        }

        return (block, blockW, blockH);
    }

    private static void RotateLeft(char[][] floor, out char[][] rotated, ref int w, ref int h)
    {
        rotated = new char[w][];

        for (var x = 0; x < w; x++)
        {
            rotated[x] = new char[h];

            for (var y = 0; y < h; y++)
            {
                rotated[x][y] = floor[h - 1 - y][x];
            }
        }

        (w, h) = (h, w);
    }
}

internal class BlockSolver
{
    private readonly (int[] cells, int type)[] _allPieces;
    private readonly long[] _piecePrices;
    private readonly long _minPiecePrice;
    private readonly ShapeMask[] _pieceMasks;
    private readonly Dictionary<(ShapeMask, (long, int)), Dictionary<(long, int), (long price, long ways)>> _memo;

    public BlockSolver((int[] cells, int type)[] placements, long[] prices, long minPrice)
    {
        _allPieces = placements;
        _piecePrices = prices;
        _minPiecePrice = minPrice;
        _pieceMasks = new ShapeMask[placements.Length];

        for (var i = 0; i < placements.Length; i++)
        {
            _pieceMasks[i] = new ShapeMask(placements[i].cells);
        }

        _memo = new Dictionary<(ShapeMask, (long, int)), Dictionary<(long, int), (long price, long ways)>>();
    }

    public Dictionary<(long, int), (long price, long ways)> Solve(
        Dictionary<int, List<int>> openPositions,
        Dictionary<int, int> cellCoverageCounts,
        ShapeMask boardKey,
        int[] usage,
        long currentPrice,
        ref long bestBlockPrice)
    {
        var usageKey = PieceCountEncoder.Pack(usage);
        var memoKey = (boardKey, usageKey);

        if (_memo.TryGetValue(memoKey, out var memoizedResult))
        {
            return memoizedResult;
        }

        if (openPositions.Count == 0)
        {
            if (currentPrice < bestBlockPrice)
            {
                bestBlockPrice = currentPrice;
            }

            return new Dictionary<(long, int), (long price, long ways)>
            {
                { usageKey, (currentPrice, 1) }
            };
        }

        if (currentPrice + _minPiecePrice * (openPositions.Count / 4) > bestBlockPrice)
        {
            return new Dictionary<(long, int), (long price, long ways)>();
        }

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

        if (minCoverage == 0 || (cellToFill == -1 && openPositions.Count != 0))
        {
            return new Dictionary<(long, int), (long price, long ways)>();
        }

        var results = new Dictionary<(long, int), (long price, long ways)>();
        var placementsToTry = openPositions[cellToFill];

        foreach (var placementId in placementsToTry)
        {
            var (cells, type) = _allPieces[placementId];
            usage[type]++;

            var nextPositions = new Dictionary<int, List<int>>(openPositions.Count);

            foreach (var kvp in openPositions)
            {
                nextPositions[kvp.Key] = new List<int>(kvp.Value);
            }

            var nextCounts = new Dictionary<int, int>(cellCoverageCounts);

            foreach (var cell in cells)
            {
                if (!nextPositions.TryGetValue(cell, out var affectedPieces))
                {
                    continue;
                }

                foreach (var affectedPieceId in affectedPieces.ToArray())
                {
                    foreach (var affectedCell in _allPieces[affectedPieceId].cells)
                    {
                        if (nextCounts.ContainsKey(affectedCell))
                        {
                            nextCounts[affectedCell]--;
                        }

                        if (nextPositions.TryGetValue(affectedCell, out var pieceList))
                        {
                            pieceList.Remove(affectedPieceId);
                        }
                    }
                }

                nextPositions.Remove(cell);
                nextCounts.Remove(cell);
            }

            var recursiveSolutions = Solve(
                nextPositions,
                nextCounts,
                boardKey.Remove(_pieceMasks[placementId]),
                usage,
                currentPrice + _piecePrices[type],
                ref bestBlockPrice);

            foreach (var solution in recursiveSolutions)
            {
                if (!results.TryGetValue(solution.Key, out var existing))
                {
                    results[solution.Key] = solution.Value;
                }
                else if (solution.Value.price < existing.price)
                {
                    results[solution.Key] = solution.Value;
                }
                else if (solution.Value.price == existing.price)
                {
                    results[solution.Key] = (solution.Value.price, existing.ways + solution.Value.ways);
                }
            }

            usage[type]--;
        }

        var optimalResults = new Dictionary<(long, int), (long price, long ways)>();

        if (results.Count != 0)
        {
            var minPriceForBranch = results.Min(pair => pair.Value.price);

            foreach (var result in results)
            {
                if (result.Value.price == minPriceForBranch && result.Value.price <= bestBlockPrice)
                {
                    optimalResults[result.Key] = result.Value;
                }
            }
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
        {
            key1 = (key1 << 10) | (uint)usage[i];
        }

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
        p1 = 0;
        p2 = 0;
        p3 = 0;
        p4 = 0;

        for (var bit = 0; bit < block.Length; bit++)
        {
            if (block[bit] != '.') continue;

            if (bit < 64)
            {
                p1 |= 1UL << bit;
            }
            else if (bit < 128)
            {
                p2 |= 1UL << (bit - 64);
            }
            else if (bit < 192)
            {
                p3 |= 1UL << (bit - 128);
            }
            else
            {
                p4 |= 1UL << (bit - 192);
            }
        }
    }

    public ShapeMask(int[] cells)
    {
        p1 = 0;
        p2 = 0;
        p3 = 0;
        p4 = 0;

        for (var i = 0; i < cells.Length; i++)
        {
            var bit = cells[i];

            if (bit < 64)
            {
                p1 |= 1UL << bit;
            }
            else if (bit < 128)
            {
                p2 |= 1UL << (bit - 64);
            }
            else if (bit < 192)
            {
                p3 |= 1UL << (bit - 128);
            }
            else
            {
                p4 |= 1UL << (bit - 192);
            }
        }
    }

    private ShapeMask(ulong p1, ulong p2, ulong p3, ulong p4)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.p4 = p4;
    }

    internal ShapeMask Remove(ShapeMask other)
    {
        return new ShapeMask(
            p1 & ~other.p1,
            p2 & ~other.p2,
            p3 & ~other.p3,
            p4 & ~other.p4);
    }

    public bool Equals(ShapeMask other)
    {
        return p1 == other.p1 && p2 == other.p2 && p3 == other.p3 && p4 == other.p4;
    }

    public override bool Equals(object obj)
    {
        return obj is ShapeMask other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(p1, p2, p3, p4);
    }
}

internal class PieceCountComparer : IComparer<int[]>
{
    public static readonly PieceCountComparer Instance = new();

    public int Compare(int[] x, int[] y)
    {
        for (var i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
            {
                return x[i].CompareTo(y[i]);
            }
        }

        return 0;
    }
}