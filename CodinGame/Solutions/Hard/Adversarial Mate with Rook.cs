using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var state = new GameState(
            new Position(parts[0]),
            new Position(parts[1]),
            new Position(parts[2]),
            PieceColor.White
        );

        var engine = new ChessGame();
        var sw = Stopwatch.StartNew();
        var move = engine.FindNextBestMove(state);
        sw.Stop();

        Console.WriteLine(move);
        Console.Error.WriteLine($"Initial search: {sw.ElapsedMilliseconds}ms, MemoWhite={ChessGame.MemoWhiteCount}, MemoBlack={ChessGame.MemoBlackCount}");

        if (!move.From.IsValid()) return;
        state = state.ApplyWhite(move);

        string op;
        while (!string.IsNullOrEmpty(op = Console.ReadLine()))
        {
            var to = new Position(op.Substring(2, 2));
            state = state.ApplyBlack(new Move(state.BK, to));

            sw.Restart();
            move = engine.FindNextBestMove(state);
            sw.Stop();

            Console.WriteLine(move);
            if (!move.From.IsValid()) break;
            state = state.ApplyWhite(move);
        }
    }
}

public enum PieceColor { White, Black }

public struct Position : IEquatable<Position>
{
    private const int BoardSize = 8;
    public readonly int File, Rank;

    public Position(int file, int rank) { File = file; Rank = rank; }

    public Position(string alg)
    {
        if (alg?.Length == 2 && alg[0] >= 'a' && alg[0] < 'a' + BoardSize && alg[1] >= '1' && alg[1] < '1' + BoardSize)
        {
            File = alg[0] - 'a';
            Rank = alg[1] - '1';
        }
        else
        {
            File = Rank = -1;
        }
    }

    public bool IsValid() => File >= 0 && Rank >= 0 && File < BoardSize && Rank < BoardSize;
    public bool Equals(Position other) => File == other.File && Rank == other.Rank;
    public override bool Equals(object obj) => obj is Position p && Equals(p);
    public override int GetHashCode() => HashCode.Combine(File, Rank);
    public override string ToString() => IsValid() ? $"{(char)('a' + File)}{Rank + 1}" : "invalid";

    public bool IsAdjacentOrSame(Position other)
        => Math.Abs(File - other.File) <= 1 && Math.Abs(Rank - other.Rank) <= 1;
}

public struct Move
{
    public readonly Position From, To;
    public Move(Position from, Position to) { From = from; To = to; }
    public override string ToString() => $"{From}{To}";
}

public struct GameState : IEquatable<GameState>
{
    public readonly Position WK, WR, BK;
    public readonly PieceColor Side;

    public GameState(Position wk, Position wr, Position bk, PieceColor side)
    {
        WK = wk; WR = wr; BK = bk; Side = side;
    }

    public GameState ApplyWhite(Move m)
        => m.From.Equals(WK)
            ? new GameState(m.To, WR, BK, PieceColor.Black)
            : new GameState(WK, m.To, BK, PieceColor.Black);

    public GameState ApplyBlack(Move m)
        => new GameState(WK, WR, m.To, PieceColor.White);

    public bool Equals(GameState other)
        => Side == other.Side && WK.Equals(other.WK) && WR.Equals(other.WR) && BK.Equals(other.BK);
    public override bool Equals(object obj) => obj is GameState gs && Equals(gs);
    public override int GetHashCode() => HashCode.Combine(Side, WK, WR, BK);
}

public class ChessGame
{
    private const int MaxDepth = 7;

    private static readonly Dictionary<(GameState state, int depth), bool> MemoWhite = new();
    private static readonly Dictionary<(GameState state, int depth), bool> MemoBlack = new();

    public static int MemoWhiteCount => MemoWhite.Count;
    public static int MemoBlackCount => MemoBlack.Count;

    public Move FindNextBestMove(GameState s)
    {
        var whiteMoves = GenerateWhiteMoves(s);
        if (whiteMoves.Count == 0) return default;

        foreach (var depth in Enumerable.Range(1, MaxDepth))
        foreach (var wm in whiteMoves)
        {
            var next = s.ApplyWhite(wm);
            bool check = IsCheck(next);
            var blackMoves = GenerateBlackMoves(next);

            if (check && !blackMoves.Any())
                return wm;

            if (depth > 1 && CanBlackAvoid(next, depth - 1, check, blackMoves))
                return wm;
        }

        return whiteMoves[0];
    }

