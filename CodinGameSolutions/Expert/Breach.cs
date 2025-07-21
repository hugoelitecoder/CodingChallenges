using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Solution
{
    static readonly Dictionary<string, char> _colorNameToChar = new Dictionary<string, char>
    {
        { "RED", 'R' }, { "GREEN", 'G' }, { "BLUE", 'B' }, { "YELLOW", 'y' },
        { "ORANGE", 'o' }, { "PINK", 'P' }, { "VIOLET", 'V' }, { "GRAY", 'W' },
        { "LIGHT_RED", 'r' }, { "LIGHT_GREEN", 'g' }, { "LIGHT_BLUE", 'b' },
        { "LIGHT_PINK", 'p' }, { "LIGHT_VIOLET", 'v' }, { "WHITE", 'w' }
    };
    static readonly Dictionary<char, string> _colorCharToName = _colorNameToChar.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    static readonly Dictionary<int, string> _atomicToSymbol = new Dictionary<int, string> {
        {1,"H"},{2,"He"},{3,"Li"},{4,"Be"},{5,"B"},{6,"C"},{7,"N"},{8,"O"},{9,"F"},{10,"Ne"},{11,"Na"},{12,"Mg"},{13,"Al"},{14,"Si"},{15,"P"},{16,"S"},{17,"Cl"},{18,"Ar"},{19,"K"},{20,"Ca"},{21,"Sc"},{22,"Ti"},{23,"V"},{24,"Cr"},{25,"Mn"},{26,"Fe"},{27,"Co"},{28,"Ni"},{29,"Cu"},{30,"Zn"},{31,"Ga"},{32,"Ge"},{33,"As"},{34,"Se"},{35,"Br"},{36,"Kr"},{37,"Rb"},{38,"Sr"},{39,"Y"},{40,"Zr"},{41,"Nb"},{42,"Mo"},{43,"Tc"},{44,"Ru"},{45,"Rh"},{46,"Pd"},{47,"Ag"},{48,"Cd"},{49,"In"},{50,"Sn"},{51,"Sb"},{52,"Te"},{53,"I"},{54,"Xe"},{55,"Cs"},{56,"Ba"},{57,"La"},{58,"Ce"},{59,"Pr"},{60,"Nd"},{61,"Pm"},{62,"Sm"},{63,"Eu"},{64,"Gd"},{65,"Tb"},{66,"Dy"},{67,"Ho"},{68,"Er"},{69,"Tm"},{70,"Yb"},{71,"Lu"},{72,"Hf"},{73,"Ta"},{74,"W"},{75,"Re"},{76,"Os"},{77,"Ir"},{78,"Pt"},{79,"Au"},{80,"Hg"},{81,"Tl"},{82,"Pb"},{83,"Bi"},{84,"Po"},{85,"At"},{86,"Rn"},{87,"Fr"},{88,"Ra"},{89,"Ac"},{90,"Th"},{91,"Pa"},{92,"U"},{93,"Np"},{94,"Pu"},{95,"Am"},{96,"Cm"},{97,"Bk"},{98,"Cf"},{99,"Es"},{100,"Fm"},{101,"Md"},{102,"No"},{103,"Lr"},{104,"Rf"},{105,"Db"},{106,"Sg"},{107,"Bh"},{108,"Hs"},{109,"Mt"},{110,"Ds"},{111,"Rg"},{112,"Cn"},{113,"Nh"},{114,"Fl"},{115,"Mc"},{116,"Lv"},{117,"Ts"},{118,"Og"}
    };
    static readonly Dictionary<string, int> _symbolToAtomic = _atomicToSymbol.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    static readonly List<string[]> _digitsAsciiArt = new List<string[]>
    {
        new[] {" ++++ ", "+    +", "+    +", "+    +", "+    +", " ++++ "},
        new[] {" ++++ ", "+++++ ", "  +++ ", "  +++ ", "  +++ ", " +++++"},
        new[] {" +++++ ", "++   ++", " +  ++ ", "   ++  ", "  ++   ", "+++++++"},
        new[] {" +++++ ", "++   ++", "    ++ ", "++   ++", " +++++ ", "       "},
        new[] {"   ++++ ", " ++   ++", " ++   ++", "++++++++", "      ++", "      ++"},
        new[] {"++++++", "+     ", "++++  ", "    + ", "    + ", "+++++ "},
        new[] {" +++ ", "+    ", "++++ ", "+   +", "+++  ", "     "},
        new[] {"++++++", "    ++", "   ++ ", "  ++  ", " ++   ", " +    "},
        new[] {" ++ ", "+  +", " ++ ", "+  +", " ++ ", "    "},
        new[] {" ++++ ", "+    +", " ++++ ", "    + ", "    + ", ""}
    };

    static readonly List<string> _normalizedDigits = new List<string>
    {
        NormalizeArt(_digitsAsciiArt[0].ToList()),
        NormalizeArt(_digitsAsciiArt[1].ToList()),
        NormalizeArt(_digitsAsciiArt[2].ToList()),
        NormalizeArt(_digitsAsciiArt[3].ToList()),
        NormalizeArt(_digitsAsciiArt[4].ToList()),
        NormalizeArt(_digitsAsciiArt[5].ToList()),
        NormalizeArt(_digitsAsciiArt[6].ToList()),
        NormalizeArt(_digitsAsciiArt[7].ToList()),
        NormalizeArt(_digitsAsciiArt[8].ToList()),
        NormalizeArt(_digitsAsciiArt[9].ToList())
    };

    static int _ssConCounter = 0;

    static void Main(string[] args)
    {
        while (true)
        {
            var numLines = int.Parse(Console.ReadLine());
            var input = new List<string>();
            string lockType = null;
            for (var i = 0; i < numLines; i++)
            {
                var line = Console.ReadLine();
                if (i == 0)
                {
                    var firstLineParts = line.Split(' ');
                    lockType = firstLineParts[0].TrimEnd(':');
                }
                input.Add(line);
            }
            var data = input.GetRange(1, input.Count - 1);

            var answer = SolveLock(lockType, data);
            Console.WriteLine(answer);
        }
    }

    static string SolveLock(string lockType, List<string> data)
    {
        switch (lockType)
        {
            case "ss_f": return SolveSsf(data);
            case "ss_m": return SolveSsm(data);
            case "ss_n": return SolveSsn(data);
            case "ss_asc": return SolveSsasc(data);
            case "ss_con": return SolveSscon(data);
            case "ss_colv": return SolveSscolv(data[0]);
            case "rs_f": return SolveRsf(data);
            case "rs_n": return SolveRsn(data);
            case "rs_colv": return SolveRscolv(data[0]);
            case "gs_m": return SolveGsm(data);
            default: return "UNKNOWN_LOCK";
        }
    }

    static string SolveSsf(List<string> data)
    {
        var line = data[0];
        try
        {
            var lowerChar = line.First(char.IsLower);
            return (lowerChar - 'a').ToString();
        }
        catch (InvalidOperationException) { return "0"; }
    }

    static string SolveSsm(List<string> data)
    {
        var line = data[0];
        if (_symbolToAtomic.TryGetValue(line, out var atomicNumber))
        {
            return atomicNumber.ToString();
        }
        return "UNKNOWN";
    }

    static string SolveSsn(List<string> data)
    {
        var line = data[0];
        var match = Regex.Match(line, @"\[(\d+),.*\]\[(\d+)\]");        
        var start = long.Parse(match.Groups[1].Value);
        var k = int.Parse(match.Groups[2].Value);
        var n = k + 1;

        if (n <= 2) return start.ToString();
        long a = start, b = start;
        for (var i = 3; i <= n; i++)
        {
            var temp = a + b;
            a = b;
            b = temp;
        }
        return b.ToString();
    }

    static string NormalizeArt(List<string> art) => string.Join("\n", art.Select(s => s.Trim()));

    static void DfsContour(int r, int c, List<string> art, bool[,] visited, List<Tuple<int, int>> contour)
    {
        if (r < 0 || r >= art.Count || c < 0 || c >= art[r].Length || visited[r, c] || art[r][c] != '+') return;
        visited[r, c] = true;
        contour.Add(Tuple.Create(r, c));
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
                if (dr != 0 || dc != 0)
                    DfsContour(r + dr, c + dc, art, visited, contour);
    }

    static string SolveSsasc(List<string> data)
    {
        var maxHeight = data.Count;
        var maxWidth = data.Max(l => l.Length);
        var visited = new bool[maxHeight, maxWidth];
        var contours = new List<List<Tuple<int, int>>>();

        for (int r = 0; r < maxHeight; r++)
            for (int c = 0; c < data[r].Length; c++)
                if (data[r][c] == '+' && !visited[r, c])
                {
                    var contour = new List<Tuple<int, int>>();
                    DfsContour(r, c, data, visited, contour);
                    contours.Add(contour);
                }

        contours.Sort((a, b) => a.Min(p => p.Item2).CompareTo(b.Min(p => p.Item2)));
        var result = new StringBuilder();
        foreach (var contour in contours)
        {
            var minC = contour.Min(p => p.Item2);
            var maxC = contour.Max(p => p.Item2);
            var artStrings = new List<string>();
            for (int r = 0; r < maxHeight; r++)
            {
                var row = new StringBuilder();
                for (int c = minC; c <= maxC; c++)
                {
                    row.Append(contour.Any(p => p.Item1 == r && p.Item2 == c) ? '+' : ' ');
                }
                artStrings.Add(row.ToString());
            }

            var normalizedArt = NormalizeArt(artStrings);
            var digitIndex = _normalizedDigits.IndexOf(normalizedArt);
            if (digitIndex != -1) result.Append(digitIndex);
        }
        return result.ToString();
    }

    static string SolveSscon(List<string> data)
    {
        var line = data[0];
        var matches = Regex.Matches(line, @"\u00AC[^\u00AC]+\.\.\.");
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].Value[1] == 'r')
            {
                if (i == 0)
                {
                    if (_ssConCounter != 9)
                    {
                        _ssConCounter++;
                        return "1";
                    }
                    else
                    {
                        _ssConCounter = 0;
                        return "0";
                    }
                }
                if (i == 5) return "0";
                return (i + 1).ToString();
            }
        }
        return "0";
    }

    static string SolveSscolv(string line)
    {
        var match = Regex.Match(line, @"\u00AC([A-Za-z])\+");
        if (match.Success)
        {
            var colorChar = match.Groups[1].Value[0];
            return _colorCharToName[colorChar];
        }
        return "UNKNOWN";
    }

    static string SolveRsf(List<string> data)
    {
        var line = data[0];
        return (line[0] - 'a').ToString();
    }

    static string SolveRsn(List<string> data)
    {
        var line = data[0];
        var match = Regex.Match(line, @"\[(.*?)\]\[(.*?)\]");
        var nums = match.Groups[1].Value.Split(',').Select(s => s.Trim()).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
        var k = int.Parse(match.Groups[2].Value);
        var diff = nums[1] - nums[0];
        return (nums[0] + (long)k * diff).ToString();
    }

    static string SolveRscolv(string line)
    {
        var colorChar = line[1];
        return _colorCharToName[colorChar];
    }

    static string SolveGsm(List<string> data)
    {
        var line = data[0];
        var match = Regex.Match(line, @"\d+");
        var atomicNum = int.Parse(match.Value);
        return _atomicToSymbol[atomicNum];
    }
}
