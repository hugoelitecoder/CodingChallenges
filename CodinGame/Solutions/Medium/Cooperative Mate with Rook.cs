using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

public class Program
{
    public static void Main()
    {
        var inpts = Console.ReadLine().Split();
        var pieces = new List<Piece>
        {
            new Piece(PieceType.King, PieceColor.White, new Position(inpts[1])),
            new Piece(PieceType.Rook, PieceColor.White, new Position(inpts[2])),
            new Piece(PieceType.King, PieceColor.Black, new Position(inpts[3])),
        };
        var initial = new Chessboard(pieces, inpts[0] == "white" ? PieceColor.White : PieceColor.Black);

        var solver = new ChessSolver();

        var stopwatch = Stopwatch.StartNew();
        var (moves, visitedCount) = solver.FindShortestMate(initial);
        stopwatch.Stop();

        Console.Error.WriteLine("[DEBUG] Cooperative Mate");
        Console.Error.WriteLine($"[DEBUG] Solution found in: {stopwatch.ElapsedMilliseconds}ms");
        Console.Error.WriteLine($"[DEBUG] Unique states analyzed: {visitedCount:N0}");
        
        var currentBoard = initial;
        Console.Error.WriteLine("\n[DEBUG] Initial Board");
        PrintBoard(currentBoard);

        for(int i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Console.Error.WriteLine($"\n[DEBUG] Move {i + 1}: {move}");
            
            var (nextBoard, _) = currentBoard.GetLegalMoves().First(m => m.move.Equals(move));
            currentBoard = nextBoard;
            PrintBoard(currentBoard);
        }

        Console.WriteLine(string.Join(" ", moves));
    }

    private static void PrintBoard(Chessboard board)
    {
        var grid = new char[8, 8];
        for (int r = 0; r < 8; r++)
            for (int f = 0; f < 8; f++)
                grid[f, r] = '.';

        foreach(var piece in board.Pieces)
        {
            char c = piece.Type switch
            {
                PieceType.King => 'k',
                PieceType.Rook => 'r',
                _ => '?'
            };
            if (piece.Color == PieceColor.White) c = char.ToUpper(c);
            grid[piece.Position.File, piece.Position.Rank] = c;
        }
        
        Console.Error.WriteLine("  +-----------------+");
        for (int r = 7; r >= 0; r--)
        {
            Console.Error.Write($"{r + 1} | ");
            for (int f = 0; f < 8; f++)
            {
                Console.Error.Write(grid[f, r] + " ");
            }
            Console.Error.WriteLine("|");
        }
        Console.Error.WriteLine("  +-----------------+");
        Console.Error.WriteLine("    a b c d e f g h");
    }
}

public class ChessSolver
{
    public (List<Move> Moves, int VisitedStates) FindShortestMate(Chessboard initial)
    {
        var queue = new Queue<Chessboard>();
        var predecessors = new Dictionary<Chessboard, (Chessboard parent, Move move)>();
        queue.Enqueue(initial);
        predecessors[initial] = (null, default);
        Chessboard mateState = null;

        while (queue.Count > 0)
        {
            var currentBoard = queue.Dequeue();
            if (currentBoard.IsCheckmate) { mateState = currentBoard; break; }
            foreach (var (nextBoard, move) in currentBoard.GetLegalMoves())
            {
                if (!predecessors.ContainsKey(nextBoard))
                {
                    predecessors[nextBoard] = (currentBoard, move);
                    queue.Enqueue(nextBoard);
                }
            }
        }

        var solution = new List<Move>();
        if (mateState != null)
        {
            for (var current = mateState; predecessors[current].parent != null; current = predecessors[current].parent)
                solution.Add(predecessors[current].move);
            solution.Reverse();
        }
        return (solution, predecessors.Count);
    }
}

public class Chessboard : IEquatable<Chessboard>
{
    private readonly long _packedState;
    
    public Chessboard(IEnumerable<Piece> pieces, PieceColor sideToMove)
    {
        _packedState = Pack(pieces, sideToMove);
    }
    
    private Chessboard(long packedState) { _packedState = packedState; }

