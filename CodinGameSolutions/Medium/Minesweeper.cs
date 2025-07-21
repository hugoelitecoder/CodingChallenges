using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Player
{
    public static void Main()
    {
        var agent = new MinesweeperAgent();
        while (true)
        {
            var boardState = new char[MinesweeperAgent.Size];
            var line = Console.ReadLine();
            if (line == null) break;

            for (var r = 0; r < MinesweeperAgent.Height; r++)
            {
                var toks = line.Split(' ');
                for (var c = 0; c < MinesweeperAgent.Width; c++)
                {
                    boardState[r * MinesweeperAgent.Width + c] = toks[c][0];
                }
                if (r < MinesweeperAgent.Height - 1)
                {
                    line = Console.ReadLine();
                }
            }
            
            agent.UpdateBoard(boardState);
            var moves = agent.GetNextMoves();
            Console.WriteLine(string.Join(" ", moves));
            Console.Out.Flush();
        }
    }
}

public class MinesweeperAgent
{
    public const int Height = 16;
    public const int Width = 30;
    public const int Size = Height * Width;
    public const int TotalMines = 99;
    private const int MaxBacktrackSize = 15; 

    private readonly GridState _grid;
    private readonly HashSet<int> _safeToClick;
    private readonly HashSet<int> _newlyFlagged;
    private readonly Stopwatch _stopwatch;
    private bool _isFirstMove = true;

    public MinesweeperAgent()
    {
        _grid = new GridState();
        _safeToClick = new HashSet<int>();
        _newlyFlagged = new HashSet<int>();
        _stopwatch = new Stopwatch();
    }

    public void UpdateBoard(char[] input)
    {
        _grid.Update(input);
        _safeToClick.Clear();
        _newlyFlagged.Clear();
    }

    public List<string> GetNextMoves()
    {
        _stopwatch.Restart();

        int detSafePre = 0, detFlagsPre = 0;
        int probSafe = 0, probFlags = 0;
        string probSummary = "Not run.";
        
        RunDeterministicLoop();
        detSafePre = _safeToClick.Count;
        detFlagsPre = _newlyFlagged.Count;

        var bestGuess = -1;
        if (_safeToClick.Count == 0)
        {
            probSummary = RunProbabilisticPass();
            probSafe = _safeToClick.Count - detSafePre;
            probFlags = _newlyFlagged.Count - detFlagsPre;
        }

        if (_safeToClick.Count == 0 && _grid.GetUnknownCount() > _grid.GetFlaggedCount())
        {
            bestGuess = FindBestGuess();
            if (bestGuess != -1)
            {
                _safeToClick.Add(bestGuess);
            }
        }
        
        var moves = FormatMoves(out var finalMove);
        PrintDebugSummary(finalMove, bestGuess, detSafePre, detFlagsPre, probSummary, probSafe, probFlags);
        
        _isFirstMove = false;
        _stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Turn took {_stopwatch.ElapsedMilliseconds}ms.");
        return moves;
    }
    
    private void RunDeterministicLoop()
    {
        while (true)
        {
            var flagged = FindAndFlagDeterminedMines();
            var cleared = FindAndMarkDeterminedSafeCells();
            var subsetted = FindSubsetMinesAndSafes();
            if (!flagged && !cleared && !subsetted) break;
        }
    }

    private bool FindAndFlagDeterminedMines()
    {
        var newFlags = false;
        for (var i = 0; i < Size; i++)
        {
            var clue = _grid.GetClueValue(i);
            if (clue <= 0) continue;
            var neighbors = _grid.GetNeighbors(i);
            var flaggedCount = 0;
            var hidden = new List<int>();
            foreach (var n in neighbors)
            {
                if (_grid.IsFlagged(n)) flaggedCount++;
                else if (_grid.IsUnknown(n)) hidden.Add(n);
            }
            if (hidden.Count > 0 && clue == flaggedCount + hidden.Count)
            {
                foreach (var n in hidden)
                {
                    if (_grid.TrySetFlag(n))
                    {
                        _newlyFlagged.Add(n);
                        newFlags = true;
                    }
                }
            }
        }
        return newFlags;
    }

    private bool FindAndMarkDeterminedSafeCells()
    {
        var newSafe = false;
        for (var i = 0; i < Size; i++)
        {
            var clue = _grid.GetClueValue(i);
            if (clue <= 0) continue;
            var neighbors = _grid.GetNeighbors(i);
            var flaggedCount = 0;
            foreach (var n in neighbors)
            {
                if (_grid.IsFlagged(n)) flaggedCount++;
            }
            if (clue == flaggedCount)
            {
                foreach (var n in neighbors)
                {
                    if (_grid.IsUnknown(n) && !_grid.IsFlagged(n) && !_safeToClick.Contains(n))
                    {
                        _safeToClick.Add(n);
                        newSafe = true;
                    }
                }
            }
        }
        return newSafe;
    }
    