    private static List<Move> GenerateWhiteMoves(GameState s)
    {
        var moves = new List<Move>(16);
        moves.AddRange(GenerateKingMoves(s.WK, s, isWhite: true));
        moves.AddRange(GenerateRookMoves(s.WR, s));
        return moves;
    }

    private static List<Move> GenerateBlackMoves(GameState s)
        => GenerateKingMoves(s.BK, s, isWhite: false).ToList();

    private static IEnumerable<Move> GenerateKingMoves(Position kingPos, GameState s, bool isWhite)
    {
        var dirs = new (int df, int dr)[] {(-1, -1), (-1, 0), (-1, 1),(0, -1),(0, 1),(1, -1),  (1, 0),  (1, 1)};
        foreach (var (df, dr) in dirs)
        {
            var to = new Position(kingPos.File + df, kingPos.Rank + dr);
            if (!to.IsValid()) continue;
            if (isWhite)
            {
                if (to.Equals(s.WR) || to.IsAdjacentOrSame(s.BK)) continue;
                yield return new Move(kingPos, to);
            }
            else
            {
                if (to.Equals(s.WK)) continue;
                if (to.Equals(s.WR) && to.IsAdjacentOrSame(s.WK)) continue;
                if (to.Equals(s.WR) || !IsAttackedByWhite(to, s))
                    yield return new Move(kingPos, to);
            }
        }
    }

    private static IEnumerable<Move> GenerateRookMoves(Position rook, GameState s)
    {
        var dirs = new (int df, int dr)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
        foreach (var (df, dr) in dirs)
        {
            int f = rook.File, r = rook.Rank;
            while (true)
            {
                f += df; r += dr;
                var to = new Position(f, r);
                if (!to.IsValid()) break;
                if (to.Equals(s.WK) || to.Equals(s.BK))
                {
                    if (to.Equals(s.BK)) yield return new Move(rook, to);
                    break;
                }
                yield return new Move(rook, to);
            }
        }
    }

    private static bool IsAttackedByWhite(Position t, GameState s)
    {
        if (t.IsAdjacentOrSame(s.WK)) return true;
        if (t.Rank == s.WR.Rank)
        {
            int start = Math.Min(t.File, s.WR.File) + 1;
            int end   = Math.Max(t.File, s.WR.File);
            for (int x = start; x < end; x++)
                if (new Position(x, t.Rank).Equals(s.WK))
                    return false;
            return true;
        }
        if (t.File == s.WR.File)
        {
            int start = Math.Min(t.Rank, s.WR.Rank) + 1;
            int end   = Math.Max(t.Rank, s.WR.Rank);
            for (int y = start; y < end; y++)
                if (new Position(t.File, y).Equals(s.WK))
                    return false;
            return true;
        }
        return false;
    }

    private static bool IsCheck(GameState s) => IsAttackedByWhite(s.BK, s);

    private static bool CanBlackAvoid(GameState s, int depth, bool inCheck, List<Move> blackMoves)
    {
        var key = (s, depth);
        if (MemoBlack.TryGetValue(key, out var memo)) return memo;

        if (!blackMoves.Any())
            return MemoBlack[key] = inCheck;

        foreach (var bm in blackMoves)
        {
            if (bm.To.Equals(s.WR))
                return MemoBlack[key] = false;

            var ns = s.ApplyBlack(bm);
            if (!CanWhiteForce(ns, depth))
                return MemoBlack[key] = false;
        }
        return MemoBlack[key] = true;
    }

    private static bool CanWhiteForce(GameState s, int depth)
    {
        var key = (s, depth);
        if (MemoWhite.TryGetValue(key, out var memo)) return memo;
        if (depth == 0) return MemoWhite[key] = false;

        foreach (var wm in GenerateWhiteMoves(s))
        {
            var ns = s.ApplyWhite(wm);
            bool chk = IsCheck(ns);
            var bf = GenerateBlackMoves(ns);
            if (chk && !bf.Any()) return MemoWhite[key] = true;
            if (CanBlackAvoid(ns, depth - 1, chk, bf)) return MemoWhite[key] = true;
        }
        return MemoWhite[key] = false;
    }
}
