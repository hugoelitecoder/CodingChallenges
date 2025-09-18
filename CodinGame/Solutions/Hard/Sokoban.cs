using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var dim = Console.ReadLine().Split(' ');
        var w = int.Parse(dim[0]);
        var h = int.Parse(dim[1]);
        var boxCount = int.Parse(dim[2]);
        var map = new char[h][];
        var targets = new List<(int, int)>();

        for (var y = 0; y < h; ++y)
        {
            var row = Console.ReadLine();
            map[y] = row.ToCharArray();
            for (var x = 0; x < w; ++x)
                if (row[x] == '*')
                    targets.Add((x, y));
        }

        var solver = new SokobanSolver(w, h, map, targets);

        PrintAugmentedMap(w, h, map, solver.DeadSquares);

        while (true)
        {
            var pos = Console.ReadLine().Split(' ');
            var px = int.Parse(pos[0]);
            var py = int.Parse(pos[1]);
            var boxes = new (int, int)[boxCount];
            for (var i = 0; i < boxCount; ++i)
            {
                var b = Console.ReadLine().Split(' ');
                boxes[i] = (int.Parse(b[0]), int.Parse(b[1]));
            }

            Console.Error.WriteLine($"\n[DEBUG] Turn {solver.Turn + 1} ");
            var boxPositions = boxes.Select(b => (b.Item1, b.Item2)).ToHashSet();
            PrintBoard(w, h, solver.Walls, solver.Targets, solver.DeadSquares, px, py, boxPositions);

            var move = solver.GetNextMove(px, py, boxes);

            if (solver.LastSearchResult != null)
            {
                Console.Error.WriteLine(solver.LastSearchResult.GetDebugMessage());
                if (solver.LastSearchResult.Success)
                {
                    Console.Error.WriteLine($"[DEBUG] Plan: {string.Join("", solver.LastSearchResult.Plan)}");
                }
            }

            Console.WriteLine(move);
        }
    }

    private static void PrintBoard(int w, int h, ISet<int> walls, ISet<int> targets, ISet<int> deadSquares, int px, int py, ISet<(int, int)> boxes)
    {
        var boxPositions = new HashSet<int>(boxes.Select(b => b.Item2 * w + b.Item1));
        var playerPos = py * w + px;

        for (int y = 0; y < h; y++)
        {
            var sb = new StringBuilder("[DEBUG] ");
            for (int x = 0; x < w; x++)
            {
                var pos = y * w + x;
                if (pos == playerPos) sb.Append('P');
                else if (boxPositions.Contains(pos) && targets.Contains(pos)) sb.Append('$');
                else if (boxPositions.Contains(pos)) sb.Append('B');
                else if (deadSquares.Contains(pos)) sb.Append('X');
                else if (targets.Contains(pos)) sb.Append('*');
                else if (walls.Contains(pos)) sb.Append('#');
                else sb.Append('.');
            }
            Console.Error.WriteLine(sb.ToString());
        }
    }

    private static void PrintAugmentedMap(int w, int h, char[][] initialMap, ISet<int> deadSquares)
    {
        Console.Error.WriteLine("[DEBUG] Dead zones ('X'):");
        for (int y = 0; y < h; y++)
        {
            var sb = new StringBuilder("[DEBUG] ");
            for (int x = 0; x < w; x++)
            {
                var pos = y * w + x;
                if (deadSquares.Contains(pos)) sb.Append('X');
                else sb.Append(initialMap[y][x]);
            }
            Console.Error.WriteLine(sb.ToString());
        }
    }
}

public record SearchResult(bool Success, List<char> Plan, long ElapsedMilliseconds, int NodesExpanded, string StatusMessage)
{
    public string GetDebugMessage() =>
        $"[DEBUG] {StatusMessage} Time: {ElapsedMilliseconds}ms. Nodes: {NodesExpanded}. " +
        (Success ? $"Player moves: {Plan.Count}." : "");
}

class SokobanSolver
{
    public int Turn => _turn;
    public ISet<int> Walls => _walls;
    public ISet<int> Targets => _targetsSet;
    public ISet<int> DeadSquares => _simpleDeadSquares;
    public SearchResult LastSearchResult { get; private set; }

    private readonly int _w;
    private readonly int _h;
    private readonly int[] _targetsArr;
    private readonly HashSet<int> _walls;
    private readonly HashSet<int> _targetsSet;
    private readonly HashSet<int> _simpleDeadSquares;
    private readonly int[,] _distanceMatrix;
    private readonly Queue<char> _plan = new Queue<char>();
    private State _expectedState;
    private int _fallbackIdx = 0;
    private int _turn = 0;
    private static readonly int[] Dx = { 0, 0, 1, -1 };
    private static readonly int[] Dy = { -1, 1, 0, 0 };
    private static readonly char[] Dc = { 'U', 'D', 'R', 'L' };

