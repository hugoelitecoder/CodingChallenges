using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var protoDims = Console.ReadLine().Split(' ');
        var protoWidth = int.Parse(protoDims[0]);
        var protoHeight = int.Parse(protoDims[1]);
        var prototype = new char[protoHeight][];
        for (var i = 0; i < protoHeight; i++)
        {
            prototype[i] = Console.ReadLine().ToCharArray();
        }

        var inputDims = Console.ReadLine().Split(' ');
        var inputWidth = int.Parse(inputDims[0]);
        var inputHeight = int.Parse(inputDims[1]);
        var input = new char[inputHeight][];
        for (var i = 0; i < inputHeight; i++)
        {
            input[i] = Console.ReadLine().ToCharArray();
        }

        var wf = new WaveFunction(prototype, input);
        while (wf.Constrain()) { }

        Console.Write(wf.ToText());
    }
}

class WaveFunction
{
    private readonly List<char[,]> _allCards;
    private readonly HashSet<char> _allLetters;
    private readonly Dictionary<(int, int), List<char[,]>> _cards;
    private readonly Dictionary<(int, int), List<char>> _letters;
    private readonly int _height;
    private readonly int _width;

    public WaveFunction(char[][] prototype, char[][] image)
    {
        _allCards = ExtractCards(prototype).ToList();
        _allLetters = new HashSet<char>(prototype.SelectMany(row => row));
        _height = image.Length;
        _width = image[0].Length;
        _cards = new Dictionary<(int, int), List<char[,]>>();
        _letters = new Dictionary<(int, int), List<char>>();
        Populate(image);
    }

    private IEnumerable<char[,]> ExtractCards(char[][] image)
    {
        for (var h = 0; h <= image.Length - 3; h++)
        {
            for (var w = 0; w <= image[0].Length - 3; w++)
            {
                var card = new char[3, 3];
                for (var dh = 0; dh < 3; dh++)
                {
                    for (var dw = 0; dw < 3; dw++)
                    {
                        card[dh, dw] = image[h + dh][w + dw];
                    }
                }
                yield return card;
            }
        }
    }

    private void Populate(char[][] image)
    {
        for (var h = 0; h < _height; h++)
        {
            for (var w = 0; w < _width; w++)
            {
                var key = (h, w);
                if (image[h][w] == '?')
                {
                    _cards[key] = new List<char[,]>(_allCards);
                    _letters[key] = new List<char>(_allLetters);
                }
                else
                {
                    _cards[key] = new List<char[,]>(_allCards);
                    _letters[key] = new List<char> { image[h][w] };
                }
            }
        }
    }

    public bool Constrain()
    {
        var modified = false;
        for (var h = 1; h < _height - 1; h++)
        {
            for (var w = 1; w < _width - 1; w++)
            {
                modified |= ConstrainCards(h, w);
            }
        }
        return modified;
    }

    private bool ConstrainCards(int h, int w)
    {
        var key = (h, w);
        var validCards = new List<char[,]>(_cards[key]);
        for (var dh = -1; dh <= 1; dh++)
        {
            for (var dw = -1; dw <= 1; dw++)
            {
                var neighborKey = (h + dh, w + dw);
                if (!_letters.ContainsKey(neighborKey)) continue;
                var letters = _letters[neighborKey];
                validCards = validCards.Where(card => letters.Contains(card[dh + 1, dw + 1])).ToList();
            }
        }

        if (validCards.Count == _cards[key].Count) return false;

        _cards[key] = validCards;

        for (var dh = -1; dh <= 1; dh++)
        {
            for (var dw = -1; dw <= 1; dw++)
            {
                var neighborKey = (h + dh, w + dw);
                if (!_letters.ContainsKey(neighborKey)) continue;
                var letters = new HashSet<char>(validCards.Select(card => card[dh + 1, dw + 1]));
                if (letters.Count > 0 && letters.Count < _letters[neighborKey].Count)
                {
                    _letters[neighborKey] = letters.ToList();
                }
            }
        }

        return true;
    }

    public string ToText()
    {
        var sb = new System.Text.StringBuilder();
        for (var h = 0; h < _height; h++)
        {
            for (var w = 0; w < _width; w++)
            {
                var key = (h, w);
                var letters = _letters[key];
                sb.Append(letters.Count == 1 ? letters[0] : '?');
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
