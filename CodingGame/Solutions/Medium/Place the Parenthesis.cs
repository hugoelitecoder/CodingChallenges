using System;
using System.Data;

class Program
{
    static readonly DataTable _evaluator = new DataTable();

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            string equation = Console.ReadLine();
            Console.WriteLine(Solve(equation));
        }
    }

    static string Solve(string equation)
    {
        int eqPos = equation.IndexOf('=');
        string left = equation.Substring(0, eqPos);
        string rightExpr = equation.Substring(eqPos + 1);
        double right = Eval(rightExpr);

        if (Eval(left) == right)
            return equation;

        int L = left.Length;
        for (int i = 0; i < L - 1; i++)
        {
            for (int j = i + 2; j <= L; j++)
            {
                string candidate = left.Substring(0, i)
                                  + "("
                                  + left.Substring(i, j - i)
                                  + ")"
                                  + left.Substring(j);
                try
                {
                    if (Eval(candidate) == right)
                    {
                        string rightStr = (right % 1 == 0)
                            ? ((long)right).ToString()
                            : right.ToString();
                        return candidate + "=" + rightStr;
                    }
                }
                catch
                {
                }
            }
        }

        return equation;
    }

    static double Eval(string expr)
    {
        object result = _evaluator.Compute(expr, null);
        return Convert.ToDouble(result);
    }
}