    public SokobanSolver(int w, int h, char[][] map, List<(int, int)> targetsCoords)
    {
        _w = w;
        _h = h;
        _walls = new HashSet<int>();
        for (var y = 0; y < h; ++y)
            for (var x = 0; x < w; ++x)
                if (map[y][x] == '#')
                    _walls.Add(y * w + x);
        _targetsArr = targetsCoords.Select(t => t.Item2 * w + t.Item1).ToArray();
        _targetsSet = new HashSet<int>(_targetsArr);
        _distanceMatrix = PrecomputeAllPairDistances();
        _simpleDeadSquares = PrecomputeSimpleDeadZones();
    }

    public char GetNextMove(int px, int py, (int, int)[] boxes)
    {
        _turn++;
        LastSearchResult = null;
        var boxArr = boxes.Select(b => b.Item2 * _w + b.Item1).OrderBy(x => x).ToArray();
        var current = new State(px, py, boxArr);

        string reasonForRecalc = null;
        if (_turn == 1) reasonForRecalc = "Calculating initial plan...";
        else if (_plan.Count == 0) reasonForRecalc = "Plan finished. Recalculating...";
        else if (_expectedState == null || !_expectedState.Equals(current)) reasonForRecalc = "State mismatch. Recalculating...";

        bool needPlan = reasonForRecalc != null;

        if (needPlan)
        {
            _plan.Clear();
            _expectedState = current;
            var searchResult = SearchMacro(current, _turn == 1 ? 9900 : 45);
            LastSearchResult = searchResult;

            if (searchResult.Success && searchResult.Plan.Count > 0)
            {
                foreach (var m in searchResult.Plan) _plan.Enqueue(m);
            }
            else
            {
                return FallbackMove(px, py, boxArr);
            }
        }

        if (_plan.Count > 0)
        {
            var move = _plan.Dequeue();
            _expectedState = NextState(_expectedState, move);
            return move;
        }

        return FallbackMove(px, py, boxArr);
    }

    private SearchResult SearchMacro(State start, int maxMs)
    {
        var visited = new Dictionary<State, int>();
        var open = new PriorityQueue<MacroNode, int>();
        int nodesExpanded = 0;

        int startHeuristic = Heuristic(start.Boxes);
        if (startHeuristic == int.MaxValue)
            return new SearchResult(false, null, 0, 0, "Start state is unsolvable (no path from box to target).");

        open.Enqueue(new MacroNode(start, null, null, 0), startHeuristic);
        visited.Add(start, 0);

        var timer = Stopwatch.StartNew();
        if (IsGoal(start.Boxes))
        {
            return new SearchResult(true, new List<char>(), timer.ElapsedMilliseconds, 0, "Start state is already goal state.");
        }

        while (open.Count > 0)
        {
            if (timer.ElapsedMilliseconds > maxMs)
            {
                return new SearchResult(false, null, timer.ElapsedMilliseconds, nodesExpanded, "Search timed out.");
            }

            var node = open.Dequeue();
            nodesExpanded++;

            if (visited.ContainsKey(node.State) && visited[node.State] < node.PlayerMoveCost) continue;

            if (IsGoal(node.State.Boxes))
            {
                var fullMoves = new List<char>();
                var trace = node;
                while (trace.Parent != null)
                {
                    fullMoves.InsertRange(0, trace.PusherMoves);
                    trace = trace.Parent;
                }
                return new SearchResult(true, fullMoves, timer.ElapsedMilliseconds, nodesExpanded, "Solution found!");
            }

            var reachablePlayerPos = GetPusherReachableAll(node.State.PX, node.State.PY, node.State.Boxes, out var parentDict);
            var currentBoxesSet = new HashSet<int>(node.State.Boxes);

            for (int i = 0; i < node.State.Boxes.Length; i++)
            {
                int boxPos = node.State.Boxes[i];
                for (var d = 0; d < 4; ++d)
                {
                    int playerPushFromPos = Linear(boxPos % _w - Dx[d], boxPos / _w - Dy[d]);
                    if (playerPushFromPos == -1 || _walls.Contains(playerPushFromPos) || !reachablePlayerPos.Contains(playerPushFromPos)) continue;

                    int boxNewLinearPos = Linear(boxPos % _w + Dx[d], boxPos / _w + Dy[d]);
                    if (boxNewLinearPos == -1 || _walls.Contains(boxNewLinearPos) || currentBoxesSet.Contains(boxNewLinearPos)) continue;

                    var newBoxesAfterPush = (int[])node.State.Boxes.Clone();
                    newBoxesAfterPush[i] = boxNewLinearPos;
                    Array.Sort(newBoxesAfterPush);

                    if (IsDeadlock(newBoxesAfterPush)) continue;

                    var movesToPush = new List<char>();
                    BuildMoves(parentDict, playerPushFromPos, Linear(node.State.PX, node.State.PY), movesToPush);
                    movesToPush.Add(Dc[d]);

                    int newPlayerMoveCost = node.PlayerMoveCost + movesToPush.Count;
                    var nextState = new State(boxPos % _w, boxPos / _w, newBoxesAfterPush);

                    if (!visited.ContainsKey(nextState) || newPlayerMoveCost < visited[nextState])
                    {
                        int heuristicVal = Heuristic(newBoxesAfterPush);
                        if (heuristicVal == int.MaxValue) continue;

                        int fValue = newPlayerMoveCost + heuristicVal;

                        visited[nextState] = newPlayerMoveCost;
                        open.Enqueue(new MacroNode(nextState, node, movesToPush, newPlayerMoveCost), fValue);
                    }
                }
            }
        }

        return new SearchResult(false, null, timer.ElapsedMilliseconds, nodesExpanded, "Search exhausted.");
    }

