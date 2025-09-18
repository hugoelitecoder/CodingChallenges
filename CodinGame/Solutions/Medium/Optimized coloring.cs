using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int w = int.Parse(Console.ReadLine());
        int h = int.Parse(Console.ReadLine());
        var sheet = new string[h];
        for (int i = 0; i < h; i++)
            sheet[i] = Console.ReadLine().PadRight(w);

        var sections = Section.AnalyseSections(sheet, w, h);

        int answer = Colorizer.FillSections(sections);
        Console.WriteLine(answer);
    }
}

class Section
{
    public int Index { get; }
    public List<(int x,int y)> Cases { get; }
    public List<int> Neighbours { get; }
    public int Color { get; set; }

    public Section(int idx, List<(int x,int y)> cases)
    {
        Index = idx;
        Cases = cases;
        Neighbours = new List<int>();
        Color = -1;
    }

    static List<(int x,int y)> FindSection(string[] sheet, int w, int h, int sx, int sy, 
                                            bool[,] visited)
    {
        var result = new List<(int,int)>();
        var q = new Queue<(int,int)>();
        q.Enqueue((sx,sy));
        visited[sy,sx] = true;

        var dirs = new (int dx,int dy)[] { (1,0),(-1,0),(0,1),(0,-1) };
        while (q.Count > 0)
        {
            var (x,y) = q.Dequeue();
            result.Add((x,y));
            foreach (var (dx,dy) in dirs)
            {
                int nx = x + dx, ny = y + dy;
                if (nx>=0 && nx<w && ny>=0 && ny<h
                    && !visited[ny,nx]
                    && sheet[ny][nx] == ' ')
                {
                    visited[ny,nx] = true;
                    q.Enqueue((nx,ny));
                }
            }
        }

        return result;
    }

    public static List<Section> AnalyseSections(string[] sheet, int w, int h)
    {
        var visited = new bool[h,w];
        var sections = new List<Section>();

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            if (!visited[y,x] && sheet[y][x] == ' ')
            {
                var cases = FindSection(sheet, w, h, x, y, visited);
                sections.Add(new Section(sections.Count, cases));
            }
        }

        int[] dx = { 1,-1, 0, 0 };
        int[] dy = { 0, 0, 1,-1 };
        var owner = new Dictionary<(int,int),int>();
        foreach (var sec in sections)
            foreach (var c in sec.Cases)
                owner[c] = sec.Index;

        foreach (int y in Enumerable.Range(0,h))
        foreach (int x in Enumerable.Range(0,w))
        {
            if (sheet[y][x] == ' ') continue;
            for (int d = 0; d < 4; d++)
            {
                int x1 = x + dx[d], y1 = y + dy[d];
                int x2 = x - dx[d], y2 = y - dy[d];
                if (x1>=0&&x1<w&&y1>=0&&y1<h &&x2>=0&&x2<w&&y2>=0&&y2<h
                 && sheet[y1][x1]==' ' && sheet[y2][x2]==' ')
                {
                    int a = owner[(x1,y1)];
                    int b = owner[(x2,y2)];
                    if (a != b)
                    {
                        if (!sections[a].Neighbours.Contains(b))
                            sections[a].Neighbours.Add(b);
                        if (!sections[b].Neighbours.Contains(a))
                            sections[b].Neighbours.Add(a);
                    }
                }
            }
        }

        return sections;
    }
}

static class Colorizer
{
    static bool IsValid(int idx, int c, List<Section> secs)
    {
        foreach (var nb in secs[idx].Neighbours)
            if (secs[nb].Color == c)
                return false;
        return true;
    }

    public static int FillSections(List<Section> secs)
    {
        int n = secs.Count;
        if (n == 0) return 0;
        if (n == 1) return 1;

        for (int k = 1; k <= 4; k++)
        {
            if (TryColor(secs, 0, k))
                return k;
            foreach (var s in secs) s.Color = -1;
        }
        return 4;
    }

    static bool TryColor(List<Section> secs, int idx, int maxC)
    {
        if (idx == secs.Count) return true;
        secs.Sort((a,b) => b.Neighbours.Count - a.Neighbours.Count);
        var sec = secs[idx];
        for (int c = 1; c <= maxC; c++)
        {
            if (IsValid(sec.Index, c, secs))
            {
                sec.Color = c;
                if (TryColor(secs, idx+1, maxC))
                    return true;
                sec.Color = -1;
            }
        }
        return false;
    }
}
