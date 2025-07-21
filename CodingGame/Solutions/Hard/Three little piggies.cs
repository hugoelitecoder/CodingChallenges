using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var field = new Field();
        for (int i = 0; i < 4; i++) field.SetRow(i, Console.ReadLine());

        var solver = new HouseSolver(field);
        solver.Solve();

        foreach (var row in solver.Field.ToCharArrayArray())
            Console.WriteLine(new string(row));
    }
}

class Position : IEquatable<Position>
{
    public int Y { get; }
    public int X { get; }
    public Position(int y, int x) { Y = y; X = x; }
    public Position Offset(int dy, int dx) => new Position(Y + dy, X + dx);
    public bool Equals(Position other) => other != null && Y == other.Y && X == other.X;
    public override bool Equals(object obj) => Equals(obj as Position);
    public override int GetHashCode() => Y * 17 + X;
    public override string ToString() => $"({Y},{X})";
    public bool InBounds() => 0 <= Y && Y < 4 && 0 <= X && X < 4;
}

class Field
{
    public char[,] Grid { get; } = new char[4, 4];
    public static readonly Position[] TreePositions = { new Position(0, 0), new Position(0, 3), new Position(3, 0) };

    public void SetRow(int y, string line)
    {
        for (int x = 0; x < 4; x++)
            Grid[y, x] = line[x];
    }

    public bool IsTree(Position pos) => TreePositions.Any(p => p.Equals(pos));

    public bool HasWolf(out Position wolf)
    {
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                if (Grid[y, x] == 'W')
                {
                    wolf = new Position(y, x);
                    return true;
                }
        wolf = null;
        return false;
    }

    public List<Position> FindPigs()
    {
        var pigs = new List<Position>();
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                if (Grid[y, x] == 'P')
                    pigs.Add(new Position(y, x));
        return pigs;
    }

    public char[,] CloneGrid()
    {
        var copy = new char[4, 4];
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                copy[y, x] = Grid[y, x];
        return copy;
    }

    public char[][] ToCharArrayArray()
    {
        var arr = new char[4][];
        for (int i = 0; i < 4; i++)
        {
            arr[i] = new char[4];
            for (int j = 0; j < 4; j++)
                arr[i][j] = Grid[i, j];
        }
        return arr;
    }
}

class TileShape
{
    public string FillChar { get; }
    public List<List<Position>> Orientations { get; }
    public TileShape(string fill, List<List<Position>> orients)
    {
        FillChar = fill;
        Orientations = orients;
    }

    public static TileShape Straw => new TileShape("s", new List<List<Position>> {
        new List<Position>{ new Position(0,-1), new Position(1,0)},
        new List<Position>{ new Position(-1,0), new Position(0,-1)},
        new List<Position>{ new Position(0,1), new Position(1,0)},
        new List<Position>{ new Position(-1,0), new Position(0,1)}
    });

    public static TileShape Sticks => new TileShape("S", new List<List<Position>> {
        new List<Position>{ new Position(0,-1), new Position(0,1)},
        new List<Position>{ new Position(-1,0), new Position(1,0)},
        new List<Position>{ new Position(0,-1), new Position(0,1)},
        new List<Position>{ new Position(-1,0), new Position(1,0)}
    });

    public static TileShape Brick => new TileShape("B", new List<List<Position>> {
        new List<Position>{ new Position(1,0), new Position(0,1), new Position(0,2)},
        new List<Position>{ new Position(-2,0), new Position(-1,0), new Position(0,1)},
        new List<Position>{ new Position(-1,0), new Position(0,-2), new Position(0,-1)},
        new List<Position>{ new Position(0,1), new Position(1,0), new Position(2,0)}
    });
}

class TilePlacement
{
    public TileShape Tile { get; }
    public Position Center { get; }
    public List<Position> Coverage { get; }
    public TilePlacement(TileShape tile, Position center, List<Position> rel)
    {
        Tile = tile;
        Center = center;
        Coverage = new List<Position> { center };
        Coverage.AddRange(rel.Select(d => center.Offset(d.Y, d.X)));
    }
}

