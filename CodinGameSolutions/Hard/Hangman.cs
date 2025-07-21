using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var word = Console.ReadLine();
        var guessList = Console.ReadLine().Split(' ');
        var game = new Hangman(word);
        foreach (var g in guessList)
        {
            if (!game.IsOver)
                game.Guess(g[0]);
        }
        game.Print();
    }
}

class Hangman
{
    private readonly string _word;
    private readonly HashSet<char> _guessed;
    private readonly char[] _revealed;
    private int _mistakes;
    public bool IsOver => _mistakes >= 6 || AllRevealed();

    public Hangman(string word)
    {
        _word = word;
        _guessed = new HashSet<char>();
        _revealed = new char[_word.Length];
        for (var i = 0; i < _word.Length; i++)
            _revealed[i] = _word[i] == ' ' ? ' ' : '_';
        _mistakes = 0;
    }

    public void Guess(char c)
    {
        if (_guessed.Contains(c))
        {
            _mistakes++;
            return;
        }
        _guessed.Add(c);
        var found = false;
        for (var i = 0; i < _word.Length; i++)
        {
            if (char.ToLower(_word[i]) == c)
            {
                _revealed[i] = _word[i];
                found = true;
            }
        }
        if (!found) _mistakes++;
    }

    private bool AllRevealed()
    {
        for (var i = 0; i < _revealed.Length; i++)
            if (_revealed[i] == '_')
                return false;
        return true;
    }

    public void Print()
    {
        var stages = new[]
        {
            new[]{"+--+","|","|","|\\"},
            new[]{"+--+","|  o","|","|\\"},
            new[]{"+--+","|  o","|  |","|\\"},
            new[]{"+--+","|  o","| /|","|\\"},
            new[]{"+--+","|  o","| /|\\","|\\"},
            new[]{"+--+","|  o","| /|\\","|\\/"},
            new[]{"+--+","|  o","| /|\\","|\\/ \\"}
        };
        var pic = stages[Math.Min(_mistakes, 6)];
        foreach (var line in pic) Console.WriteLine(line);
        if (_mistakes >= 6)
        {
            for (var i = 0; i < _revealed.Length; i++)
                if (_revealed[i] == '_')
                    _revealed[i] = '_';
        }
        Console.WriteLine(new string(_revealed));
    }
}
