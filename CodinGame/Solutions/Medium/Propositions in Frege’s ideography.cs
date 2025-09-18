using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int rowCount = int.Parse(Console.ReadLine());
        var grid = new List<string>(rowCount);
        for (int i = 0; i < rowCount; i++)
            grid.Add(Console.ReadLine());

        var parser = new ExpressionParser(grid);
        var formula = parser.Parse();

        var variables = new List<char>(parser.Variables);
        variables.Sort();

        int varCount = variables.Count;
        int totalCombos = 1 << varCount;
        var counterExamples = new List<bool[]>();

        for (int mask = 0; mask < totalCombos; mask++)
        {
            var assignment = new bool[varCount];
            var mapping = new Dictionary<char, bool>(varCount);

            for (int i = 0; i < varCount; i++)
            {
                bool value = ((mask >> (varCount - 1 - i)) & 1) == 1;
                assignment[i] = value;
                mapping[variables[i]] = value;
            }

            if (!formula.Evaluate(mapping))
                counterExamples.Add(assignment);
        }

        if (counterExamples.Count > 0)
        {
            foreach (var assignment in counterExamples)
            {
                var output = new List<string>();
                for (int i = 0; i < varCount; i++)
                {
                    output.Add($"{variables[i]} {assignment[i]}");
                }
                Console.WriteLine(string.Join(" ", output));
            }
        }
        else
        {
            Console.WriteLine("TAUTOLOGY");
        }
    }

    class ExpressionParser
    {
        private readonly int rowCount;
        private readonly int colCount;
        private readonly List<string> grid;
        private readonly HashSet<char> variableSet = new HashSet<char>();

        public IEnumerable<char> Variables => variableSet;

        public ExpressionParser(List<string> grid)
        {
            this.grid = grid;
            this.rowCount = grid.Count;
            this.colCount = grid[rowCount - 1].Length;
        }

        public Expr Parse() => ParseAt(0, 0);

        private Expr ParseAt(int row, int col)
        {
            while (true)
            {
                if (col == colCount - 2)
                {
                    char varName = grid[row][col + 1];
                    variableSet.Add(varName);
                    return new VariableExpression(varName);
                }

                char c = grid[row][col];
                switch (c)
                {
                    case '|':
                        row++;
                        continue;
                    case '\'':
                        col++;
                        continue;
                    case '+':
                        return new NotExpression(ParseAt(row, col + 1));
                }

                if (row + 1 < rowCount && (grid[row + 1][col] == '|' || grid[row + 1][col] == '\''))
                {
                    Expr leftNot = new NotExpression(ParseAt(row + 1, col));
                    Expr rightExpr = ParseAt(row, col + 1);
                    return new OrExpression(leftNot, rightExpr);
                }

                col++;
            }
        }

        public abstract class Expr
        {
            public abstract bool Evaluate(Dictionary<char, bool> vars);
        }

        class VariableExpression : Expr
        {
            private readonly char name;
            public VariableExpression(char name) => this.name = name;
            public override bool Evaluate(Dictionary<char, bool> vars) => vars[name];
        }

        class NotExpression : Expr
        {
            private readonly Expr operand;
            public NotExpression(Expr operand) => this.operand = operand;
            public override bool Evaluate(Dictionary<char, bool> vars) => !operand.Evaluate(vars);
        }

        class OrExpression : Expr
        {
            private readonly Expr left;
            private readonly Expr right;
            public OrExpression(Expr left, Expr right) => (this.left, this.right) = (left, right);
            public override bool Evaluate(Dictionary<char, bool> vars) => left.Evaluate(vars) || right.Evaluate(vars);
        }
    }
}