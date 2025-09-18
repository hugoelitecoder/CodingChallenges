using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var header = Console.ReadLine().Split();
        var variableCount = int.Parse(header[2]);
        var clauseCount = int.Parse(header[3]);
        var clauses = new List<List<int>>();
        for (var i = 0; i < clauseCount; i++)
        {
            var clause = Console.ReadLine().Split().Select(int.Parse).TakeWhile(x => x != 0).ToList();
            clauses.Add(clause);
        }
        var solver = new HornSATSolver(variableCount, clauseCount, clauses);
        var result = solver.Solve();
        Console.WriteLine(result.IsSatisfiable ? "s SATISFIABLE" : "s UNSATISFIABLE");
        if (result.IsSatisfiable)
        {
            Console.Write("v ");
            foreach (var val in result.Model) Console.Write(val + " ");
            Console.WriteLine("0");
        }
    }

    class HornSATSolver
    {
        private readonly int _v;
        private readonly List<List<int>> _clauses;
        private readonly Dictionary<int, List<int>> _litMap;
        private readonly bool?[] _model;
        private readonly Queue<int> _units;
        private bool _satisfiable;

        public HornSATSolver(int v, int c, List<List<int>> clauses)
        {
            _v = v;
            _clauses = clauses;
            _litMap = new Dictionary<int, List<int>>();
            for (var i = -v; i <= v; i++) _litMap[i] = new List<int>();
            _model = new bool?[v + 1];
            _units = new Queue<int>();
            _satisfiable = true;
            Initialize();
        }

        public Result Solve()
        {
            while (_units.Count > 0 && _satisfiable)
            {
                var lit = _units.Dequeue();
                var var = Math.Abs(lit);
                var val = lit > 0;
                if (_model[var] == null) _model[var] = val;
                else if (_model[var] != val)
                {
                    _satisfiable = false;
                    break;
                }
                foreach (var id in _litMap[lit]) _clauses[id] = null;
                foreach (var id in _litMap[-lit])
                {
                    var clause = _clauses[id];
                    if (clause == null) continue;
                    clause.Remove(-lit);
                    if (clause.Count == 1) _units.Enqueue(clause[0]);
                    else if (clause.Count == 0)
                    {
                        _satisfiable = false;
                        return new Result(false, null);
                    }
                }
            }
            if (!_satisfiable) return new Result(false, null);
            var output = new List<int>();
            for (var i = 1; i <= _v; i++) output.Add((_model[i] ?? false) ? i : -i);
            return new Result(true, output);
        }

        private void Initialize()
        {
            for (var id = 0; id < _clauses.Count; id++)
            {
                var clause = _clauses[id];
                foreach (var lit in clause) _litMap[lit].Add(id);
                if (clause.Count == 1) _units.Enqueue(clause[0]);
                else if (clause.Count == 0) _satisfiable = false;
            }
        }
    }

    class Result
    {
        public bool IsSatisfiable { get; }
        public List<int> Model { get; }

        public Result(bool sat, List<int> model)
        {
            IsSatisfiable = sat;
            Model = model;
        }
    }
}
