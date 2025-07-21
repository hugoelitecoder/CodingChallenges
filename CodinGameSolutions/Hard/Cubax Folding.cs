using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var blocks = Console.ReadLine();
        
        var solver = new CubaxSolver(n, blocks);
        var result = solver.Solve();
        
        var iterPerSecond = result.ElapsedSeconds > 0 ? result.Iterations / result.ElapsedSeconds : 0;
        foreach (var s in result.Solutions)
        {
            Console.WriteLine(s);
        }
        Console.Error.WriteLine($"Duration: {result.ElapsedSeconds*1000:F2}ms, {result.Iterations} iterations ({iterPerSecond:F0}/s)");
    }
}

public readonly struct SolveResult
{
    public readonly List<string> Solutions;
    public readonly long Iterations;
    public readonly double ElapsedSeconds;

    public SolveResult(List<string> solutions, long iterations, double elapsedSeconds)
    {
        Solutions = solutions;
        Iterations = iterations;
        ElapsedSeconds = elapsedSeconds;
    }
}

public readonly struct Direction
{
    public readonly char Name;
    public readonly (int X, int Y, int Z) Vector;
    public readonly int Index;
    public readonly int OppositeIndex;

    public Direction(char name, (int, int, int) vector, int index, int oppositeIndex)
    {
        Name = name;
        Vector = vector;
        Index = index;
        OppositeIndex = oppositeIndex;
    }
}

public class GridInfo
{
    public readonly int N;
    public readonly int TotalElements;
    public readonly int EndKey;
    private readonly int[][] _nextKey;
    public readonly int[] InitialNeighbourCount;

    public GridInfo(int n, Direction[] directions)
    {
        N = n;
        var n2 = n * n;
        TotalElements = n * n * n;
        EndKey = TotalElements - 1;
        _nextKey = new int[TotalElements][];
        InitialNeighbourCount = new int[TotalElements];

        for (var z = 0; z < N; z++)
        for (var y = 0; y < N; y++)
        for (var x = 0; x < N; x++)
        {
            var key = x + y * N + z * n2;
            _nextKey[key] = new int[directions.Length];
            var neighbors = 0;
            foreach (var dir in directions)
            {
                var (nx, ny, nz) = (x + dir.Vector.X, y + dir.Vector.Y, z + dir.Vector.Z);
                if (nx >= 0 && nx < N && ny >= 0 && ny < N && nz >= 0 && nz < N)
                {
                    _nextKey[key][dir.Index] = nx + ny * N + nz * n2;
                    neighbors++;
                }
                else
                {
                    _nextKey[key][dir.Index] = -1;
                }
            }
            InitialNeighbourCount[key] = neighbors;
        }
    }
    
    public int GetNextKey(int currentKey, Direction direction) => _nextKey[currentKey][direction.Index];
}

public class SymmetryManager
{
    public readonly List<Dictionary<char, char>> RotationMaps;

    public SymmetryManager(Direction[] directions)
    {
        RotationMaps = new List<Dictionary<char, char>>();
        Precompute(directions);
    }

    public HashSet<string> Expand(IEnumerable<string> canonicalSolutions)
    {
        var finalSolutions = new HashSet<string>();
        var sb = new StringBuilder();
        foreach (var s in canonicalSolutions)
        {
            foreach (var map in RotationMaps)
            {
                sb.Clear();
                foreach (var c in s) sb.Append(map[c]);
                finalSolutions.Add(sb.ToString());
            }
        }
        return finalSolutions;
    }

    private void Precompute(Direction[] directions)
    {
        var dirVecToChar = directions.ToDictionary(d => d.Vector, d => d.Name);

        var validTransforms = new Func<(int x, int y, int z), (int x, int y, int z)>[]
        {
            p => (p.x, p.y, p.z), // Identity
            p => (p.y, p.x, p.z), // Reflect across x=y plane
            p => (p.z, p.y, p.x), // Reflect across x=z plane
            p => (p.x, p.z, p.y), // Reflect across y=z plane
            p => (p.y, p.z, p.x), // Rotate 120 degrees around (1,1,1)
            p => (p.z, p.x, p.y), // Rotate 240 degrees around (1,1,1)
        };
        
        foreach (var transform in validTransforms)
        {
            var map = new Dictionary<char, char>();
            foreach (var dir in directions)
            {
                var originalChar = dir.Name;
                var transformedVec = transform(dir.Vector);
                var newChar = dirVecToChar[transformedVec];
                map[originalChar] = newChar;
            }
            RotationMaps.Add(map);
        }
    }
}

public class CubaxSolver
{
    private static readonly Direction[] Directions = {
        new Direction('R', (1, 0, 0), 0, 1), new Direction('L', (-1, 0, 0), 1, 0),
        new Direction('U', (0, 1, 0), 2, 3), new Direction('D', (0, -1, 0), 3, 2),
        new Direction('F', (0, 0, 1), 4, 5), new Direction('B', (0, 0, -1), 5, 4)
    };
    
