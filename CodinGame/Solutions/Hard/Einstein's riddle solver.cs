using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        var nbCharacteristics = int.Parse(inputs[0]);
        var nbPeople = int.Parse(inputs[1]);
        var characteristics = new List<List<string>>();
        for (int i = 0; i < nbCharacteristics; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            characteristics.Add(inputs.ToList());
        }
        var nbLinks = int.Parse(Console.ReadLine());
        var links = new List<string>();
        for (int i = 0; i < nbLinks; i++)
        {
            var link = Console.ReadLine();
            links.Add(link);
        }

        var riddle = new Riddle(nbCharacteristics, nbPeople, characteristics, links);
        riddle.Solve();
        var solution = riddle.GetFormattedSolution();
        
        foreach(var line in solution)
        {
            Console.WriteLine(line);
        }
    }
}

class Riddle
{
    private readonly int _nbCharacteristics;
    private readonly int _nbPeople;
    private readonly int _totalChars;
    private readonly List<string> _allChars;
    private readonly Dictionary<string, int> _charToIdx;
    private readonly int[] _idxToCat;
    private readonly List<List<int>> _categoryChars;
    private readonly bool[,] _possible;
    private readonly Queue<(int, int)> _propagationQueue;
    private readonly bool[,] _enqueuedPairs;

    public Riddle(int nbCharacteristics, int nbPeople, List<List<string>> characteristics, List<string> links)
    {
        _nbCharacteristics = nbCharacteristics;
        _nbPeople = nbPeople;
        _totalChars = _nbCharacteristics * _nbPeople;

        _allChars = new List<string>();
        _charToIdx = new Dictionary<string, int>();
        _idxToCat = new int[_totalChars];
        _categoryChars = new List<List<int>>();

        var charCounter = 0;
        for (var i = 0; i < _nbCharacteristics; i++)
        {
            var catList = new List<int>();
            foreach (var characteristic in characteristics[i])
            {
                _allChars.Add(characteristic);
                _charToIdx[characteristic] = charCounter;
                _idxToCat[charCounter] = i;
                catList.Add(charCounter);
                charCounter++;
            }
            _categoryChars.Add(catList);
        }

        _possible = new bool[_totalChars, _totalChars];
        for (var i = 0; i < _totalChars; i++)
        {
            for (var j = 0; j < _totalChars; j++)
            {
                _possible[i, j] = _idxToCat[i] != _idxToCat[j];
            }
            _possible[i, i] = true;
        }

        _propagationQueue = new Queue<(int, int)>();
        _enqueuedPairs = new bool[_totalChars, _totalChars];

        ParseAndApplyLinks(links);
    }

    private void ParseAndApplyLinks(List<string> links)
    {
        var positiveLinks = new List<(int, int)>();

        foreach (var link in links)
        {
            var parts = link.Split(' ');
            var char1 = parts[0];
            var op = parts[1];
            var char2 = parts[2];
            var idx1 = _charToIdx[char1];
            var idx2 = _charToIdx[char2];

            if (op == "!")
            {
                if (_possible[idx1, idx2])
                {
                    _possible[idx1, idx2] = false;
                    _possible[idx2, idx1] = false;
                    OnPossibilityRemoved(idx1, idx2);
                }
            }
            else
            {
                positiveLinks.Add((idx1, idx2));
            }
        }
        
        foreach(var (u,v) in positiveLinks)
        {
            EnqueuePair(u, v);
        }
    }
    
    private void EnqueuePair(int u, int v)
    {
        if (u == v) return;
        var first = Math.Min(u, v);
        var second = Math.Max(u, v);
        if (!_enqueuedPairs[first, second])
        {
            _propagationQueue.Enqueue((first, second));
            _enqueuedPairs[first, second] = true;
        }
    }

    public void Solve()
    {
        for (var i = 0; i < _totalChars; i++)
        {
            for (var c = 0; c < _nbCharacteristics; c++)
            {
                if (_idxToCat[i] != c)
                {
                    CheckForUniquePartner(i, c);
                }
            }
        }

        while (_propagationQueue.Count > 0)
        {
            var (u, v) = _propagationQueue.Dequeue();

            for (var k = 0; k < _totalChars; k++)
            {
                if (_possible[u, k] && !_possible[v, k])
                {
                    _possible[u, k] = false;
                    _possible[k, u] = false;
                    OnPossibilityRemoved(u, k);
                }
                if (_possible[v, k] && !_possible[u, k])
                {
                    _possible[v, k] = false;
                    _possible[k, v] = false;
                    OnPossibilityRemoved(v, k);
                }
            }
        }
    }

    private void OnPossibilityRemoved(int char1, int char2)
    {
        var cat1 = _idxToCat[char1];
        var cat2 = _idxToCat[char2];
        CheckForUniquePartner(char1, cat2);
        CheckForUniquePartner(char2, cat1);
    }

    private void CheckForUniquePartner(int c1, int targetCat)
    {
        var soloPartner = -1;
        var count = 0;
        foreach (var c2 in _categoryChars[targetCat])
        {
            if (_possible[c1, c2])
            {
                count++;
                soloPartner = c2;
            }
        }
        if (count == 1)
        {
            EnqueuePair(c1, soloPartner);
        }
    }

    public List<string> GetFormattedSolution()
    {
        var solutions = new List<List<int>>();
        var used = new bool[_totalChars];
        for (var i = 0; i < _totalChars; i++)
        {
            if (used[i]) continue;

            var group = new List<int>();
            for (var j = 0; j < _totalChars; j++)
            {
                if (_possible[i, j])
                {
                    group.Add(j);
                    used[j] = true;
                }
            }
            solutions.Add(group);
        }

        foreach (var group in solutions)
        {
            group.Sort((a, b) => _idxToCat[a].CompareTo(_idxToCat[b]));
        }

        solutions.Sort((a, b) => string.Compare(_allChars[a[0]], _allChars[b[0]], StringComparison.Ordinal));

        var outputLines = new List<string>();
        for (var row = 0; row < _nbCharacteristics; row++)
        {
            var line = solutions.Select(group => _allChars[group[row]]);
            outputLines.Add(string.Join(" ", line));
        }

        return outputLines;
    }
}
