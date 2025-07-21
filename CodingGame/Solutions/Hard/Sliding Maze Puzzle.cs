using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var R = int.Parse(inputs[0]);
        var C = int.Parse(inputs[1]);
        var grid = new char[9, 9];
        for (var i = 0; i < 9; i++)
        {
            var row = Console.ReadLine();
            for (var j = 0; j < 9; j++)
            {
                grid[i, j] = row[j];
            }
        }

        var solver = new MazeSolver(grid);
        var solution = solver.Solve(R, C);
        
        if (solution != null)
        {
            Console.WriteLine(solution.Count);
            foreach (var move in solution)
            {
                Console.WriteLine(move.ToString());
            }
        }
    }
}

public class Move
{
    public char Type { get; }
    public char Dir { get; }

    public Move(char type, char dir)
    {
        Type = type;
        Dir = dir;
    }

    public override string ToString() => $"{Type} {Dir}";
}

public class State
{
    public int PIndex { get; }
    public int WIndex { get; }
    public int[] Perm { get; }

    public State(int pIndex, int wIndex, int[] perm)
    {
        PIndex = pIndex;
        WIndex = wIndex;
        Perm = perm;
    }
}

public class StateEqualityComparer : IEqualityComparer<State>
{
    public bool Equals(State s1, State s2)
    {
        if (s1 == null || s2 == null) return s1 == s2;
        if (s1.PIndex != s2.PIndex || s1.WIndex != s2.WIndex)
        {
            return false;
        }
        return s1.Perm.SequenceEqual(s2.Perm);
    }

    public int GetHashCode(State s)
    {
        var hashCode = new HashCode();
        hashCode.Add(s.PIndex);
        hashCode.Add(s.WIndex);
        foreach (var p in s.Perm)
        {
            hashCode.Add(p);
        }
        return hashCode.ToHashCode();
    }
}

public class MazeSolver
{
    private readonly List<string> _baseTiles = new List<string>();
    private int _waterTileId = -1;
    private bool[] _isFloor0Tile;
    private HashSet<int> _possibleExitTiles;

    private static readonly char[] Dirs = { 'v', '<', '>', '^' };
    private static readonly Dictionary<char, int> DirToDelta = new Dictionary<char, int>
    {
        { 'v', 3 }, { '<', -1 }, { '>', 1 }, { '^', -3 }
    };
    private static readonly Dictionary<char, (int dr, int dc)> TileMoveSourceOffsets = new Dictionary<char, (int dr, int dc)>
    {
        { 'v', (-1, 0) }, { '<', (0, 1) }, { '>', (0, -1) }, { '^', (1, 0) }
    };

    public MazeSolver(char[,] initialGrid)
    {
        ParseAndPrecompute(initialGrid);
    }

    public List<Move> Solve(int startTileR, int startTileC)
    {
        var varStartPIndex = startTileR * 3 + startTileC;
        var varInitialPermutation = Enumerable.Range(0, 9).ToArray();
        var varStartWIndex = Array.IndexOf(varInitialPermutation, _waterTileId);
        var varStartState = new State(varStartPIndex, varStartWIndex, varInitialPermutation);

        var queue = new Queue<State>();
        queue.Enqueue(varStartState);
        var cameFrom = new Dictionary<State, (State parent, Move move)>(new StateEqualityComparer())
        {
            [varStartState] = (null, null)
        };

        while (queue.Count > 0)
        {
            var currentState = queue.Dequeue();
            var currentPerm = currentState.Perm;
            var currentPIndex = currentState.PIndex;
            var currentWIndex = currentState.WIndex;

            if (currentPIndex == 0 && _possibleExitTiles.Contains(currentPerm[0]))
            {
                return ReconstructPath(currentState, cameFrom);
            }

            foreach (var dir in Dirs)
            {
                if (!TryGetNeighbor(currentPIndex, dir, out var nextPIndex)) continue;
                
                if (CanMovePlayer(currentPIndex, nextPIndex, currentPerm))
                {
                    var nextState = new State(nextPIndex, currentWIndex, currentPerm);
                    if (!cameFrom.ContainsKey(nextState))
                    {
                        cameFrom[nextState] = (currentState, new Move('P', dir));
                        queue.Enqueue(nextState);
                    }
                }
            }

            if (_isFloor0Tile[currentPerm[currentPIndex]])
            {
                var waterR = currentWIndex / 3;
                var waterC = currentWIndex % 3;
                
                foreach (var dir in Dirs)
                {
                    var (dr, dc) = TileMoveSourceOffsets[dir];
                    var srcR = waterR + dr;
                    var srcC = waterC + dc;

                    if (srcR < 0 || srcR >= 3 || srcC < 0 || srcC >= 3) continue;

                    var srcIndex = srcR * 3 + srcC;
                    if (srcIndex == currentPIndex) continue;

                    var newPerm = (int[])currentPerm.Clone();
                    (newPerm[currentWIndex], newPerm[srcIndex]) = (newPerm[srcIndex], newPerm[currentWIndex]);

                    var nextState = new State(currentPIndex, srcIndex, newPerm);
                    if (!cameFrom.ContainsKey(nextState))
                    {
                        cameFrom[nextState] = (currentState, new Move('T', dir));
                        queue.Enqueue(nextState);
                    }
                }
            }
        }
        return null;
    }
    