    private int Heuristic(int[] boxes)
    {
        int totalMinDist = 0;
        bool[] targetsUsed = new bool[_targetsArr.Length];

        foreach (var boxPos in boxes)
        {
            if (_targetsSet.Contains(boxPos)) continue;

            int minDistToTarget = int.MaxValue;
            int bestTargetIdx = -1;

            for (int i = 0; i < _targetsArr.Length; i++)
            {
                if (targetsUsed[i]) continue;

                int targetPos = _targetsArr[i];
                if (Array.IndexOf(boxes, targetPos) > -1) continue;

                int dist = _distanceMatrix[boxPos, targetPos];
                if (dist != -1 && dist < minDistToTarget)
                {
                    minDistToTarget = dist;
                    bestTargetIdx = i;
                }
            }

            if (bestTargetIdx != -1)
            {
                totalMinDist += minDistToTarget;
                targetsUsed[bestTargetIdx] = true;
            }
            else
            {
                return int.MaxValue;
            }
        }
        return totalMinDist;
    }

    private bool IsDeadlock(int[] allBoxes)
    {
        var boxSet = new HashSet<int>(allBoxes);
        foreach (var boxPos in allBoxes)
        {
            if (_simpleDeadSquares.Contains(boxPos)) return true;

            int x = boxPos % _w;
            int y = boxPos / _w;

            if (boxSet.Contains(Linear(x + 1, y)) && boxSet.Contains(Linear(x, y + 1)) && boxSet.Contains(Linear(x + 1, y + 1)))
            {
                if (!_targetsSet.Contains(boxPos) || !_targetsSet.Contains(Linear(x + 1, y)) || !_targetsSet.Contains(Linear(x, y + 1)) || !_targetsSet.Contains(Linear(x + 1, y + 1)))
                    return true;
            }
        }
        return false;
    }

    private State NextState(State s, char move)
    {
        int dir = Array.IndexOf(Dc, move);
        var npx = s.PX + Dx[dir];
        var npy = s.PY + Dy[dir];
        var newBoxes = (int[])s.Boxes.Clone();
        int pushedBoxOriginalPos = npy * _w + npx;
        int boxIdx = Array.IndexOf(newBoxes, pushedBoxOriginalPos);
        if (boxIdx != -1)
        {
            newBoxes[boxIdx] = (npy + Dy[dir]) * _w + (npx + Dx[dir]);
        }
        Array.Sort(newBoxes);
        return new State(npx, npy, newBoxes);
    }

    private char FallbackMove(int px, int py, int[] boxArr)
    {
        for (var i = 0; i < 4; ++i)
        {
            var d = (_fallbackIdx + i) % 4;
            var nposLinear = Linear(px + Dx[d], py + Dy[d]);
            if (nposLinear == -1 || _walls.Contains(nposLinear) || Array.IndexOf(boxArr, nposLinear) > -1) continue;
            _fallbackIdx = d;
            return Dc[d];
        }
        return Dc[(_fallbackIdx + 1) % 4];
    }

    private int[,] PrecomputeAllPairDistances()
    {
        int size = _w * _h;
        var dists = new int[size, size];
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++) dists[i, j] = -1;

