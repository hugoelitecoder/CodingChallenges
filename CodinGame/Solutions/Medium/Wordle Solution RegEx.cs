using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace WordleRegex
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine());
            var builder = new WordleRegexBuilder();

            for (int i = 0; i < n; i++)
            {
                var parts = Console.ReadLine().Split(' ');
                builder.AddGuess(parts[0], parts[1]);
            }

            Console.WriteLine(builder.BuildPattern());
        }
    }

    public class WordleRegexBuilder
    {
        private readonly List<string> _guesses = new List<string>();
        private readonly List<string> _results = new List<string>();

        public void AddGuess(string guess, string result)
        {
            if (guess.Length != 5 || result.Length != 5)
                throw new ArgumentException("Both guess and result must be 5 characters long.");

            _guesses.Add(guess);
            _results.Add(result);
        }

        public string BuildPattern()
        {
            int n = _guesses.Count;

            for (int i = 0; i < n; i++)
            {
                if (_results[i] == "GGGGG")
                    return $"^{_guesses[i]}$";
            }

            bool[] hasG = new bool[26], hasY = new bool[26], hasBlank = new bool[26];
            char[] green = Enumerable.Repeat('\0', 5).ToArray();
            var immNeg = Enumerable.Range(0, 5)
                                   .Select(_ => new SortedSet<char>())
                                   .ToArray();

            for (int i = 0; i < n; i++)
            {
                string g = _guesses[i];
                string r = _results[i];
                for (int j = 0; j < 5; j++)
                {
                    char c = g[j];
                    int idx = c - 'a';
                    switch (r[j])
                    {
                        case 'G':
                            hasG[idx] = true;
                            green[j] = c;
                            break;
                        case 'Y':
                            hasY[idx] = true;
                            immNeg[j].Add(c);
                            break;
                        case '_':
                            hasBlank[idx] = true;
                            break;
                        default:
                            throw new ArgumentException("Invalid result character: " + r[j]);
                    }
                }
            }

            var posList = Enumerable.Range(0, 26)
                                    .Where(i => hasY[i] && !hasG[i])
                                    .Select(i => (char)('a' + i))
                                    .ToList();

            var exclList = Enumerable.Range(0, 26)
                                     .Where(i => hasBlank[i] && !hasG[i] && !hasY[i])
                                     .Select(i => (char)('a' + i))
                                     .ToList();

            var sb = new StringBuilder();
            sb.Append('^');

            foreach (char c in posList)
                sb.Append($"(?=.*{c})");

            if (exclList.Count == 1)
                sb.Append($"(?!.*{exclList[0]})");
            else if (exclList.Count > 1)
                sb.Append($"(?!.*[{new string(exclList.ToArray())}])");

            for (int pos = 0; pos < 5; pos++)
            {
                if (green[pos] != '\0')
                {
                    sb.Append(green[pos]);
                }
                else if (immNeg[pos].Count == 1)
                {
                    sb.Append($"(?!{immNeg[pos].First()}).");
                }
                else if (immNeg[pos].Count > 1)
                {
                    sb.Append($"(?![{new string(immNeg[pos].ToArray())}]).");
                }
                else
                {
                    sb.Append('.');
                }
            }

            sb.Append('$');
            return sb.ToString();
        }
    }
}