    private bool FindSubsetMinesAndSafes()
    {
        bool changed = false;
        var constraints = _grid.GetActiveConstraints();

        for (int i = 0; i < constraints.Count; i++)
        {
            for (int j = 0; j < constraints.Count; j++)
            {
                if (i == j) continue;

                var c1 = constraints[i];
                var c2 = constraints[j];

                if (c1.Neighbors.IsSupersetOf(c2.Neighbors))
                {
                    var diffCells = new HashSet<int>(c1.Neighbors);
                    diffCells.ExceptWith(c2.Neighbors);

                    if (diffCells.Count > 0)
                    {
                        int mineDiff = c1.MinesLeft - c2.MinesLeft;
                        if (mineDiff == diffCells.Count)
                        {
                            foreach (var cell in diffCells)
                            {
                                if (_grid.TrySetFlag(cell))
                                {
                                    _newlyFlagged.Add(cell);
                                    changed = true;
                                }
                            }
                        }
                        if (mineDiff == 0)
                        {
                            foreach (var cell in diffCells)
                            {
                                if (!_safeToClick.Contains(cell) && !_grid.IsFlagged(cell))
                                {
                                    _safeToClick.Add(cell);
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return changed;
    }

    private string RunProbabilisticPass()
    {
        var components = _grid.PartitionFrontier();
        if (components.Count == 0) return "Frontier is empty.";
        
        int exactCount = 0;
        int heuristicCount = 0;

        foreach (var component in components)
        {
            if (component.Cells.Count <= MaxBacktrackSize)
            {
                var solution = ExactProbabilitySolver.Solve(component);
                _grid.ComponentSolutions[component] = solution;
                foreach (var (cell, prob) in solution.Probabilities)
                {
                    _grid.Probabilities[cell] = prob;
                    if (prob < 1e-9) _safeToClick.Add(cell);
                    if (prob > 1.0 - 1e-9 && _grid.TrySetFlag(cell)) _newlyFlagged.Add(cell);
                }
                exactCount++;
            }
            else
            {
                var probabilities = HeuristicProbabilitySolver.Solve(component);
                foreach (var (cell, prob) in probabilities)
                {
                    _grid.Probabilities[cell] = prob;
                }
                heuristicCount++;
            }
        }
        
        if (_safeToClick.Count > 0 || _newlyFlagged.Count > 0)
        {
            Console.Error.WriteLine("[DEBUG] Probabilistic pass yielded new info. Re-running deterministic loop.");
            RunDeterministicLoop();
        }
        
        return $"{components.Count} components ({exactCount} Exact, {heuristicCount} Heuristic).";
    }

    private int FindBestGuess()
    {
        if (_isFirstMove)
        {
            return (Height / 2) * Width + (Width / 2);
        }

        var (frontier, sea) = _grid.GetFrontierAndSea();
        var allUnknown = new HashSet<int>(frontier);
        allUnknown.UnionWith(sea);
        if (allUnknown.Count == 0) return -1;

        double expectedFrontierMines = 0.0;

        foreach (var component in _grid.ComponentSolutions.Keys)
        {
            var solution = _grid.ComponentSolutions[component];
            double expectedMines = 0;
            long totalSolutions = 0;
            foreach (var (mineCount, numSolutions) in solution.MineCountDistribution)
            {
                expectedMines += mineCount * numSolutions;
                totalSolutions += numSolutions;
            }
            if (totalSolutions > 0)
            {
                expectedFrontierMines += expectedMines / totalSolutions;
            }
        }
        
        var solvedFrontierCells = new HashSet<int>(_grid.ComponentSolutions.Keys.SelectMany(c => c.Cells));
        foreach (var cell in frontier.Where(c => !solvedFrontierCells.Contains(c)))
        {
            if (_grid.Probabilities[cell] >= 0)
            {
                expectedFrontierMines += _grid.Probabilities[cell];
            }
        }
        
        var remainingMines = TotalMines - _grid.GetFlaggedCount();
        var seaMines = remainingMines - expectedFrontierMines;
        var seaProb = sea.Count > 0 ? Math.Max(0, seaMines / sea.Count) : 2.0;

        double minProb = 2.0;
        int bestCell = -1;

        foreach (var cell in allUnknown)
        {
            double currentProb = sea.Contains(cell) ? seaProb : (_grid.Probabilities[cell] >= 0 ? _grid.Probabilities[cell] : 1.0);
            if (currentProb < minProb)
            {
                minProb = currentProb;
                bestCell = cell;
            }
        }
        
        foreach(var cell in allUnknown)
        {
            if(!_grid.ComponentSolutions.Keys.Any(c => c.Cells.Contains(cell)))
            {
                 _grid.Probabilities[cell] = sea.Contains(cell) ? seaProb : _grid.Probabilities[cell];
            }
        }

        return bestCell;
    }
    
    private List<string> FormatMoves(out int finalMove)
    {
        finalMove = -1;
        var moves = new List<string>();
        if (_safeToClick.Count > 0)
        {
            foreach (var cell in _safeToClick)
            {
                if (_grid.IsUnknown(cell) && !_grid.IsFlagged(cell))
                {
                    finalMove = cell;
                    break;
                }
            }
            if (finalMove != -1)
            {
                moves.Add((finalMove % Width).ToString());
                moves.Add((finalMove / Width).ToString());
            }
        }
        
        foreach (var flag in _newlyFlagged)
        {
            moves.Add((flag % Width).ToString());
            moves.Add((flag / Width).ToString());
        }
        return moves;
    }
    
    private void PrintDebugSummary(int finalMove, int bestGuess, int detSafe, int detFlags, string probSummary, int probSafe, int probFlags)
    {
        Console.Error.WriteLine("[DEBUG] --- Turn Summary ---");
        Console.Error.WriteLine($"[DEBUG] State: Unknown: {_grid.GetUnknownCount()}, Flagged: {_grid.GetFlaggedCount()}, Mines Left: {TotalMines - _grid.GetFlaggedCount()}");
        Console.Error.WriteLine($"[DEBUG] Solvers: Found {detSafe + probSafe} safe, {detFlags + probFlags} new flags.");
        Console.Error.WriteLine($"[DEBUG] Probability Details: {probSummary}");

        if (finalMove != -1)
        {
            if (bestGuess == finalMove)
            {
                double mineProb = _grid.Probabilities[bestGuess];
                double safeProb = (1.0 - mineProb) * 100.0;
                Console.Error.WriteLine($"[DEBUG] Final Action: GUESSING ({finalMove % Width}, {finalMove / Width}) with {safeProb:F1}% safety chance.");
            }
            else
            {
                Console.Error.WriteLine($"[DEBUG] Final Action: Clicking guaranteed safe cell ({finalMove % Width}, {finalMove / Width}). Total safe moves found: {_safeToClick.Count}.");
            }
        }
        else if (_newlyFlagged.Count > 0)
        {
            Console.Error.WriteLine($"[DEBUG] Final Action: Flagging {_newlyFlagged.Count} new cells. No safe cells to click.");
        }
        else
        {
            Console.Error.WriteLine("[DEBUG] Final Action: No moves found. Game should be over.");
        }
    }
}

internal class GridState
{
    private readonly char[] _state;
    private readonly bool[] _flagged;
    private readonly List<int>[] _neighbors;
    public readonly double[] Probabilities;
    public readonly Dictionary<FrontierGroup, ComponentSolution> ComponentSolutions;

    public GridState()
    {
        _state = new char[MinesweeperAgent.Size];
        _flagged = new bool[MinesweeperAgent.Size];
        _neighbors = new List<int>[MinesweeperAgent.Size];
        Probabilities = new double[MinesweeperAgent.Size];
        ComponentSolutions = new Dictionary<FrontierGroup, ComponentSolution>();
        PrecomputeNeighbors();
    }

    public void Update(char[] input)
    {
        Array.Copy(input, _state, MinesweeperAgent.Size);
        Array.Fill(Probabilities, -1.0);
        ComponentSolutions.Clear();
    }
    
    public bool IsUnknown(int i) => _state[i] == '?';
    public bool IsFlagged(int i) => _flagged[i];
    public bool TrySetFlag(int i) { if (_flagged[i]) return false; _flagged[i] = true; return true; }
    public int GetClueValue(int i) => _state[i] >= '1' && _state[i] <= '8' ? _state[i] - '0' : 0;
    public char GetDisplayChar(int i) => _state[i];
    public List<int> GetNeighbors(int i) => _neighbors[i];
    public int GetFlaggedCount() { var c=0; for(var i=0;i<MinesweeperAgent.Size;i++) if(_flagged[i]) c++; return c; }
    public int GetUnknownCount() { var c=0; for(var i=0;i<MinesweeperAgent.Size;i++) if(_state[i]=='?') c++; return c; }
    
    public List<FrontierGroup> PartitionFrontier()
    {
        var components = new List<FrontierGroup>();
        var (frontier, cellToConstraints) = GetFrontierAndConstraints();
        var visited = new HashSet<int>();

        foreach (var cell in frontier)
        {
            if (visited.Contains(cell)) continue;
            var componentCells = new List<int>();
            var q = new Queue<int>();
            q.Enqueue(cell);
            visited.Add(cell);
            var relevantConstraints = new HashSet<Constraint>();

            while (q.Count > 0)
            {
                var current = q.Dequeue();
                componentCells.Add(current);
                if (cellToConstraints.TryGetValue(current, out var affectingConstraints))
                {
                    foreach(var constraint in affectingConstraints)
                    {
                        relevantConstraints.Add(constraint);
                        foreach (var neighbor in constraint.Neighbors)
                        {
                            if (frontier.Contains(neighbor) && !visited.Contains(neighbor))
                            {
                                visited.Add(neighbor);
                                q.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }
            components.Add(new FrontierGroup(componentCells, relevantConstraints.ToList()));
        }
        return components;
    }

    public (List<int> Frontier, List<int> Sea) GetFrontierAndSea()
    {
        var frontier = new List<int>();
        var sea = new List<int>();
        for (var i = 0; i < MinesweeperAgent.Size; i++)
        {
            if (IsUnknown(i) && !IsFlagged(i))
            {
                if (GetNeighbors(i).Any(n => GetClueValue(n) > 0)) frontier.Add(i);
                else sea.Add(i);
            }
        }
        return (frontier, sea);
    }
    
    public List<Constraint> GetActiveConstraints()
    {
        var constraints = new List<Constraint>();
        for (var i = 0; i < MinesweeperAgent.Size; i++)
        {
            var clueVal = GetClueValue(i);
            if (clueVal > 0)
            {
                var hidden = new HashSet<int>();
                var flaggedCount = 0;
                foreach (var n in GetNeighbors(i))
                {
                    if (IsUnknown(n) && !IsFlagged(n)) hidden.Add(n);
                    else if (IsFlagged(n)) flaggedCount++;
                }

                if (hidden.Count > 0)
                {
                    constraints.Add(new Constraint(clueVal - flaggedCount, hidden));
                }
            }
        }
        return constraints;
    }

    private (HashSet<int> Frontier, Dictionary<int, List<Constraint>> CellToConstraints) GetFrontierAndConstraints()
    {
        var frontier = new HashSet<int>();
        var cellToConstraints = new Dictionary<int, List<Constraint>>();
        var constraints = GetActiveConstraints();
        foreach (var constraint in constraints)
        {
            foreach (var h in constraint.Neighbors)
            {
                frontier.Add(h);
                if (!cellToConstraints.ContainsKey(h)) cellToConstraints[h] = new List<Constraint>();
                cellToConstraints[h].Add(constraint);
            }
        }
        return (frontier, cellToConstraints);
    }
    
    private void PrecomputeNeighbors()
    {
        var dr = new[] { -1, -1, -1, 0, 0, 1, 1, 1 };
        var dc = new[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        for (var i = 0; i < MinesweeperAgent.Size; i++)
        {
            _neighbors[i] = new List<int>(8);
            var r = i / MinesweeperAgent.Width;
            var c = i % MinesweeperAgent.Width;
            for (var k = 0; k < 8; k++)
            {
                var nr = r + dr[k];
                var nc = c + dc[k];
                if (nr >= 0 && nr < MinesweeperAgent.Height && nc >= 0 && nc < MinesweeperAgent.Width)
                {
                    _neighbors[i].Add(nr * MinesweeperAgent.Width + nc);
                }
            }
        }
    }
}

internal record ComponentSolution(Dictionary<int, double> Probabilities, Dictionary<int, long> MineCountDistribution);
internal static class ExactProbabilitySolver
{
    private static long _solutionCount;
    private static long[] _mineCounts;
    private static bool[] _assignment;
    private static List<int> _cells;
    private static List<Constraint> _constraints;
    private static int[] _cellToIndexMap;
    private static Dictionary<int, long> _mineCountDistribution;
    private const long MaxSolutions = 40000;
    
    public static ComponentSolution Solve(FrontierGroup component)
    {
        _cells = component.Cells;
        _constraints = component.Constraints;
        _solutionCount = 0;
        _mineCounts = new long[_cells.Count];
        _mineCountDistribution = new Dictionary<int, long>();
        _assignment = new bool[MinesweeperAgent.Size];
        _cellToIndexMap = new int[MinesweeperAgent.Size];
        Array.Fill(_cellToIndexMap, -1);
        for(var i=0; i<_cells.Count; i++) _cellToIndexMap[_cells[i]] = i;
        
        Backtrack(0, 0);

        var probabilities = new Dictionary<int, double>();
        if (_solutionCount > 0)
        {
            for (var i = 0; i < _cells.Count; i++)
            {
                probabilities[_cells[i]] = (double)_mineCounts[i] / _solutionCount;
            }
        }
        return new ComponentSolution(probabilities, _mineCountDistribution);
    }
    
    private static void Backtrack(int k, int currentMines)
    {
        if (_solutionCount > MaxSolutions) return;
        if (k == _cells.Count)
        {
            _solutionCount++;
            for (var i = 0; i < _cells.Count; i++) if (_assignment[_cells[i]]) _mineCounts[i]++;
            if (!_mineCountDistribution.ContainsKey(currentMines)) _mineCountDistribution[currentMines] = 0;
            _mineCountDistribution[currentMines]++;
            return;
        }
        var cell = _cells[k];
        _assignment[cell] = true;
        if (IsConsistent(k)) Backtrack(k + 1, currentMines + 1);
        _assignment[cell] = false;
        if (IsConsistent(k)) Backtrack(k + 1, currentMines);
    }

    private static bool IsConsistent(int k)
    {
        foreach (var constraint in _constraints)
        {
            if (!constraint.Neighbors.Contains(_cells[k])) continue;
            var minesFound = 0;
            var remainingUnknown = 0;
            foreach (var n in constraint.Neighbors)
            {
                var cellIdx = _cellToIndexMap[n];
                if (cellIdx != -1 && cellIdx <= k)
                {
                    if (_assignment[n]) minesFound++;
                }
                else
                {
                    remainingUnknown++;
                }
            }
            if (minesFound > constraint.MinesLeft || minesFound + remainingUnknown < constraint.MinesLeft) return false;
        }
        return true;
    }
}

internal static class HeuristicProbabilitySolver
{
    public static Dictionary<int, double> Solve(FrontierGroup component)
    {
        var probabilities = new Dictionary<int, double>();
        var cells = component.Cells;
        var constraints = component.Constraints;
        if (cells.Count == 0) return probabilities;
        
        var totalMines = 0.0;
        foreach(var constraint in constraints) totalMines += constraint.MinesLeft;
        
        var initialProb = cells.Count > 0 ? Math.Clamp(totalMines / cells.Count, 0.01, 0.99) : 0.0;
        foreach (var cell in cells) probabilities[cell] = initialProb;
        
        var cellToConstraints = new Dictionary<int, List<Constraint>>();
        foreach (var cell in cells) cellToConstraints[cell] = new List<Constraint>();
        foreach (var constraint in constraints)
        {
            foreach (var neighbor in constraint.Neighbors)
            {
                if (cellToConstraints.ContainsKey(neighbor))
                {
                    cellToConstraints[neighbor].Add(constraint);
                }
            }
        }

        var nextProbabilities = new Dictionary<int, double>(probabilities);
        for (var i = 0; i < 30; i++) 
        {
            foreach (var cell in cells)
            {
                var affectingConstraints = cellToConstraints[cell];
                if (affectingConstraints.Count == 0)
                {
                    nextProbabilities[cell] = probabilities[cell];
                    continue;
                }

                double scaleSum = 0.0;
                int scaleCount = 0;
                foreach (var constraint in affectingConstraints)
                {
                    double sumProb = 0.0;
                    foreach(var n in constraint.Neighbors)
                    {
                       if(probabilities.ContainsKey(n))
                       {
                           sumProb += probabilities[n];
                       }
                    }
                    
                    if (Math.Abs(sumProb) > 1e-9)
                    {
                        scaleSum += constraint.MinesLeft / sumProb;
                        scaleCount++;
                    }
                }

                if (scaleCount > 0)
                {
                    double avgScale = scaleSum / scaleCount;
                    nextProbabilities[cell] = Math.Clamp(probabilities[cell] * avgScale, 0.0, 1.0);
                }
                else
                {
                    nextProbabilities[cell] = probabilities[cell];
                }
            }
            
            foreach(var cell in cells)
            {
                probabilities[cell] = nextProbabilities[cell];
            }
        }
        return probabilities;
    }
}


internal record FrontierGroup(List<int> Cells, List<Constraint> Constraints);
internal record Constraint(int MinesLeft, HashSet<int> Neighbors);