using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        
        var solver = new SokobanSolver(w, h, boxCount, map, targets);
        
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

            var move = solver.GetNextMove(px, py, boxes);
            Console.WriteLine(move);
        }
    }
}

class SokobanSolver
{
    private readonly int _w;
    private readonly int _h;
    private readonly int _boxCount;
    private readonly HashSet<int> _walls;
    private readonly List<int> _targetsList;
    private readonly HashSet<int> _targetsSet;

    private static readonly int[] Dx = { 0, 0, 1, -1 }; 
    private static readonly int[] Dy = { -1, 1, 0, 0 };
    private static readonly char[] Dc = { 'U', 'D', 'R', 'L' };

    private readonly Queue<char> _plan = new Queue<char>();
    private State _expectedState;
    private readonly int[] _targetArr; 
    private int _fallbackIdx = 0;
    private int _turn = 0;

    private readonly HashSet<int> _deadZone; 


    public SokobanSolver(int w, int h, int boxCount, char[][] map, List<(int, int)> targetsCoords)
    {
        _w = w;
        _h = h;
        _boxCount = boxCount;

        _walls = new HashSet<int>();
        for (var y = 0; y < h; ++y)
            for (var x = 0; x < w; ++x)
                if (map[y][x] == '#')
                    _walls.Add(y * _w + x);

        _targetsList = new List<int>();
        foreach (var t in targetsCoords)
            _targetsList.Add(t.Item2 * _w + t.Item1);
        
        _targetsSet = new HashSet<int>(_targetsList);
        _targetArr = _targetsList.ToArray();

        _deadZone = PrecomputeSimpleDeadZones();

        Console.Error.WriteLine($"Precomputed simple {_deadZone.Count} dead zones.");
        Console.Error.WriteLine($"Targets: {string.Join(",", _targetsList.Select(t => $"({t % _w},{t / _w})"))}. ");
        Console.Error.WriteLine($"Simple Dead zones: {string.Join(",", _deadZone.Select(d => $"({d % _w},{d / _w})"))}");
    }

    private HashSet<int> PrecomputeSimpleDeadZones()
    {
        var dead = new HashSet<int>();
        for (int y = 0; y < _h; y++)
        {
            for (int x = 0; x < _w; x++)
            {
                int pos = y * _w + x;
                if (_targetsSet.Contains(pos) || _walls.Contains(pos)) continue; 

                bool wallN = (y == 0) || _walls.Contains(Linear(x, y - 1));
                bool wallS = (y == _h - 1) || _walls.Contains(Linear(x, y + 1));
                bool wallE = (x == _w - 1) || _walls.Contains(Linear(x + 1, y));
                bool wallW = (x == 0) || _walls.Contains(Linear(x - 1, y));

                if ((wallN && wallE) || (wallN && wallW) || (wallS && wallE) || (wallS && wallW))
                {
                    dead.Add(pos);
                }
            }
        }
        return dead;
    }
    private int Linear(int x, int y) {
        if (x < 0 || x >= _w || y < 0 || y >= _h) return -1; 
        return y * _w + x;
    }


