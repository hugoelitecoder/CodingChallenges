using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int L = int.Parse(inputs[0]);
        int N = int.Parse(inputs[1]);
        
        var queries = new List<string>();
        for (int i = 0; i < N; i++)
        {
            queries.Add(Console.ReadLine());
        }

        var solver = new QuerySolver(L);
        solver.ProcessQueries(queries);

        int finalS = solver.GetFinalS(); 
        Console.WriteLine(solver.ToBinaryString(finalS));
        solver.PrintResults();
    }
}

class QuerySolver
{
    private int _L;
    private char _nextChar;
    private bool _sIsFixed;
    private HashSet<int> _possibleS;
    private Dictionary<int, char> _mappings;
    private Dictionary<char, int> _charToFirstQuery;
    private List<char> _results;

    public QuerySolver(int L)
    {
        this._L = L;
        _nextChar = 'A';
        _sIsFixed = false;
        _possibleS = new HashSet<int>(Enumerable.Range(1, (1 << L) - 1));
        _mappings = new Dictionary<int, char>();
        _charToFirstQuery = new Dictionary<char, int>();
        _results = new List<char>();
    }

    public void ProcessQueries(List<string> queries)
    {
        foreach (var queryStr in queries)
        {
            int queryInt = Convert.ToInt32(queryStr, 2);
            if (_mappings.ContainsKey(queryInt))
            {
                _results.Add(_mappings[queryInt]);
                continue;
            }

            if (_sIsFixed)
            {
                ProcessFixedSecret(queryInt);
                continue;
            }

            var forbiddenS = GetForbiddenSecrets(queryInt);
            var remainingS = new HashSet<int>(_possibleS);
            remainingS.ExceptWith(forbiddenS);

            if (remainingS.Any())
            {
                _possibleS = remainingS;
                AddMapping(queryInt, _nextChar);
                _nextChar++;
            }
            else
            {
                HandleUnfixedSecret(queryInt);
            }
        }
    }

    public int GetFinalS()
    {
        return _possibleS.Max();
    }

    public string ToBinaryString(int value)
    {
        return Convert.ToString(value, 2).PadLeft(_L, '0');
    }

    public void PrintResults()
    {
        foreach (var r in _results)
        {
            Console.WriteLine(r);
        }
    }

    private void ProcessFixedSecret(int queryInt)
    {
        int theSecret = _possibleS.First();
        int pairInt = queryInt ^ theSecret;
        char resultChar;

        if (_mappings.ContainsKey(pairInt))
        {
            resultChar = _mappings[pairInt];
        }
        else
        {
            resultChar = _nextChar;
            _charToFirstQuery[resultChar] = queryInt;
            _nextChar++;
        }
        _mappings[queryInt] = resultChar;
        _results.Add(resultChar);
    }

    private HashSet<int> GetForbiddenSecrets(int queryInt)
    {
        var forbiddenS = new HashSet<int>();
        foreach (var prevQuery in _mappings.Keys)
        {
            forbiddenS.Add(queryInt ^ prevQuery);
        }
        return forbiddenS;
    }

    private void HandleUnfixedSecret(int queryInt)
    {
        for (char checkChar = 'A'; checkChar < 'Z'; checkChar++)
        {
            if (!_charToFirstQuery.ContainsKey(checkChar)) continue;
            int prevQueryInt = _charToFirstQuery[checkChar];
            int potentialS = queryInt ^ prevQueryInt;

            if (_possibleS.Contains(potentialS))
            {
                char resultChar = checkChar;
                _sIsFixed = true;
                _possibleS.Clear();
                _possibleS.Add(potentialS);
                _mappings[queryInt] = resultChar;
                _results.Add(resultChar);
                break;
            }
        }
    }

    private void AddMapping(int queryInt, char resultChar)
    {
        _mappings[queryInt] = resultChar;
        _charToFirstQuery[resultChar] = queryInt;
        _results.Add(resultChar);
    }
}