    private readonly GridInfo _grid;
    private readonly SymmetryManager _symmetryManager;
    private readonly int[] _blocks;
    private readonly bool[] _elts;
    private readonly int[] _neighbourCounts;
    private long _iterations;

    public CubaxSolver(int n, string blocksStr)
    {
        _grid = new GridInfo(n, Directions);
        _symmetryManager = new SymmetryManager(Directions);
        _blocks = blocksStr.Select(c => c - '0').ToArray();
        _elts = new bool[_grid.TotalElements];
        _neighbourCounts = new int[_grid.TotalElements];
        _iterations = 0;
    }

    public SolveResult Solve()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var canonicalSolutions = FindCanonicalSolutions();
        
        stopwatch.Stop();

        var finalSolutions = _symmetryManager.Expand(canonicalSolutions);
        var sortedSolutions = finalSolutions.ToList();
        sortedSolutions.Sort(StringComparer.Ordinal);
        
        return new SolveResult(sortedSolutions, _iterations, stopwatch.Elapsed.TotalSeconds);
    }

    private List<string> FindCanonicalSolutions()
    {
        var solutions = new List<string>();
        var path = new char[_blocks.Length];
        
        Array.Copy(_grid.InitialNeighbourCount, _neighbourCounts, _grid.TotalElements);
        Array.Clear(_elts, 0, _grid.TotalElements);
        var isolatedKey = -1;
        var changes = new List<(int, int)>();
        SetKey(0, ref isolatedKey, changes);

        var dirR = Directions[0];
        
        var (valid1, key1, iso1, changes1) = PlaceSegment(0, 0, dirR, isolatedKey, path);
        if (valid1)
        {
            Search(1, key1, dirR, iso1, path, solutions);
        }
        Backtrack(changes1);
        Backtrack(changes);

        return solutions;
    }

    private void Search(int blockIdx, int currentKey, Direction prevDir, int isolatedKey, char[] path, List<string> solutions)
    {
        _iterations++;
        if (blockIdx == _blocks.Length)
        {
            if (currentKey == _grid.EndKey) solutions.Add(new string(path));
            return;
        }

        foreach (var dir in Directions)
        {
            if (dir.Index == prevDir.Index || dir.Index == prevDir.OppositeIndex) continue;

            var (isValid, nextKey, newIsolatedKey, changes) = PlaceSegment(blockIdx, currentKey, dir, isolatedKey, path);
            if (isValid) Search(blockIdx + 1, nextKey, dir, newIsolatedKey, path, solutions);
            Backtrack(changes);
        }
    }
    
    private (bool, int, int, List<(int, int)>) PlaceSegment(int blockIdx, int currentKey, Direction dir, int isolatedKey, char[] path)
    {
        var segmentKeys = new List<int>();
        var tempKey = currentKey;
        var len = _blocks[blockIdx];
        for (var i = 1; i < len; i++)
        {
            tempKey = _grid.GetNextKey(tempKey, dir);
            if (tempKey == -1 || _elts[tempKey]) return (false, 0, 0, null);
            segmentKeys.Add(tempKey);
        }
    
        var changes = new List<(int, int)>();
        path[blockIdx] = dir.Name;
        var currentIsolatedKey = isolatedKey;

        foreach (var key in segmentKeys)
        {
            if (!SetKey(key, ref currentIsolatedKey, changes))
            {
                Backtrack(changes);
                return (false, 0, 0, null);
            }
        }

        return (true, segmentKeys.Count > 0 ? segmentKeys.Last() : currentKey, currentIsolatedKey, changes);
    }
    
    private bool SetKey(int key, ref int isolatedKey, List<(int, int)> changes)
    {
        if (isolatedKey != -1 && key != isolatedKey) return false;

        _elts[key] = true;
        changes.Add((key, -1));
        
        if (isolatedKey != -1 && key == isolatedKey) isolatedKey = -1;

        foreach (var neighborDir in Directions)
        {
            var neighborKey = _grid.GetNextKey(key, neighborDir);
            if (neighborKey != -1 && !_elts[neighborKey])
            {
                changes.Add((neighborKey, _neighbourCounts[neighborKey]));
                _neighbourCounts[neighborKey]--;

                if (_neighbourCounts[neighborKey] == 1 && neighborKey != _grid.EndKey)
                {
                    if (isolatedKey != -1) return false;
                    isolatedKey = neighborKey;
                }
            }
        }
        return true;
    }
    
    private void Backtrack(List<(int key, int oldValue)> changes)
    {
        if (changes == null) return;
        for (var i = changes.Count - 1; i >= 0; i--)
        {
            var (key, val) = changes[i];
            if (val == -1) _elts[key] = false;
            else _neighbourCounts[key] = val;
        }
    }
}