class HouseSolver
{
    public Field Field { get; }
    readonly bool night;
    readonly Position wolf;
    readonly HashSet<Position> pigs;
    static readonly TileShape[] Tiles = { TileShape.Straw, TileShape.Sticks, TileShape.Brick };

    public HouseSolver(Field f)
    {
        Field = f;
        pigs = Field.FindPigs().ToHashSet();
        night = Field.HasWolf(out wolf);
    }

    public void Solve()
    {
        foreach (var placements in AllTilePlacements())
        {
            if (Valid(placements))
            {
                Place(placements);
                break;
            }
        }
    }

    IEnumerable<List<TilePlacement>> AllTilePlacements()
    {
        var centers = AllPermutations(AllGridPositions(), 3);
        var dirs = AllProducts(Enumerable.Range(0, 4), Enumerable.Range(0, 4), Enumerable.Range(0, 4));
        foreach (var pos in centers)
            foreach (var d in dirs)
                yield return new List<TilePlacement> {
                    new TilePlacement(Tiles[0], pos[0], Tiles[0].Orientations[d[0]]),
                    new TilePlacement(Tiles[1], pos[1], Tiles[1].Orientations[d[1]]),
                    new TilePlacement(Tiles[2], pos[2], Tiles[2].Orientations[d[2]])
                };
    }

    bool Valid(List<TilePlacement> placements)
    {
        var allCellsFlat = placements.SelectMany(x => x.Coverage).ToList();
        if (allCellsFlat.Any(Field.IsTree)) return false;
        if (night && !pigs.IsSubsetOf(placements.Select(x => x.Center).ToHashSet())) return false;
        if (!night && placements.Any(x => pigs.Contains(x.Center))) return false;
        if (night && allCellsFlat.Any(c => c.Equals(wolf))) return false;
        if (allCellsFlat.GroupBy(c => c).Any(g => g.Count() > 1)) return false;
        if (placements.Any(p => p.Coverage.Skip(1).Any(pos => !pos.InBounds()))) return false;
        if (!night && placements.Any(p => p.Coverage.Skip(1).Any(pos => pigs.Contains(pos)))) return false;
        if (night && placements.Any(p => p.Coverage.Skip(1).Any(pos => pos.Equals(wolf)))) return false;
        return true;
    }

    void Place(List<TilePlacement> placements)
    {
        foreach (var p in placements)
        {
            Field.Grid[p.Center.Y, p.Center.X] = 'H';
            foreach (var c in p.Coverage.Skip(1))
                Field.Grid[c.Y, c.X] = p.Tile.FillChar[0];
        }
    }

    static IEnumerable<Position> AllGridPositions()
    {
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                yield return new Position(y, x);
    }

    static IEnumerable<T[]> AllPermutations<T>(IEnumerable<T> items, int k)
    {
        var arr = items.ToArray();
        foreach (var idxs in KPerms(arr.Length, k))
            yield return idxs.Select(i => arr[i]).ToArray();
    }

    static IEnumerable<int[]> KPerms(int n, int k)
    {
        var idxs = Enumerable.Range(0, n).ToArray();
        var used = new bool[n];
        var result = new int[k];
        IEnumerable<int[]> Gen(int d)
        {
            if (d == k)
            {
                yield return (int[])result.Clone();
                yield break;
            }
            for (int i = 0; i < n; i++)
            {
                if (used[i]) continue;
                used[i] = true;
                result[d] = idxs[i];
                foreach (var r in Gen(d + 1))
                    yield return r;
                used[i] = false;
            }
        }
        foreach (var r in Gen(0)) yield return r;
    }

    static IEnumerable<int[]> AllProducts(IEnumerable<int> a, IEnumerable<int> b, IEnumerable<int> c)
    {
        foreach (var x in a)
            foreach (var y in b)
                foreach (var z in c)
                    yield return new[] { x, y, z };
    }
}
