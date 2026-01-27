using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
public class Solution
{
    public static void Main()
    {
        var watch = Stopwatch.StartNew();
        var header = Console.ReadLine();
        if (string.IsNullOrEmpty(header)) return;
        var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var height = int.Parse(parts[0]);
        var width = int.Parse(parts[1]);
        var grid = new char[height, width];
        for (var y = 0; y < height; y++)
        {
            var row = Console.ReadLine() ?? string.Empty;
            for (var x = 0; x < width; x++) grid[y, x] = x < row.Length ? row[x] : ' ';
        }
        var analyzer = new MountainAnalyzer(height, width, grid);
        var result = analyzer.Analyze();
        Console.Error.WriteLine($"[DEBUG] GRID: {width}X{height}");
        Console.Error.WriteLine("[DEBUG] INPUT MAP:");
        for (var y = 0; y < height; y++)
        {
            var sb = new StringBuilder();
            for (var x = 0; x < width; x++) sb.Append(grid[y, x]);
            Console.Error.WriteLine($"[DEBUG] {y:D2} | {sb}");
        }
        Console.Error.WriteLine($"[DEBUG] TIME: {watch.ElapsedMilliseconds}ms");
        Console.WriteLine(result);
        watch.Stop();
    }
}
public class MountainSection
{
    public string Id;
    public List<int> ElevationPoints = new List<int>();
    public HashSet<string> ObscuredSectionIds = new HashSet<string>();
    public int StartX;
}
public class MountainAnalyzer
{
    private const char EMPTY = ' ';
    private const char FLAT = '-';
    private const char UP = '/';
    private const char DOWN = '\\';
    private const int ASCII_A = 65;
    private readonly int _gridHeight;
    private readonly int _gridWidth;
    private readonly char[,] _grid;
    public MountainAnalyzer(int height, int width, char[,] grid)
    {
        _gridHeight = height;
        _gridWidth = width;
        _grid = grid;
    }
    public int Analyze()
    {
        var intersections = FindIntersections();
        var allSections = new Dictionary<string, MountainSection>();
        var activeSectionIds = InitializeLeadingEdge(allSections);
        for (var x = 1; x < _gridWidth; x++)
        {
            var activeIdx = 0;
            for (var y = _gridHeight - 1; y >= 0; y--)
            {
                if (_grid[y, x] is EMPTY) continue;
                var currentId = activeSectionIds[activeIdx++];
                if (intersections.TryGetValue((x, y), out var isForegroundMerge))
                    ProcessIntersection(x, y, isForegroundMerge, currentId, allSections, activeSectionIds, ref activeIdx);
                else
                    AppendToSection(x, y, currentId, allSections);
            }
        }
        var mountainRidges = GroupRidges(allSections);
        var elevationProfiles = BuildProfiles(mountainRidges, allSections);
        var totalSummits = 0;
        Console.Error.WriteLine($"[DEBUG] TOTAL SECTIONS: {allSections.Count}");
        Console.Error.WriteLine($"[DEBUG] TOTAL RIDGES: {mountainRidges.Count}");
        for (int i = 0; i < elevationProfiles.Count; i++)
        {
            var summits = CountLocalSummits(elevationProfiles[i]);
            totalSummits += summits;
            Console.Error.WriteLine($"[DEBUG] RIDGE {i} SUMMITS: {summits}");
        }
        return totalSummits;
    }
    private List<string> InitializeLeadingEdge(Dictionary<string, MountainSection> sections)
    {
        var active = new List<string>();
        for (var y = _gridHeight - 1; y >= 0; y--)
        {
            if (_grid[y, 0] is EMPTY) continue;
            var id = ((char)(ASCII_A + sections.Count)).ToString();
            var initialElevation = _grid[y, 0] is DOWN ? y - 1 : y;
            sections[id] = new MountainSection { Id = id, ElevationPoints = new List<int> { initialElevation }, StartX = 0 };
            if (sections.Count > 1) sections[((char)(ASCII_A + sections.Count - 2)).ToString()].ObscuredSectionIds.Add(id);
            active.Add(id);
        }
        return active;
    }
    private void ProcessIntersection(int x, int y, bool isForegroundMerge, string currentId, Dictionary<string, MountainSection> sections, List<string> active, ref int idx)
    {
        if (isForegroundMerge)
        {
            sections[currentId].ElevationPoints.Add(y);
            sections[currentId].ObscuredSectionIds.Add(active[idx]);
            active.RemoveAt(idx);
        }
        else
        {
            var newId = ((char)(ASCII_A + sections.Count)).ToString();
            var hidden = idx < active.Count ? new HashSet<string> { active[idx] } : new HashSet<string>();
            sections[newId] = new MountainSection { Id = newId, ElevationPoints = new List<int> { y }, ObscuredSectionIds = hidden, StartX = x };
            sections[active[idx - 1]].ElevationPoints.Add(y);
            sections[active[idx - 1]].ObscuredSectionIds.Add(newId);
            active.Insert(idx++, newId);
        }
    }
    private void AppendToSection(int x, int y, string id, Dictionary<string, MountainSection> sections)
    {
        var s = sections[id];
        if (_grid[y, x] is DOWN && _grid[y, x - 1] is UP) s.ElevationPoints.Add(y - 1);
        else if (x == _gridWidth - 1 && _grid[y, x] is UP) { s.ElevationPoints.Add(y); s.ElevationPoints.Add(y - 1); }
        else s.ElevationPoints.Add(y);
    }
    private List<HashSet<string>> GroupRidges(Dictionary<string, MountainSection> sections)
    {
        var ridges = new List<HashSet<string>> { new HashSet<string> { "A" } };
        while (ridges.Last().Count > 0)
        {
            var nextSet = new HashSet<string>();
            foreach (var id in ridges.Last()) foreach (var hiddenId in sections[id].ObscuredSectionIds) nextSet.Add(hiddenId);
            foreach (var id in nextSet) foreach (var r in ridges) r.Remove(id);
            ridges.Add(nextSet);
        }
        ridges.RemoveAt(ridges.Count - 1);
        return ridges;
    }
    private List<List<int>> BuildProfiles(List<HashSet<string>> ridges, Dictionary<string, MountainSection> sections)
    {
        var profiles = new List<List<int>>();
        foreach (var ridge in ridges)
        {
            var p = new List<int>();
            foreach (var id in ridge.OrderBy(s => s))
            {
                var s = sections[id];
                if (p.Count == 0) p.AddRange(Enumerable.Repeat(s.ElevationPoints[0], s.StartX));
                else
                {
                    var lastH = Math.Min(p.Last(), s.ElevationPoints[0]);
                    for (var i = p.Count; i < s.StartX; i++)
                    {
                        var minVis = profiles.Count > 0 ? profiles.Min(prof => prof[i]) : 0;
                        lastH = Math.Max(lastH, minVis);
                        p.Add(lastH);
                    }
                }
                p.AddRange(s.ElevationPoints);
            }
            if (p.Count < _gridWidth) p.AddRange(Enumerable.Repeat(p.Last(), _gridWidth - p.Count));
            profiles.Add(p);
        }
        return profiles;
    }
    private int CountLocalSummits(List<int> profile)
    {
        var filtered = new List<int>();
        for (var i = 0; i < profile.Count; i++) if (i == 0 || profile[i] != profile[i - 1]) filtered.Add(profile[i]);
        if (filtered.Count <= 1) return filtered.Count;
        filtered.Insert(0, filtered[1]);
        filtered.Add(filtered[filtered.Count - 2]);
        var count = 0;
        for (var i = 1; i < filtered.Count - 1; i++) if (filtered[i - 1] > filtered[i] && filtered[i + 1] > filtered[i]) count++;
        return count;
    }
    private Dictionary<(int, int), bool> FindIntersections()
    {
        var matches = new Dictionary<(int, int), bool>();
        for (var x = 0; x < _gridWidth; x++) for (var y = 0; y < _gridHeight; y++)
            {
                if (IsPatternMatch(x, y, false)) matches[(x, y)] = false;
                else if (IsPatternMatch(x, y, true)) matches[(x, y)] = true;
            }
        return matches;
    }
    private bool IsPatternMatch(int x, int y, bool isForeground)
    {
        if (isForeground)
        {
            return (SafeGet(x - 2, y), SafeGet(x - 1, y), SafeGet(x - 1, y + 1), SafeGet(x, y), SafeGet(x + 1, y - 1)) switch
            {
                (FLAT, FLAT, UP, UP, UP) => true,
                _ when (SafeGet(x - 2, y - 2), SafeGet(x - 1, y - 1), SafeGet(x - 2, y), SafeGet(x - 1, y), SafeGet(x, y), SafeGet(x + 1, y)) is (DOWN, DOWN, FLAT, FLAT, FLAT, FLAT) => true,
                _ when (SafeGet(x - 2, y - 1), SafeGet(x - 1, y), SafeGet(x - 1, y + 1), SafeGet(x, y), SafeGet(x + 1, y - 1)) is (DOWN, DOWN, UP, UP, UP) => true,
                _ => false
            };
        }
        return (SafeGet(x - 1, y - 1), SafeGet(x, y), SafeGet(x + 1, y + 1), SafeGet(x + 1, y), SafeGet(x + 2, y)) switch
        {
            (DOWN, DOWN, DOWN, FLAT, FLAT) => true,
            _ when (SafeGet(x - 1, y), SafeGet(x, y), SafeGet(x + 1, y), SafeGet(x + 2, y), SafeGet(x + 1, y - 1), SafeGet(x + 2, y - 2)) is (FLAT, FLAT, FLAT, FLAT, UP, UP) => true,
            _ when (SafeGet(x - 1, y - 1), SafeGet(x, y), SafeGet(x + 1, y + 1), SafeGet(x + 1, y), SafeGet(x + 2, y - 1)) is (DOWN, DOWN, DOWN, UP, UP) => true,
            _ => false
        };
    }
    private char SafeGet(int x, int y) => (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight) ? EMPTY : _grid[y, x];
}