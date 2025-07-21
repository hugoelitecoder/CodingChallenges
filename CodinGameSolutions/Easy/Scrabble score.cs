using System;
using System.Collections.Generic;
using System.Linq;

class WordPlay
{
    public string Word { get; set; }
    public List<(int x, int y)> Positions { get; set; }
    public int Score { get; set; }

    public WordPlay(string word, List<(int, int)> positions)
    {
        Word = word;
        Positions = positions;
        Score = 0;
    }

    public void ComputeScore(Dictionary<char, int> tileScores, List<string> specials, HashSet<(int, int)> newTileSet)
    {
        int total = 0;
        int wordMultiplier = 1;

        for (int i = 0; i < Word.Length; i++)
        {
            var (x, y) = Positions[i];
            char ch = Word[i];
            int tileScore = tileScores.ContainsKey(ch) ? tileScores[ch] : 0;

            if (newTileSet.Contains((x, y)))
            {
                char special = specials[y][x];
                if (special == 'l') tileScore *= 2;
                else if (special == 'L') tileScore *= 3;
                else if (special == 'w') wordMultiplier *= 2;
                else if (special == 'W') wordMultiplier *= 3;
            }

            total += tileScore;
        }

        Score = total * wordMultiplier;
    }
}

class Solution
{
    static Dictionary<char, int> tileScores = new Dictionary<char, int>();

    static void Main()
    {
        int nbTiles = int.Parse(Console.ReadLine());
        for (int i = 0; i < nbTiles; i++)
        {
            var parts = Console.ReadLine().Split();
            tileScores[parts[0][0]] = int.Parse(parts[1]);
        }

        var dims = Console.ReadLine().Split();
        int width = int.Parse(dims[0]);
        int height = int.Parse(dims[1]);

        var specialBoard = new List<string>();
        var oldBoard = new List<string>();
        var newBoard = new List<string>();

        for (int i = 0; i < height; i++) specialBoard.Add(Console.ReadLine());
        for (int i = 0; i < height; i++) oldBoard.Add(Console.ReadLine());
        for (int i = 0; i < height; i++) newBoard.Add(Console.ReadLine());

        var newTiles = GetNewTiles(oldBoard, newBoard);
        var newTileSet = new HashSet<(int, int)>(newTiles.Select(t => (t.x, t.y)));

        var words = GetNewWords(newBoard, newTiles);

        var wordScores = new SortedDictionary<string, WordPlay>();
        int total = 0;

        foreach (var wordPlay in words)
        {
            wordPlay.ComputeScore(tileScores, specialBoard, newTileSet);
            wordScores[wordPlay.Word] = wordPlay;
            total += wordPlay.Score;
        }

        foreach (var kv in wordScores)
            Console.WriteLine($"{kv.Key} {kv.Value.Score}");

        if (newTiles.Count == 7)
        {
            Console.WriteLine("Bonus 50");
            total += 50;
        }

        Console.WriteLine($"Total {total}");
    }

    static List<(int x, int y, char c)> GetNewTiles(List<string> oldBoard, List<string> newBoard)
    {
        var list = new List<(int, int, char)>();
        for (int y = 0; y < oldBoard.Count; y++)
            for (int x = 0; x < oldBoard[0].Length; x++)
                if (oldBoard[y][x] == '.' && newBoard[y][x] != '.')
                    list.Add((x, y, newBoard[y][x]));
        return list;
    }

    static List<WordPlay> GetNewWords(List<string> board, List<(int, int, char)> newTiles)
    {
        var words = new List<WordPlay>();
        var seen = new HashSet<string>();

        foreach (var (x, y, _) in newTiles)
        {
            foreach (var (dx, dy) in new[] { (1, 0), (0, 1) })
            {
                int sx = x, sy = y;
                while (sx - dx >= 0 && sy - dy >= 0 && board[sy - dy][sx - dx] != '.')
                {
                    sx -= dx;
                    sy -= dy;
                }

                string word = "";
                var positions = new List<(int, int)>();
                int cx = sx, cy = sy;

                while (cx >= 0 && cy >= 0 && cx < board[0].Length && cy < board.Count && board[cy][cx] != '.')
                {
                    word += board[cy][cx];
                    positions.Add((cx, cy));
                    cx += dx;
                    cy += dy;
                }

                if (word.Length > 1 && !seen.Contains(word))
                {
                    seen.Add(word);
                    words.Add(new WordPlay(word, positions));
                }
            }
        }

        return words;
    }
}
