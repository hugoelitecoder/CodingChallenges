using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var S = int.Parse(inputs[0]);
        var T = int.Parse(inputs[1]);
        var X = int.Parse(inputs[2]);
        var START = Console.ReadLine();
        var N = int.Parse(Console.ReadLine());
        
        var rules = new Dictionary<string, Action[]>();
        for (var i = 0; i < N; i++)
        {
            var stateLine = Console.ReadLine();
            var parts = stateLine.Split(new[] { ':' }, 2);
            var stateName = parts[0];
            var actionStrings = parts[1].Trim().Split(';');
            var actions = new Action[S];
            for (var j = 0; j < S; j++)
            {
                var actionParts = actionStrings[j].Trim().Split(' ');
                var write = int.Parse(actionParts[0]);
                var move = actionParts[1][0];
                var next = actionParts[2];
                actions[j] = new Action(write, move, next);
            }
            rules[stateName] = actions;
        }

        var machine = new TuringMachine(T, X, START, rules);
        machine.Run();
        
        Console.WriteLine(machine.Steps);
        Console.WriteLine(machine.HeadPosition);
        Console.WriteLine(machine.TapeContents);
    }
}

internal record Action(int WriteSymbol, char MoveDirection, string NextState);

internal class TuringMachine
{
    public int Steps { get; private set; }
    public int HeadPosition => _head;
    public string TapeContents => string.Join("", _tape);

    private readonly int[] _tape;
    private readonly Dictionary<string, Action[]> _rules;
    private readonly int _tapeSize;
    private string _currentState;
    private int _head;

    public TuringMachine(int tapeSize, int initialHeadPos, string startState, Dictionary<string, Action[]> rules)
    {
        _tape = new int[tapeSize];
        _tapeSize = tapeSize;
        _head = initialHeadPos;
        _currentState = startState;
        _rules = rules;
        Steps = 0;
    }

    public void Run()
    {
        while (_currentState != "HALT" && _head >= 0 && _head < _tapeSize)
        {
            var symbol = _tape[_head];
            var action = _rules[_currentState][symbol];
            _tape[_head] = action.WriteSymbol;
            if (action.MoveDirection == 'R')
            {
                _head++;
            }
            else
            {
                _head--;
            }
            _currentState = action.NextState;
            Steps++;
        }
    }
}