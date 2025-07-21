using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int L = int.Parse(Console.ReadLine());
        var sentences = new string[N];
        for (int i = 0; i < N; i++)
            sentences[i] = Console.ReadLine().Trim();

        var solver = new LiarSolver(N, L, sentences);
        var liars = solver.Solve();
        Console.WriteLine(string.Join(" ", liars));
    }
}

public class LiarSolver
{
    private readonly int _N;
    private readonly int _L;
    private readonly string[] _sentences;

    public LiarSolver(int N, int L, string[] sentences)
    {
        _N = N;
        _L = L;
        _sentences = sentences;
    }

    public List<int> Solve()
    {
        var result = new List<int>();
        int maxMask = 1 << _N;
        for (int mask = 0; mask < maxMask; mask++)
        {
            if (CountBits(mask) != _L) continue;
            if (EvaluateConfiguration(mask, out var liars))
            {
                return liars;
            }
        }
        return result;
    }

    private bool EvaluateConfiguration(int mask, out List<int> liars)
    {
        bool[] ctx = new bool[_N];
        liars = new List<int>();
        for (int i = 0; i < _N; i++)
        {
            bool isLiar = ((mask >> (_N - 1 - i)) & 1) == 1;
            ctx[i] = isLiar;
            if (isLiar) liars.Add(i);
        }

        for (int i = 0; i < _N; i++)
        {
            bool contentTrue = ParseSentence(_sentences[i], ctx);
            bool claimTruth = _sentences[i].EndsWith("T");
            if (contentTrue != !_sentences[i].EndsWith("T"))
                return false;
        }
        return true;
    }

    private bool ParseSentence(string sent, bool[] ctx)
    {
        if (sent.Length > 1 && sent[1] == '>')
        {
            int speaker = sent[0] - '0';
            string rest = sent.Substring(2);
            bool inner = ParseSentence(rest, ctx);
            return ctx[speaker] ? !inner : inner;
        }
        else
        {
            int person = sent[0] - '0';
            return ctx[person];
        }
    }

    private int CountBits(int x)
    {
        int count = 0;
        while (x != 0)
        {
            count += x & 1;
            x >>= 1;
        }
        return count;
    }
}