        for (int startNode = 0; startNode < size; startNode++)
        {
            if (_walls.Contains(startNode)) continue;
            dists[startNode, startNode] = 0;
            var q = new Queue<int>();
            q.Enqueue(startNode);
            while (q.Count > 0)
            {
                var u = q.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    int v = Linear(u % _w + Dx[d], u / _w + Dy[d]);
                    if (v != -1 && !_walls.Contains(v) && dists[startNode, v] == -1)
                    {
                        dists[startNode, v] = dists[startNode, u] + 1;
                        q.Enqueue(v);
                    }
                }
            }
        }
        return dists;
    }

    private HashSet<int> PrecomputeSimpleDeadZones()
    {
        var dead = new HashSet<int>();
        for (int y = 0; y < _h; y++)
        {
            for (int x = 0; x < _w; x++)
            {
                int pos = Linear(x, y);
                if (_targetsSet.Contains(pos) || _walls.Contains(pos)) continue;

                bool wallN = _walls.Contains(Linear(x, y - 1));
                bool wallS = _walls.Contains(Linear(x, y + 1));
                bool wallE = _walls.Contains(Linear(x + 1, y));
                bool wallW = _walls.Contains(Linear(x - 1, y));
                if ((wallN || wallS) && (wallE || wallW)) dead.Add(pos);
            }
        }
        return dead;
    }

    private void BuildMoves(Dictionary<int, int> parentMap, int end, int start, List<char> moves)
    {
        if (start == end) return;
        var pathPositions = new List<int>();
        var curr = end;
        while (curr != start)
        {
            pathPositions.Add(curr);
            if (!parentMap.TryGetValue(curr, out curr)) { moves.Clear(); return; }
        }
        pathPositions.Reverse();
        int prevPos = start;
        foreach (var pos in pathPositions)
        {
            int diff = pos - prevPos;
            if (diff == 1) moves.Add('R');
            else if (diff == -1) moves.Add('L');
            else if (diff == _w) moves.Add('D');
            else if (diff == -_w) moves.Add('U');
            prevPos = pos;
        }
    }

    private HashSet<int> GetPusherReachableAll(int px, int py, int[] boxes, out Dictionary<int, int> parentMap)
    {
        var q = new Queue<int>();
        var seen = new HashSet<int>();
        parentMap = new Dictionary<int, int>();
        var boxSet = new HashSet<int>(boxes);
        var startPos = Linear(px, py);
        q.Enqueue(startPos);
        seen.Add(startPos);
        while (q.Count > 0)
        {
            var curr = q.Dequeue();
            for (var d = 0; d < 4; ++d)
            {
                var next = Linear(curr % _w + Dx[d], curr / _w + Dy[d]);
                if (next != -1 && !_walls.Contains(next) && !boxSet.Contains(next) && seen.Add(next))
                {
                    q.Enqueue(next);
                    parentMap[next] = curr;
                }
            }
        }
        return seen;
    }

    private bool IsGoal(int[] boxes)
    {
        foreach (var boxPos in boxes) if (!_targetsSet.Contains(boxPos)) return false;
        return true;
    }

    private int Linear(int x, int y)
    {
        if (x < 0 || x >= _w || y < 0 || y >= _h) return -1;
        return y * _w + x;
    }

    private class State : IEquatable<State>
    {
        public int PX { get; }
        public int PY { get; }
        public int[] Boxes { get; }
        private readonly int _hashCode;

        public State(int px, int py, int[] boxes)
        {
            PX = px; PY = py; Boxes = boxes;
            int hc = 17;
            hc = hc * 23 + PX;
            hc = hc * 23 + PY;
            hc = hc * 23 + new BoxState(Boxes).GetHashCode();
            _hashCode = hc;
        }
        public bool Equals(State other)
        {
            if (other is null) return false;
            if (PX != other.PX || PY != other.PY || Boxes.Length != other.Boxes.Length) return false;
            for (int i = 0; i < Boxes.Length; i++)
            {
                if (Boxes[i] != other.Boxes[i]) return false;
            }
            return true;
        }
        public override bool Equals(object obj) => Equals(obj as State);
        public override int GetHashCode() => _hashCode;
    }

    private class BoxState : IEquatable<BoxState>
    {
        private readonly int[] _boxes;
        private readonly int _hashCode;
        public BoxState(int[] boxes)
        {
            _boxes = boxes;
            int hc = 17;
            foreach (var b in _boxes) hc = hc * 31 + b;
            _hashCode = hc;
        }
        public bool Equals(BoxState other)
        {
            if (_boxes.Length != other._boxes.Length) return false;
            for (var i = 0; i < _boxes.Length; ++i) if (_boxes[i] != other._boxes[i]) return false;
            return true;
        }
        public override bool Equals(object obj) => Equals(obj as BoxState);
        public override int GetHashCode() => _hashCode;
    }

    private class MacroNode
    {
        public State State { get; }
        public MacroNode Parent { get; }
        public List<char> PusherMoves { get; }
        public int PlayerMoveCost { get; }

        public MacroNode(State s, MacroNode p, List<char> moves, int playerMoveCost)
        {
            State = s; Parent = p; PusherMoves = moves; PlayerMoveCost = playerMoveCost;
        }
    }
}