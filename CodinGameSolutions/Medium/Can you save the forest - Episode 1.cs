using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class Solution
{
    private static int _maxBurned;
    private static readonly int _N = 10;
    private static int _turnCount = 0;

    public static void Main(string[] args)
    {
        var initLine = Console.ReadLine();
        _maxBurned = int.Parse(initLine.Split(' ')[0]);
        var simulator = new GameSimulator(_N);

        while (true)
        {
            _turnCount++;
            var rootState = ReadState();

            Console.Error.WriteLine($"\n--- Turn {_turnCount}: MaxBurn={_maxBurned} ---");
            var f1 = simulator.BitCount(rootState.fire1);
            var f2 = simulator.BitCount(rootState.fire2);
            var f3 = simulator.BitCount(rootState.fire3);
            var burnt = simulator.BitCount(rootState.burnt);
            Console.Error.WriteLine($"State: Burnt={burnt}, F3={f3}, F2={f2}, F1={f1}");

            if (simulator.IsGameOver(rootState))
            {
                Console.WriteLine("0 0");
                continue;
            }

            var watch = Stopwatch.StartNew();
            var beamSearch = new BeamSearch(simulator, _maxBurned, 100, 64, 85);
            var bestAction = beamSearch.FindBestAction(rootState);
            watch.Stop();

            Console.Error.WriteLine($"Search Time: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine($"{bestAction % _N} {bestAction / _N}");
        }
    }

    private static State ReadState()
    {
        var st = new State();
        for (var y = 0; y < _N; y++)
        {
            var row = Console.ReadLine();
            if (string.IsNullOrEmpty(row)) continue;
            for (var x = 0; x < row.Length && x < _N; x++)
            {
                var c = row[x];
                var b = y * _N + x;
                var mask = BigInteger.One << b;
                switch (c)
                {
                    case '^': st.forest |= mask; break;
                    case '1': st.fire1 |= mask; st.forest |= mask; break;
                    case '2': st.fire2 |= mask; st.forest |= mask; break;
                    case '3': st.fire3 |= mask; st.forest |= mask; break;
                    case '*': st.burnt |= mask; break;
                }
            }
        }
        return st;
    }
}

public class BeamSearch
{
    private readonly GameSimulator _simulator;
    private readonly int _beamWidth;
    private readonly int _maxDepth;
    private readonly int _maxBurned;
    private readonly int _timeLimit;

    public BeamSearch(GameSimulator simulator, int maxBurned, int beamWidth, int maxDepth, int timeLimit)
    {
        _simulator = simulator;
        _maxBurned = maxBurned;
        _beamWidth = beamWidth;
        _maxDepth = maxDepth;
        _timeLimit = timeLimit;
    }

    public int FindBestAction(State initialState)
    {
        var watch = Stopwatch.StartNew();
        var actions = _simulator.GetPossibleActions(initialState);
        if (!actions.Any()) return 0;

        var bestActionFoundSoFar = actions
            .Select(action => new { Action = action, Score = _simulator.Evaluate(_simulator.Step(initialState, action), _maxBurned) })
            .OrderByDescending(x => x.Score)
            .First()
            .Action;

        var rootNode = new SearchNode(initialState, null, -1, 0);
        var beam = new List<SearchNode> { rootNode };
        var completedDepth = 0;

        for (var d = 0; d < _maxDepth; d++)
        {
            if (watch.ElapsedMilliseconds > _timeLimit)
            {
                Console.Error.WriteLine($"Search timed out at depth {d}.");
                break;
            }

            var candidates = new List<SearchNode>();
            var hasExpanded = false;
            foreach (var node in beam)
            {
                if (_simulator.IsGameOver(node.State) || _simulator.IsLost(node.State, _maxBurned))
                {
                    candidates.Add(node);
                    continue;
                }
                hasExpanded = true;
                var currentActions = _simulator.GetPossibleActions(node.State);
                foreach (var action in currentActions)
                {
                    var nextState = _simulator.Step(node.State, action);
                    var newNode = new SearchNode(nextState, node, action, d + 1);
                    newNode.Score = _simulator.Evaluate(nextState, _maxBurned);
                    candidates.Add(newNode);
                }
            }
            if (!hasExpanded || candidates.Count == 0) break;

            candidates.Sort();
            beam = candidates.Take(_beamWidth).ToList();
            completedDepth = d + 1;

            var bestNodeInBeam = beam.FirstOrDefault();
            if (bestNodeInBeam?.Parent != null)
            {
                while (bestNodeInBeam.Parent.Parent != null)
                {
                    bestNodeInBeam = bestNodeInBeam.Parent;
                }
                bestActionFoundSoFar = bestNodeInBeam.Action;
            }
        }
        
        Console.Error.WriteLine($"Beam Search: Completed Depth={completedDepth}, Width={_beamWidth}");
        var bestFinalNode = beam.FirstOrDefault();
        if(bestFinalNode != null) Console.Error.WriteLine($"Best Path Score: {bestFinalNode.Score:F0}");
        return bestActionFoundSoFar;
    }
}

public class SearchNode : IComparable<SearchNode>
{
    public State State { get; }
    public SearchNode Parent { get; }
    public int Action { get; }
    public double Score { get; set; }
    public int Depth { get; }

    public SearchNode(State state, SearchNode parent, int action, int depth)
    {
        State = state;
        Parent = parent;
        Action = action;
        Depth = depth;
    }

    public int CompareTo(SearchNode other)
    {
        return other.Score.CompareTo(this.Score);
    }
}

public struct State
{
    public BigInteger forest, fire1, fire2, fire3, burnt;
}

public class GameSimulator
{
    private readonly int _N;
    private readonly BigInteger _FileA;
    private readonly BigInteger _FileJ;
    private readonly int[] _bitCountTable = new int[256];

    public GameSimulator(int n)
    {
        _N = n;
        _bitCountTable[0] = 0;
        for (var i = 1; i < 256; i++)
        {
            _bitCountTable[i] = (i & 1) + _bitCountTable[i >> 1];
        }

        for (var y = 0; y < _N; y++)
        {
            _FileA |= BigInteger.One << (y * _N + 0);
            _FileJ |= BigInteger.One << (y * _N + (_N - 1));
        }
    }

    public int BitCount(BigInteger n)
    {
        var count = 0;
        foreach (var b in n.ToByteArray())
        {
            count += _bitCountTable[b & 0xFF];
        }
        return count;
    }

    public bool IsGameOver(State state) => (state.fire1 | state.fire2 | state.fire3) == 0;
    public bool IsLost(State state, int maxBurned) => BitCount(state.burnt) > maxBurned;

    public List<int> GetPossibleActions(State s)
    {
        var actions = new List<int>();
        var fires = s.fire1 | s.fire2 | s.fire3;
        for (var b = 0; b < _N * _N; b++)
        {
            if (((fires >> b) & 1) != 0)
            {
                actions.Add(b);
            }
        }
        return actions;
    }

    public State Step(State st, int action)
    {
        var next = st;
        var mask = BigInteger.One << action;
        next.fire1 &= ~mask;
        next.fire2 &= ~mask;
        next.fire3 &= ~mask;

        var toBurn = next.fire3;
        next.burnt |= toBurn;
        next.fire3 = next.fire2;
        next.fire2 = next.fire1;

        var allHazards = next.burnt | next.fire2 | next.fire3;
        var L = (toBurn & ~_FileA) >> 1;
        var R = (toBurn & ~_FileJ) << 1;
        var U = toBurn >> _N;
        var D = toBurn << _N;
        var spread = (L | R | U | D) & next.forest & ~allHazards;
        next.fire1 = spread;

        return next;
    }

    public double Evaluate(State st, int maxBurned)
    {
        var burntCount = BitCount(st.burnt);
        if (IsLost(st, maxBurned))
        {
            return -1e12 - (burntCount * 50000.0);
        }
        if (IsGameOver(st))
        {
            const double WIN_BONUS = 1e9;
            return WIN_BONUS - (burntCount * 50000.0);
        }

        double penalty = 0;
        penalty += burntCount * 50000.0;
        penalty += BitCount(st.fire3) * 30000.0;
        penalty += BitCount(st.fire2) * 1500.0;
        penalty += BitCount(st.fire1) * 100.0;
        
        var validTargets = st.forest & ~st.burnt & ~(st.fire1 | st.fire2 | st.fire3);
        
        var n3 = (st.fire3 >> 1 & ~_FileJ) | (st.fire3 << 1 & ~_FileA) | (st.fire3 >> _N) | (st.fire3 << _N);
        var t3 = n3 & validTargets;
        penalty += BitCount(t3) * 1000.0;
        validTargets &= ~t3;
        
        var n2 = (st.fire2 >> 1 & ~_FileJ) | (st.fire2 << 1 & ~_FileA) | (st.fire2 >> _N) | (st.fire2 << _N);
        var t2 = n2 & validTargets;
        penalty += BitCount(t2) * 200.0;
        validTargets &= ~t2;
        
        var n1 = (st.fire1 >> 1 & ~_FileJ) | (st.fire1 << 1 & ~_FileA) | (st.fire1 >> _N) | (st.fire1 << _N);
        var t1 = n1 & validTargets;
        penalty += BitCount(t1) * 20.0;
        
        return -penalty;
    }
}