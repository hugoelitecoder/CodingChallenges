using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var t = int.Parse(Console.ReadLine());
        var game = new Hanoi(n);
        game.Simulate(t);
        game.Print();
        Console.WriteLine(game.Turns);
    }
}

class Hanoi
{
    private readonly int _n;
    private readonly int _s;
    private readonly List<List<int>> _stacks;
    private int _turns;
    private int _toPrint;
    private List<List<int>> _stateAtTurn;

    public int Turns => _turns;

    public Hanoi(int n)
    {
        _n = n;
        _s = n % 2 == 0 ? 1 : 2;
        _stacks = new List<List<int>> { new List<int>(), new List<int>(), new List<int>() };
        for (var i = 1; i <= n; i++) _stacks[0].Add(i);
    }

    public void Simulate(int t)
    {
        _turns = 0;
        while (true)
        {
            _turns++;
            if (_turns % 2 == 1)
            {
                for (var i = 0; i < 3; i++)
                {
                    if (_stacks[i].Count > 0 && _stacks[i][0] == 1)
                    {
                        _stacks[i].RemoveAt(0);
                        var idx = (i + _s) % 3;
                        _stacks[idx].Insert(0, 1);
                        break;
                    }
                }
            }
            else
            {
                int a = -1, b = -1;
                for (var i = 0; i < 3; i++)
                {
                    if (_stacks[i].Count > 0 && _stacks[i][0] == 1)
                    {
                        a = (i + 1) % 3;
                        b = (i + 2) % 3;
                        break;
                    }
                }
                if (_stacks[a].Count == 0 || (_stacks[b].Count > 0 && _stacks[a][0] > _stacks[b][0]))
                {
                    var tmp = a; a = b; b = tmp;
                }
                _stacks[b].Insert(0, _stacks[a][0]);
                _stacks[a].RemoveAt(0);
            }
            if (_turns == t)
                _stateAtTurn = CopyState();
            if (_stacks[2].Count == _n)
                break;
        }
        if (_stateAtTurn == null) _stateAtTurn = CopyState();
    }

    private List<List<int>> CopyState()
    {
        var copy = new List<List<int>>();
        foreach (var peg in _stacks)
            copy.Add(new List<int>(peg));
        return copy;
    }

    public void Print()
    {
        var stacks = _stateAtTurn;
        for (var i = 0; i < _n; i++)
        {
            var line = "";
            foreach (var peg in stacks)
            {
                if (peg.Count < _n - i)
                {
                    line += new string(' ', _n) + "|" + new string(' ', _n);
                }
                else
                {
                    var d = peg[i - _n + peg.Count];
                    line += new string(' ', _n - d) + new string('#', 2 * d + 1) + new string(' ', _n - d);
                }
                line += " ";
            }
            Console.WriteLine(line.TrimEnd());
        }
    }
}
