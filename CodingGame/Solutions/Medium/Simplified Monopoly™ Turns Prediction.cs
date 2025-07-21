using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int P = int.Parse(Console.ReadLine());
        var entries = Enumerable.Range(0, P)
            .Select(_ => Console.ReadLine())
            .ToList();

        int D = int.Parse(Console.ReadLine());
        var rolls = Enumerable.Range(0, D)
            .Select(_ => Console.ReadLine().Split().Select(int.Parse).ToArray())
            .Select(a => (d1: a[0], d2: a[1]))
            .ToList();

        Enumerable.Range(0, 40).ToList().ForEach(_ => Console.ReadLine());

        var game = new Monopoly(entries);
        game.Play(rolls);

        game.Players
            .ForEach(p => Console.WriteLine($"{p._name} {p._cell}"));
    }
}

public class Monopoly
{
    const int NumPositions = 40;
    const int GoToJailCell = 30;
    const int JailCell = 10;
    const int MaxDoubles = 2;
    private readonly Player _head;
    private readonly int _count;

    public Monopoly(List<string> entries)
    {
        var nodes = entries.Select(e => new Player(e)).ToList();
        _count = nodes.Count;
        for (int i = 0; i < _count; i++)
        {
            nodes[i]._next = nodes[(i + 1) % _count];
        }
        _head = nodes[0];
    }

    public class Player
    {
        public string _name;
        public int _cell;
        public int _doubleCount;
        public bool _inJail;
        public int _prisonAttempts;
        public Player _next;
       

        public Player(string entry)
        {
            var parts = entry.Split();
            _name = parts[0];
            _cell = int.Parse(parts[1]);
            _doubleCount = 0;
            _inJail = false;
            _prisonAttempts = 0;
        }

        public Player Move(int d1, int d2)
        {
            Player nextPlayer = this._next;
            bool isDouble = d1 == d2;

            if (_inJail)
            {
                if (isDouble || _prisonAttempts == MaxDoubles)
                {
                    _cell = (_cell + d1 + d2) % NumPositions;
                    _inJail = false;
                    _doubleCount = 0;
                    _prisonAttempts = 0;
                }
                else
                {
                    _prisonAttempts++;
                }
            }
            else
            {
                _cell = (_cell + d1 + d2) % NumPositions;
                if (_cell == GoToJailCell)
                {
                    _cell = JailCell;
                    _inJail = true;
                    _doubleCount = 0;
                    _prisonAttempts = 0;
                }
                else if (isDouble)
                {
                    if (_doubleCount == MaxDoubles)
                    {
                        _cell = JailCell;
                        _inJail = true;
                        _doubleCount = 0;
                        _prisonAttempts = 0;
                    }
                    else
                    {
                        _doubleCount++;
                        nextPlayer = this;
                    }
                }
                else
                {
                    _doubleCount = 0;
                }
            }

            return nextPlayer;
        }

        public override string ToString() => _name + " " + _cell;
    }

    public List<Player> Players
    {
        get
        {
            var list = new List<Player>(_count);
            var p = _head;
            for (int i = 0; i < _count; i++)
            {
                list.Add(p);
                p = p._next;
            }
            return list;
        }
    }

    public void Play(List<(int d1, int d2)> rolls)
    {
        var current = _head;
        foreach (var (d1, d2) in rolls)
        {
            current = current.Move(d1, d2);
        }
    }
}