    private void ParseAndPrecompute(char[,] grid)
    {
        for (var tr = 0; tr < 3; tr++)
        {
            for (var tc = 0; tc < 3; tc++)
            {
                var sb = new StringBuilder();
                for (var lr = 0; lr < 3; lr++)
                    for (var lc = 0; lc < 3; lc++)
                        sb.Append(grid[tr * 3 + lr, tc * 3 + lc]);
                _baseTiles.Add(sb.ToString());
            }
        }

        _waterTileId = _baseTiles.FindIndex(t => t[4] == '~');
        _isFloor0Tile = new bool[9];
        _possibleExitTiles = new HashSet<int>();

        for (var i = 0; i < 9; i++)
        {
            if (i == _waterTileId) continue;
            _isFloor0Tile[i] = _baseTiles[i].Contains('.');
            if (IsPossibleExitTile(i))
            {
                _possibleExitTiles.Add(i);
            }
        }
    }

    private bool IsPossibleExitTile(int tileId)
    {
        var tile = _baseTiles[tileId];
        var isF0 = _isFloor0Tile[tileId];
        char[] leftWall = { tile[0], tile[3], tile[6] };
    
        if (isF0) return leftWall.Any(c => c == '+');
        return leftWall.Any(c => c == '=');
    }

    private bool CanMovePlayer(int pFrom, int pTo, int[] perm)
    {
        var tileIdFrom = perm[pFrom];
        var tileIdTo = perm[pTo];
        var delta = pTo - pFrom;

        var wallIdxFrom = DirToDelta.First(kvp => kvp.Value == delta).Key switch
        {
            'v' => 7, '<' => 3, '>' => 5, '^' => 1, _ => -1
        };
        var wallIdxTo = wallIdxFrom switch
        {
            7 => 1, 3 => 5, 5 => 3, 1 => 7, _ => -1
        };

        var wallFrom = _baseTiles[tileIdFrom][wallIdxFrom];
        var wallTo = _baseTiles[tileIdTo][wallIdxTo];

        if (wallFrom == '#' || wallTo == '#') return false;

        return WallsFit(wallFrom, wallTo) && WallsFit(wallTo, wallFrom);
    }
    
    private bool WallsFit(char c1, char c2)
    {
        if (c1 == '.') return c2 == '.';
        if (c1 == '=') return c2 == '=' || c2 == '+';
        if (c1 == '+') return c2 == '=' || c2 == '+';
        return false;
    }

    private bool TryGetNeighbor(int index, char dir, out int neighborIndex)
    {
        var r = index / 3;
        var c = index % 3;
        var delta = DirToDelta[dir];
        
        if (dir == '<' || dir == '>')
        {
            var nc = c + delta;
            if (nc >= 0 && nc < 3) { neighborIndex = r * 3 + nc; return true; }
        }
        else 
        {
            var nr = r + delta / 3;
            if (nr >= 0 && nr < 3) { neighborIndex = nr * 3 + c; return true; }
        }

        neighborIndex = -1;
        return false;
    }

    private List<Move> ReconstructPath(State goalState, Dictionary<State, (State parent, Move move)> cameFrom)
    {
        var path = new LinkedList<Move>();
        path.AddFirst(new Move('P', '<'));

        var current = goalState;
        while (cameFrom.TryGetValue(current, out var prev) && prev.parent != null)
        {
            path.AddFirst(prev.move);
            current = prev.parent;
        }
        return path.ToList();
    }
}