    public PieceColor SideToMove => GetTurn(_packedState) ? PieceColor.White : PieceColor.Black;
    public bool IsCheckmate => IsMateInternal(_packedState);
    public bool IsInCheck => IsCheckInternal(_packedState);
    public IEnumerable<Piece> Pieces
    {
        get
        {
            yield return new Piece(PieceType.King, PieceColor.White, new Position(GetWk(_packedState)));
            yield return new Piece(PieceType.Rook, PieceColor.White, new Position(GetWr(_packedState)));
            yield return new Piece(PieceType.King, PieceColor.Black, new Position(GetBk(_packedState)));
        }
    }

    public List<(Chessboard nextBoard, Move move)> GetLegalMoves()
    {
        var results = new List<(Chessboard, Move)>();
        foreach (var (nextKey, move) in GenerateMovesInternal(_packedState))
            results.Add((new Chessboard(nextKey), move));
        return results;
    }
    
    public bool Equals(Chessboard other) => other != null && _packedState == other._packedState;
    public override bool Equals(object obj) => obj is Chessboard other && Equals(other);
    public override int GetHashCode() => _packedState.GetHashCode();
    
    private const int OFF_WK = 0, OFF_WR = 6, OFF_BK = 12, OFF_TM = 18;
    private static readonly ulong[] KingMoves = BuildKingMoves();
    
    private static long Pack(IEnumerable<Piece> pieces, PieceColor sideToMove)
    {
        byte wk=255, wr=255, bk=255;
        foreach(var p in pieces) {
            if(p.Color==PieceColor.White){ if(p.Type==PieceType.King)wk=p.Position.ToSquareIndex(); else if(p.Type==PieceType.Rook)wr=p.Position.ToSquareIndex(); }
            else { if(p.Type==PieceType.King)bk=p.Position.ToSquareIndex(); }
        }
        return Pack(wk, wr, bk, sideToMove == PieceColor.White);
    }
    
    private static long Pack(byte wk, byte wr, byte bk, bool isWhiteTurn) => (long)wk | ((long)wr << OFF_WR) | ((long)bk << OFF_BK) | (isWhiteTurn ? 1L << OFF_TM : 0);
    private static byte GetWk(long key) => (byte)(key & 0x3F);
    private static byte GetWr(long key) => (byte)((key >> OFF_WR) & 0x3F);
    private static byte GetBk(long key) => (byte)((key >> OFF_BK) & 0x3F);
    private static bool GetTurn(long key) => ((key >> OFF_TM) & 1L) != 0;

    private static List<(long key, Move move)> GenerateMovesInternal(long key) {
        byte wk=GetWk(key), wr=GetWr(key), bk=GetBk(key);
        ulong occupied = (1UL << wk) | (1UL << wr) | (1UL << bk);
        return GetTurn(key) ? GenerateWhiteMoves(key, occupied) : GenerateBlackMoves(key, occupied);
    }
    
    private static List<(long, Move)> GenerateWhiteMoves(long key, ulong occupied) {
        var moves = new List<(long, Move)>();
        byte wk=GetWk(key), wr=GetWr(key), bk=GetBk(key);
        ulong kingDests = KingMoves[wk] & ~(1UL << wr) & ~(1UL << bk);
        while(kingDests != 0) {
            int to = BitOperations.TrailingZeroCount(kingDests); kingDests &= kingDests-1;
            if((KingMoves[to] & (1UL << bk)) == 0)
                moves.Add((Pack((byte)to, wr, bk, false), new Move(PieceType.King, new Position(wk), new Position((byte)to))));
        }
        AddRookMoves(moves, key, occupied);
        return moves;
    }

    private static void AddRookMoves(List<(long, Move)> moves, long key, ulong occupied) {
        byte wk=GetWk(key), wr=GetWr(key), bk=GetBk(key);
        var dirs = new[] {-8, 8, -1, 1};
        foreach(var dir in dirs){
            for(int i=1; i<8; ++i){
                int to = wr + dir * i;
                if(to<0 || to>=64 || (Math.Abs(dir)==1 && new Position((byte)to).Rank != new Position(wr).Rank)) break;
                if(((occupied >> to) & 1) != 0) break;
                moves.Add((Pack(wk,(byte)to,bk,false), new Move(PieceType.Rook, new Position(wr), new Position((byte)to))));
            }
        }
    }