    public char GetNextMove(int px, int py, (int, int)[] boxes)
    {
        _turn++;
        var boxArr = boxes.Select(b => b.Item2 * _w + b.Item1).OrderBy(x => x).ToArray();
        var current = new State(px, py, boxArr);
        bool needPlan = (_turn == 1) || (_expectedState == null) || !_expectedState.Equals(current) || _plan.Count == 0;

        if (needPlan)
        {
            _plan.Clear();
            _expectedState = current; 
            
            List<char> moves = SearchMacro(current, _turn == 1 ? 9900 : 45); 
            if (moves != null) 
            {
                if (moves.Count > 0)
                {
                    foreach (var m in moves)
                        _plan.Enqueue(m);
                }
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
        Console.Error.WriteLine($"DEBUG GetNextMove (Turn {_turn}): Plan is empty after all checks. Resorting to fallback.");
        return FallbackMove(px, py, boxArr); 
    }

    private State NextState(State s, char move)
    {
        int dir = -1;
        for(int i=0; i<Dc.Length; ++i) if(Dc[i] == move) dir = i;
        
        var npx = s.PX + Dx[dir];
        var npy = s.PY + Dy[dir];

        var newBoxes = new int[s.Boxes.Length];
        Array.Copy(s.Boxes, newBoxes, newBoxes.Length);

        int pushedBoxOriginalPos = npy * _w + npx; 
        for (int i = 0; i < newBoxes.Length; i++)
        {
            if (newBoxes[i] == pushedBoxOriginalPos) 
            {
                newBoxes[i] = (npy + Dy[dir]) * _w + (npx + Dx[dir]); 
                break; 
            }
        }
        Array.Sort(newBoxes); 
        return new State(npx, npy, newBoxes);
    }

    private void PrintStats(int nodes, int? solutionDepth, bool solved, long ms, bool timedOut)
    {
        Console.Error.WriteLine($"Nodes expanded: {nodes}");
        if (solved && solutionDepth.HasValue) {
            Console.Error.WriteLine($"Solution depth: {solutionDepth.Value}");
        } else if (timedOut) {
            Console.Error.WriteLine("Search timed out.");
        } 
        else {
            Console.Error.WriteLine("No solution found (search space exhausted or pruned).");
        }
        Console.Error.WriteLine($"Time: {ms} ms");
    }

    private List<char> SearchMacro(State start, int maxMs)
    {
        var visited = new HashSet<State>(); 
        var open = new PriorityQueue<MacroNode>();
        
        int startHeuristic = Heuristic(start.Boxes);
        open.Enqueue(new MacroNode(start, null, null, 0, startHeuristic));
        visited.Add(start);

        int nodesExpanded = 0;
        bool solved = false;
        int? solutionDepth = null;
        var timer = Stopwatch.StartNew();

        if (IsGoal(start)) 
        {
            solved = true;
            solutionDepth = 0;
            timer.Stop();
            PrintStats(nodesExpanded, solutionDepth, solved, timer.ElapsedMilliseconds, false);
            return new List<char>(); 
        }

        while (open.Count > 0)
        {
            if (timer.ElapsedMilliseconds > maxMs)
            {
                timer.Stop();
                PrintStats(nodesExpanded, null, false, timer.ElapsedMilliseconds, true); 
                return null; 
            }

            var node = open.Dequeue();
            nodesExpanded++;

            if (IsGoal(node.State)) 
            {
                solved = true;
                var fullMoves = new List<char>();
                var trace = node;
                while (trace.Parent != null)
                {
                    if (trace.PusherMoves != null)
                        fullMoves.InsertRange(0, trace.PusherMoves);
                    trace = trace.Parent;
                }
                solutionDepth = fullMoves.Count;
                timer.Stop();
                PrintStats(nodesExpanded, solutionDepth, solved, timer.ElapsedMilliseconds, false); 
                return fullMoves;
            }

            var currentBoxesSet = new HashSet<int>(node.State.Boxes); 
            var reachablePlayerPos = GetPusherReachableAll(node.State.PX, node.State.PY, node.State.Boxes, out var parentDict);

            foreach (var playerPosToPushFrom in reachablePlayerPos) 
            {
                for (var d = 0; d < 4; ++d) 
                {
                    var boxOriginalX = playerPosToPushFrom % _w + Dx[d];
                    var boxOriginalY = playerPosToPushFrom / _w + Dy[d];
                    int boxOriginalLinearPos = Linear(boxOriginalX, boxOriginalY);

                    int boxIdx = -1; 
                    for(int k=0; k<node.State.Boxes.Length; ++k) {
                        if(node.State.Boxes[k] == boxOriginalLinearPos) {
                            boxIdx = k;
                            break;
                        }
                    }
                    if (boxIdx == -1) continue; 

                    var boxNewX = boxOriginalX + Dx[d];
                    var boxNewY = boxOriginalY + Dy[d];

                    if (boxNewX < 0 || boxNewX >= _w || boxNewY < 0 || boxNewY >= _h) continue; 
                    
                    int boxNewLinearPos = Linear(boxNewX, boxNewY);

                    if (_walls.Contains(boxNewLinearPos)) continue; 
                    if (currentBoxesSet.Contains(boxNewLinearPos)) continue; 

                    var movesToPush = new List<char>();
                    BuildMoves(parentDict, playerPosToPushFrom, Linear(node.State.PX, node.State.PY), movesToPush); 
                    movesToPush.Add(Dc[d]); 

                    var newBoxesAfterPush = new int[node.State.Boxes.Length];
                    Array.Copy(node.State.Boxes, newBoxesAfterPush, newBoxesAfterPush.Length);
                    newBoxesAfterPush[boxIdx] = boxNewLinearPos; 
                    Array.Sort(newBoxesAfterPush); 

                    if (IsDeadlock(boxNewLinearPos, newBoxesAfterPush)) { 
                        continue;
                    }
                    
                    var nextMacroState = new State(boxOriginalX, boxOriginalY, newBoxesAfterPush); 
                    
                    if (visited.Add(nextMacroState))
                    {
                        int newCost = node.Cost + movesToPush.Count;
                        int heuristicVal = Heuristic(newBoxesAfterPush);
                        open.Enqueue(new MacroNode(nextMacroState, node, movesToPush, newCost, newCost + heuristicVal));
                    } 
                }
            }
        }
        timer.Stop();
        PrintStats(nodesExpanded, null, false, timer.ElapsedMilliseconds, false); 
        return null; 
    }

    private void BuildMoves(Dictionary<int, (int prevPos, char moveChar)> parentMap, int endPlayerPosLinear, int startPlayerPosLinear, List<char> resultMoves)
    {
        var path = new List<char>();
        var current = endPlayerPosLinear;
        while (current != startPlayerPosLinear)
        {
            if (!parentMap.TryGetValue(current, out var prev))
            {
                resultMoves.Clear();
                return;
            }
            path.Add(prev.moveChar);
            current = prev.prevPos;
        }
        path.Reverse();
        resultMoves.AddRange(path);
    }

    private HashSet<int> GetPusherReachableAll(int playerX, int playerY, int[] currentBoxes, out Dictionary<int, (int prevPos, char moveChar)> parentMap)
    {
        var q = new Queue<int>();
        var seen = new HashSet<int>();
        parentMap = new Dictionary<int, (int, char)>();
        
        var boxSetForReachability = new HashSet<int>(currentBoxes); 
        var startPlayerLinearPos = Linear(playerX, playerY);

        q.Enqueue(startPlayerLinearPos);
        seen.Add(startPlayerLinearPos);
        
        while (q.Count > 0)
        {
            var currentPosLinear = q.Dequeue();
            var x = currentPosLinear % _w;
            var y = currentPosLinear / _w;

            for (var d = 0; d < 4; ++d)
            {
                var nx = x + Dx[d];
                var ny = y + Dy[d];
                
                if (nx < 0 || nx >= _w || ny < 0 || ny >= _h) continue;
                
                var nextPosLinear = Linear(nx, ny);
                if (_walls.Contains(nextPosLinear)) continue;
                if (boxSetForReachability.Contains(nextPosLinear)) continue;

                if (seen.Add(nextPosLinear))
                {
                    q.Enqueue(nextPosLinear);
                    parentMap[nextPosLinear] = (currentPosLinear, Dc[d]);
                }
            }
        }
        return seen;
    }

    private int Heuristic(int[] boxesState)
    {
        int numActiveBoxes = boxesState.Length;
        int numTargets = _targetArr.Length; 
        
        if (numActiveBoxes == 0) return 0; 
        if (numTargets == 0 && numActiveBoxes > 0) return numActiveBoxes * (_w + _h); 


        int N = numActiveBoxes; 
        int M = numTargets;   
        var cost = new int[N, M];

        for (var i = 0; i < N; ++i)
        {
            for (var j = 0; j < M; ++j)
            {
                var bx = boxesState[i] % _w;
                var by = boxesState[i] / _w;
                var tx = _targetArr[j] % _w;
                var ty = _targetArr[j] / _w;
                cost[i, j] = Math.Abs(bx - tx) + Math.Abs(by - ty);
            }
        }
        
        return Hungarian.Run(cost, _w, _h); 
    }
    
    private static class Hungarian
    {
        public static int Run(int[,] costMatrix, int mapW, int mapH) 
        {
            int n = costMatrix.GetLength(0); 
            int m = costMatrix.GetLength(1); 

            if (n == 0) return 0; 
            if (m == 0 && n > 0) return n * (mapW + mapH); 

            int[] u = new int[n + 1]; 
            int[] v = new int[m + 1]; 
            int[] p = new int[m + 1]; 
            int[] way = new int[m + 1];


            for (int i_box_loop = 1; i_box_loop <= n; i_box_loop++) 
            {
                p[0] = i_box_loop; 
                int j0_target_tree_root = 0; 
                
                var minv_slack = new int[m + 1]; 
                for (int k = 0; k <= m; k++) minv_slack[k] = int.MaxValue;
                
                var used_target_in_tree = new bool[m + 1]; 

                do 
                {
                    used_target_in_tree[j0_target_tree_root] = true; 
                    int i0_current_box_in_path = p[j0_target_tree_root];  
                                     
                    int delta_potential_update = int.MaxValue;
                    int j1_next_target_to_add = 0; 

                    for (int j_target_candidate = 1; j_target_candidate <= m; j_target_candidate++) 
                    {
                        if (!used_target_in_tree[j_target_candidate]) 
                        {
                            int current_reduced_cost = costMatrix[i0_current_box_in_path - 1, j_target_candidate - 1] - u[i0_current_box_in_path] - v[j_target_candidate]; 
                            
                            if (current_reduced_cost < minv_slack[j_target_candidate])
                            {
                                minv_slack[j_target_candidate] = current_reduced_cost;
                                way[j_target_candidate] = j0_target_tree_root; 
                            }
                            if (minv_slack[j_target_candidate] < delta_potential_update) 
                            {
                                delta_potential_update = minv_slack[j_target_candidate];
                                j1_next_target_to_add = j_target_candidate; 
                            }
                        }
                    }

                    for (int j_potential_update_idx = 0; j_potential_update_idx <= m; j_potential_update_idx++)
                    {
                        if (used_target_in_tree[j_potential_update_idx]) 
                        {
                            u[p[j_potential_update_idx]] += delta_potential_update; 
                            v[j_potential_update_idx] -= delta_potential_update;    
                        }
                        else 
                        {
                             minv_slack[j_potential_update_idx] -= delta_potential_update; 
                        }
                    }
                    j0_target_tree_root = j1_next_target_to_add; 
                } while (p[j0_target_tree_root] != 0); 
                                      
                do
                {
                    int j_prev_in_path = way[j0_target_tree_root]; 
                    p[j0_target_tree_root] = p[j_prev_in_path]; 
                    j0_target_tree_root = j_prev_in_path; 
                } while (j0_target_tree_root != 0); 
            }
            return -v[0]; 
        }
    }

    private bool IsGoal(State s) 
    {
        if (s.Boxes.Length != _boxCount) { 
            return false; 
        }

        int boxesOnTarget = 0;
        foreach (var boxPos in s.Boxes)
        {
            if (_targetsSet.Contains(boxPos))
            {
                boxesOnTarget++;
            }
        }
        bool result = (boxesOnTarget == _boxCount); 
        return result; 
    }
    
    private bool IsDeadlock(int pushedBoxNewPos, int[] currentBoxesState)
    {
        if (_targetsSet.Contains(pushedBoxNewPos)) return false;

        if (_deadZone.Contains(pushedBoxNewPos))
        {
            return true;
        }

        return false; 
    }
    
    private void PrintBoard(int px, int py, int[] currentBoxArray, string title = "Board:")
    {
        Console.Error.WriteLine(title);
        var boxSetToPrint = new HashSet<int>(currentBoxArray);
        var playerPosLinear = Linear(px, py);

        for (var y_coord = 0; y_coord < _h; ++y_coord)
        {
            var line = "";
            for (var x_coord = 0; x_coord < _w; ++x_coord)
            {
                var idx = Linear(x_coord, y_coord);
                if (_walls.Contains(idx)) line += "#";
                else if (playerPosLinear == idx) line += "P";
                else if (boxSetToPrint.Contains(idx) && _targetsSet.Contains(idx)) line += "$"; 
                else if (boxSetToPrint.Contains(idx)) line += "B"; 
                else if (_targetsSet.Contains(idx)) line += "*"; 
                else line += ".";
            }
            Console.Error.WriteLine(line);
        }
        Console.Error.WriteLine($"Player at ({px},{py}). Boxes at: {string.Join(", ", currentBoxArray.Select(b => $"({b % _w},{b / _w})"))}");
        Console.Error.WriteLine($"Targets at: {string.Join(", ", _targetsList.Select(t => $"({t % _w},{t / _w})"))}");
    }

    private char FallbackMove(int px, int py, int[] boxArr)
    {
        for (var i = 0; i < 4; ++i)
        {
            var d = (_fallbackIdx + i) % 4; 
            var nx = px + Dx[d];
            var ny = py + Dy[d];
            
            if (nx < 0 || nx >= _w || ny < 0 || ny >= _h) continue; 
            
            var nposLinear = Linear(nx, ny);
            if (_walls.Contains(nposLinear)) continue; 
            
            bool blockedByBox = false;
            foreach(var boxLPos in boxArr) if(boxLPos == nposLinear) blockedByBox = true;
            if (blockedByBox) continue; 

            _fallbackIdx = (_fallbackIdx + i + 1) % 4; 
            return Dc[d];
        }
        return Dc[0]; 
    }

    private class State : IEquatable<State>
    {
        public int PX { get; } 
        public int PY { get; } 
        public int[] Boxes { get; } 

        private readonly int _hashCode;

        public State(int px, int py, int[] boxes) 
        {
            PX = px;
            PY = py;
            Boxes = boxes; 

            int hc = 17; 
            hc = hc * 23 + PX.GetHashCode();
            hc = hc * 23 + PY.GetHashCode();
            if (Boxes != null) 
            {
                foreach (var boxPos in Boxes)
                {
                    hc = hc * 23 + boxPos.GetHashCode();
                }
            }
            _hashCode = hc;
        }

        public bool Equals(State other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            if (_hashCode != other._hashCode) return false; 
            
            if (PX != other.PX || PY != other.PY) return false;
            if (Boxes.Length != other.Boxes.Length) return false; 
            for (var i = 0; i < Boxes.Length; ++i)
                if (Boxes[i] != other.Boxes[i]) return false; 
            return true;
        }

        public override bool Equals(object obj) => Equals(obj as State);
        public override int GetHashCode() => _hashCode;
    }

    private class MacroNode : IComparable<MacroNode>
    {
        public State State { get; }
        public MacroNode Parent { get; }
        public List<char> PusherMoves { get; } 
        public int Cost { get; } 
        public int F { get; }    

        public MacroNode(State s, MacroNode p, List<char> moves, int cost, int fValue)
        {
            State = s; Parent = p; PusherMoves = moves; Cost = cost; F = fValue;
        }
        
        public int CompareTo(MacroNode other) 
        {
            if (F != other.F) return F.CompareTo(other.F);
            return (other.F - other.Cost).CompareTo(this.F - this.Cost); 
        }
    }

    private class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> _heap = new List<T>();
        public int Count => _heap.Count;

        public void Enqueue(T item)
        {
            _heap.Add(item);
            int i = _heap.Count - 1; 
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_heap[i].CompareTo(_heap[parent]) >= 0) break; 
                (_heap[i], _heap[parent]) = (_heap[parent], _heap[i]); 
                i = parent;
            }
        }

        public T Dequeue()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Priority queue is empty.");
            
            T top = _heap[0]; 
            _heap[0] = _heap[_heap.Count - 1]; 
            _heap.RemoveAt(_heap.Count - 1);    

            int i = 0; 
            while (true)
            {
                int leftChildIdx = 2 * i + 1;
                int rightChildIdx = 2 * i + 2;
                int smallestChildIdx = i; 

                if (leftChildIdx < _heap.Count && _heap[leftChildIdx].CompareTo(_heap[smallestChildIdx]) < 0)
                    smallestChildIdx = leftChildIdx;
                
                if (rightChildIdx < _heap.Count && _heap[rightChildIdx].CompareTo(_heap[smallestChildIdx]) < 0)
                    smallestChildIdx = rightChildIdx;
                
                if (smallestChildIdx == i) break; 

                (_heap[i], _heap[smallestChildIdx]) = (_heap[smallestChildIdx], _heap[i]);
                i = smallestChildIdx; 
            }
            return top;
        }
    }
}