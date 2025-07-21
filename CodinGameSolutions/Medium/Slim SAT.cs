using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var formula = Console.ReadLine();
        int n = int.Parse(Console.ReadLine());
        var assignments = new Dictionary<char,bool>();
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            assignments[parts[0][0]] = parts[1] == "TRUE";
        }

        var solver = new FormulaSolver(formula, assignments);
        Console.WriteLine(solver.IsSatisfiable() ? "Satisfiable" : "Unsatisfiable");
    }

    class FormulaSolver
    {
        private readonly string[] _tokens;
        private int _pos;
        private readonly Dictionary<char,bool> _baseVars;
        private readonly Node _root;

        public FormulaSolver(string formula, Dictionary<char,bool> assignments)
        {
            _tokens = formula.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            _baseVars = new Dictionary<char,bool>(assignments);
            _pos = 0;
            _root = ParseExpression();
        }

        public bool IsSatisfiable()
        {
            foreach (var xVal in new[] { false, true })
            {
                _baseVars['X'] = xVal;
                if (_root.Evaluate(_baseVars))
                    return true;
            }
            return false;
        }

        private Node ParseExpression()
        {
            if (_tokens[_pos] == "(")
            {
                _pos++;
                var left = ParseExpression();
                var op = _tokens[_pos++];
                var right = ParseExpression();
                if (_tokens[_pos] != ")")
                    throw new Exception("Missing closing parenthesis");
                _pos++;
                return new OpNode(op, left, right);
            }
            var tok = _tokens[_pos++];
            if (tok.Length == 1 && char.IsLetter(tok[0]))
                return new VarNode(tok[0]);
            throw new Exception($"Unexpected token '{tok}'");
        }

        private abstract class Node
        {
            public abstract bool Evaluate(Dictionary<char,bool> vars);
        }

        private class VarNode : Node
        {
            private readonly char _name;
            public VarNode(char name) => _name = name;
            public override bool Evaluate(Dictionary<char,bool> vars) => vars[_name];
        }

        private class OpNode : Node
        {
            private readonly string _op;
            private readonly Node _left, _right;
            public OpNode(string op, Node left, Node right)
            {
                _op = op; _left = left; _right = right;
            }
            public override bool Evaluate(Dictionary<char,bool> vars)
            {
                bool a = _left.Evaluate(vars), b = _right.Evaluate(vars);
                return _op switch
                {
                    "AND" => a && b,
                    "OR"  => a || b,
                    "XOR" => a ^ b,
                    _     => throw new InvalidOperationException($"Unknown op {_op}")
                };
            }
        }
    }
}