    private static List<(long, Move)> GenerateBlackMoves(long key, ulong occupied) {
        var moves = new List<(long, Move)>();
        byte wk=GetWk(key), wr=GetWr(key), bk=GetBk(key);
        ulong kingDests = KingMoves[bk] & ~(1UL << wk) & ~(1UL << wr);
        while(kingDests!=0){
            int to = BitOperations.TrailingZeroCount(kingDests); kingDests&=kingDests-1;
            if (to == wr || !IsCheckInternal(Pack(wk, wr, (byte)to, false)))
                 moves.Add((Pack(wk, wr, (byte)to, true), new Move(PieceType.King, new Position(bk), new Position((byte)to))));
        }
        return moves;
    }
    
    private static bool IsMateInternal(long key) => !GetTurn(key) && IsCheckInternal(key) && !BlackHasEscapeInternal(key);

    private static bool BlackHasEscapeInternal(long key) {
        byte wk=GetWk(key), wr=GetWr(key), bk=GetBk(key);
        ulong kingDests = KingMoves[bk] & ~(1UL << wk);
        while (kingDests != 0){
            int to = BitOperations.TrailingZeroCount(kingDests); kingDests &= kingDests-1;
            if((KingMoves[to] & (1UL << wk)) != 0) continue;
            if(to == wr) return true;
            if(!IsCheckInternal(Pack(wk, wr, (byte)to, false))) return true;
        } return false;
    }

    private static bool IsCheckInternal(long key) {
        byte wk=GetWk(key), wr=GetWr(key), bk=GetBk(key);
        if((KingMoves[wk] & (1UL << bk))!=0) return true;
        int rkF=new Position(wr).File, rkR=new Position(wr).Rank, bkF=new Position(bk).File, bkR=new Position(bk).Rank;
        if(rkF != bkF && rkR != bkR) return false;
        int dx=Math.Sign(bkF-rkF), dy=Math.Sign(bkR-rkR);
        for(int x=rkF+dx, y=rkR+dy; x!=bkF || y!=bkR; x+=dx, y+=dy)
            if(new Position(x,y).ToSquareIndex() == wk || new Position(x,y).ToSquareIndex() == wr) return false;
        return true;
    }

    private static ulong[] BuildKingMoves() { var k=new ulong[64]; for(int i=0;i<64;++i) for(int dx=-1;dx<=1;++dx) for(int dy=-1;dy<=1;++dy) { if(dx==0&&dy==0) continue; int f=(i&7)+dx,r=(i>>3)+dy; if(f>=0&&f<8&&r>=0&&r<8) k[i]|=1UL<<(r*8+f); } return k; }
}

public enum PieceType { King, Rook }
public enum PieceColor { White, Black }
public readonly struct Position : IEquatable<Position>
{
    public int File { get; }
    public int Rank { get; }
    public Position(int file, int rank) 
    { 
        File = file; 
        Rank = rank; 
    }
    public Position(string algebraic) {
         if(algebraic?.Length==2&&algebraic[0]>='a'&&algebraic[0]<='h'&&algebraic[1]>='1'&&algebraic[1]<='8'){File=algebraic[0]-'a';Rank=algebraic[1]-'1';}else{File=-1;Rank=-1;} 
    }
    internal Position(byte squareIndex) { File = squareIndex & 7; Rank = squareIndex >> 3; }
    internal byte ToSquareIndex() => (byte)(Rank * 8 + File);
    public override string ToString() => IsValid() ? $"{(char)('a' + File)}{Rank + 1}" : "n/a";
    public bool IsValid() => File != -1;
    public bool Equals(Position other) => File == other.File && Rank == other.Rank;
    public override bool Equals(object obj) => obj is Position other && Equals(other);
    public override int GetHashCode() => IsValid() ? (File << 3) | Rank : -1;
}

public readonly struct Piece
{
    public PieceType Type { get; }
    public PieceColor Color { get; }
    public Position Position { get; }
    public Piece(PieceType type, PieceColor color, Position position) { Type = type; Color = color; Position = position; }
}

public readonly struct Move : IEquatable<Move>
{
    public PieceType Type { get; }
    public Position From { get; }
    public Position To { get; }
    internal Move(PieceType type, Position from, Position to) { Type = type; From = from; To = to; }
    public override string ToString() => $"{From}{To}";
    public bool Equals(Move other) => Type == other.Type && From.Equals(other.From) && To.Equals(other.To);
    public override bool Equals(object obj) => obj is Move other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Type, From, To);
}