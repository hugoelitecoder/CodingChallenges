using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

public class Solution
{
    public const bool EnableDebug = true;

    public static void Main(string[] args)
    {
        FreecellNode.Initialize();
        var tableaux = new List<List<string>>(8);
        var moves = ReadFirstInput(tableaux);

        var node = new FreecellNode();
        var deal = GetDeal(tableaux);
        node.SetCards(deal);
        
        (var plan, var solutionPath) = Solve(node);

        var planIndex = 0;
        var tactical = new List<string>();
        var turn = 1;
        
        if (solutionPath.Any())
        {
            solutionPath[0].DebugState($"Planner's Expected State for Turn 1");
        }
        PlayMove(moves, ref plan, ref solutionPath, ref planIndex, tactical, ref turn);

        while (true)
        {
            var foundationCountPlusOneStr = Console.ReadLine();
            if (foundationCountPlusOneStr == null) break;

            Console.ReadLine();
            var foundationsLine = Console.ReadLine();
            var cellsLine = Console.ReadLine();

            var actualTableaux = new List<List<string>>();
            for (var i = 0; i < 8; i++)
            {
                actualTableaux.Add(Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList());
            }
            Console.ReadLine();
            var movesLine = Console.ReadLine();

            DebugActualState(turn, foundationsLine, cellsLine, actualTableaux);

            if (solutionPath != null && solutionPath.Count > turn - 1)
            {
                solutionPath[turn - 1].DebugState($"Planner's Expected State for Turn {turn}");
            }
            
            var currentMoves = movesLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            PlayMove(currentMoves, ref plan, ref solutionPath, ref planIndex, tactical, ref turn, actualTableaux, foundationsLine, cellsLine);
        }
    }

    private static (string, List<FreecellNode>) Solve(FreecellNode startNode)
    {
        var beam = new Beam<FreecellNode>(512, 128);
        var sw = Stopwatch.StartNew();
        var solutionNode = beam.BeamSearch(startNode);
        sw.Stop();
        Debug($"Solver time: {sw.ElapsedMilliseconds} ms");

        if (solutionNode == null)
        {
            Debug("Solver failed to find a solution.");
            return ("", new List<FreecellNode>());
        }

        var solutionPath = beam.ReconstructSolutionPath(startNode, solutionNode);
        var plan = beam.EncodeSolution(startNode, solutionNode);
        Debug($"New plan generated: {plan}");
        return (plan, solutionPath);
    }

    private static void DebugActualState(int turn, string foundationsLine, string cellsLine, List<List<string>> tableaux)
    {
        if (!EnableDebug) return;

        var sb = new StringBuilder();
        sb.AppendLine($"--- ACTUAL GAME STATE - TURN {turn} ---");
        
        var foundationRanks = new Dictionary<char, char> { {'S', '-'}, {'H', '-'}, {'D', '-'}, {'C', '-'} };
        var foundationParts = foundationsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (foundationParts.Length > 1)
        {
            foreach (var part in foundationParts.Skip(1))
            {
                if (part.Length == 3 && part[1] == '-')
                {
                    foundationRanks[part[0]] = part[2];
                }
            }
        }
    
        sb.Append("Foundations:");
        sb.Append($" S:{foundationRanks['S']}");
        sb.Append($" H:{foundationRanks['H']}");
        sb.Append($" D:{foundationRanks['D']}");
        sb.Append($" C:{foundationRanks['C']}");
        sb.AppendLine();
        
        var cellTokens = cellsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
        sb.Append("Reserve:    ");
        for (int i = 0; i < 4; i++)
        {
            if (i < cellTokens.Length && cellTokens[i] != "-")
            {
                sb.Append($"[{cellTokens[i],-2}] ");
            }
            else
            {
                sb.Append("[  ] ");
            }
        }
        sb.AppendLine();
        sb.AppendLine(); 

        sb.AppendLine("Tableaux:");
        int maxRows = 0;
        if (tableaux.Any())
        {
            maxRows = tableaux.Max(t => t.Count);
        }
        
        for (int i = 0; i < 8; i++) sb.Append($"   {i + 1}   ");
        sb.AppendLine();
        for (int i = 0; i < 8; i++) sb.Append("-------");
        sb.AppendLine();

        for (int r = 0; r < maxRows; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (c < tableaux.Count && r < tableaux[c].Count) sb.Append($"  {tableaux[c][r],-3}  ");
                else sb.Append("       ");
            }
            sb.AppendLine();
        }
        sb.AppendLine("--------------------------------------------------------");
        Debug(sb.ToString());
    }

    private static List<string> ReadFirstInput(List<List<string>> tableaux)
    {
        Console.ReadLine();
        Console.ReadLine();
        Console.ReadLine();
        Console.ReadLine();
        for (var i = 0; i < 8; i++)
            tableaux.Add(Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList());
        Console.ReadLine();
        var moves = Console.ReadLine().Split(' ').ToList();
        Debug($"Initial possible moves: {string.Join(" ", moves)}");
        return moves;
    }

    private static List<Card> GetDeal(List<List<string>> tableaux)
    {
        var cards = new List<Card>();
        int maxRows = tableaux.Any() ? tableaux.Max(t => t.Count) : 0;
        for (var r = 0; r < maxRows; r++)
            for (var c = 0; c < 8; c++)
                if (c < tableaux.Count && r < tableaux[c].Count)
                    cards.Add(ParseCard(tableaux[c][r]));
        return cards;
    }

    private static Card ParseCard(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length < 2) return new Card(-1);
        Rank rank = s[0] switch
        {
            'A' => Rank.ACE, 'T' => Rank.R10, 'J' => Rank.RJ, 'Q' => Rank.RQ, 'K' => Rank.KING,
            _ => (Rank)(s[0] - '2' + 1)
        };
        Suit suit = s[1] switch
        {
            'S' => Suit.SPADE, 'H' => Suit.HEART, 'D' => Suit.DIAMOND, 'C' => Suit.CLUB,
            _ => throw new ArgumentException()
        };
        return new Card(suit, rank);
    }

    private static void PlayMove(List<string> moves, ref string plan, ref List<FreecellNode> solutionPath, ref int planIndex, List<string> tactical, ref int turn, List<List<string>> currentTableaux = null, string foundationsLine = null, string cellsLine = null)
    {
        Debug($"\nTURN {turn}");
        Debug($"Current possible moves: {string.Join(" ", moves)}");
        Debug($"PlanIndex={planIndex} / PlanLen={plan.Length}");
        
        bool replan = false;

        if (tactical.Count > 0)
        {
            var move = tactical[0];
            if (moves.Contains(move))
            {
                Debug($"[TACTICAL] Playing: {move}");
                Console.WriteLine(move);
                tactical.RemoveAt(0);
            }
            else
            {
                Debug($"[TACTICAL FAIL] Move '{move}' not available. Clearing tactical plan and forcing re-plan.");
                tactical.Clear();
                replan = true;
            }
        }
        else if (planIndex + 2 <= plan.Length)
        {
            var solverMove = plan.Substring(planIndex, 2);
            var mapped = MapMoveToOutput(solverMove);
            Debug($"[STRATEGY] PlanMove='{solverMove}' mapped to '{mapped}'");
            if (moves.Contains(mapped))
            {
                Debug($"[STRATEGY] Playing planned: {mapped}");
                Console.WriteLine(mapped);
                planIndex += 2;
            }
            else
            {
                Debug($"[PLAN-DESYNC] Can't play planned '{mapped}'. Forcing a re-plan.");
                replan = true;
            }
        }
        else
        {
            Debug("[END OF PLAN] Plan finished. Checking if game is won or re-planning.");
            replan = true;
        }
        
        if (replan)
        {
            if (currentTableaux == null || !moves.Any() || foundationsLine == null || cellsLine == null)
            {
                 var fallback = moves.FirstOrDefault() ?? "1h";
                 Debug($"[REPLAN-FAIL] Incomplete state for re-plan. Playing fallback: {fallback}");
                 Console.WriteLine(fallback);
            } 
            else 
            {
                Debug("--- TRIGGERING RE-PLAN FROM FULL STATE ---");
                var newNode = new FreecellNode();
                newNode.InitializeFromFullState(currentTableaux, foundationsLine, cellsLine);

                (plan, solutionPath) = Solve(newNode);
                planIndex = 0;
                tactical.Clear();

                if (string.IsNullOrEmpty(plan))
                {
                    Debug("[REPLAN-FAIL] Solver could not find a solution from the current state. Playing fallback.");
                    Console.WriteLine(moves.FirstOrDefault() ?? "1h");
                }
                else
                {
                    PlayMove(moves, ref plan, ref solutionPath, ref planIndex, tactical, ref turn, currentTableaux, foundationsLine, cellsLine);
                    return;
                }
            }
        }

        turn++;
    }

    private static string MapMoveToOutput(string move)
    {
        var sFrom = move[0];
        var sTo = move[1];

        if (char.IsDigit(sFrom))
        {
            var gFrom = (char)(sFrom + 1);
            if (char.IsDigit(sTo)) return $"{gFrom}{(char)(sTo + 1)}";
            if (sTo == 'h') return $"{gFrom}h";
            if (sTo >= 'a' && sTo <= 'd')
            {
                var targetSlotIndex = sTo - 'a';
                var targetSlotLabel = (char)('a' + targetSlotIndex);
                return $"{gFrom}{targetSlotLabel}";
            }
        }
        else if (sFrom >= 'a' && sFrom <= 'd')
        {
            var fromSlotIndex = sFrom - 'a';
            var fromSlotLabel = (char)('a' + fromSlotIndex);
            
            if (sTo == 'h') return $"{fromSlotLabel}h";
            if (char.IsDigit(sTo)) return $"{fromSlotLabel}{(char)(sTo + 1)}";
        }
        return "invalid_map";
    }

    public static void Debug(string msg)
    {
        if (EnableDebug)
        {
            Console.Error.WriteLine("[DEBUG] " + msg);
        }
    }
}

enum Suit { SPADE, HEART, DIAMOND, CLUB }
enum CardColor { BLACK, RED }
enum Rank { ACE, R2, R3, R4, R5, R6, R7, R8, R9, R10, RJ, RQ, KING }
enum MoveType { None, TableauToFoundation, TableauToReserve, TableauToTableau, ReserveToFoundation, ReserveToTableau }

interface IBeamSearchNode<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    TNode After { get; set; } TNode Prev { get; set; } TNode Next { get; set; }
    BitStream Moves { get; } Move LastMove { get; } int MovesPerformed { get; }
    void CopyFrom(TNode other);
    void Expand(Pool<TNode> pool, List<TNode> outNodes);
    void ComputeHash();
    uint Hash { get; }
    bool StateEquals(TNode other);
    bool IsGoal();
    int Bin();
    int MinTotalMoves();
}

readonly struct Card : IEquatable<Card>
{
    private readonly sbyte _card;
    public Card(int card) { _card = (sbyte)card; }
    public Card(Suit suit, Rank rank) { _card = (sbyte)((int)suit + ((int)rank << 2)); }
    public int RawValue => _card;
    public Suit Suit => (Suit)(_card & 3);
    public Rank Rank => (Rank)(_card >> 2);
    public CardColor Color => (Suit == Suit.SPADE || Suit == Suit.CLUB) ? CardColor.BLACK : CardColor.RED;
    public bool IsMajor() => Suit == Suit.SPADE || Suit == Suit.HEART;
    public bool IsAbove(Card foundationTop) => Suit == foundationTop.Suit && Rank == foundationTop.Rank + 1;
    public bool IsBelow(Card tableauTop) => Color != tableauTop.Color && Rank + 1 == tableauTop.Rank;
    public bool Equals(Card other) => _card == other._card;
    public override bool Equals(object obj) => obj is Card other && Equals(other);
    public override int GetHashCode() => _card;
    public static bool operator ==(Card left, Card right) => left.Equals(right);
    public static bool operator !=(Card left, Card right) => !left.Equals(right);
    public override string ToString() { if (RawValue == -1) return "--"; char r = "A23456789TJQK"[(int)Rank]; char s = "SHDC"[(int)Suit]; return $"{r}{s}"; }
}

class BitStream
{
    private readonly uint[] _stream; private int _writeChunk, _writtenChunkBits; private const int ChunkBits = 32;
    public BitStream(int maxBits) { _stream = new uint[(maxBits + 31) >> 5]; }
    public BitStream(BitStream other) { _stream = (uint[])other._stream.Clone(); _writeChunk = other._writeChunk; _writtenChunkBits = other._writtenChunkBits; }
    private static int BitsNeededFor(int total) => total <= 1 ? 1 : 32 - BitOperations.LeadingZeroCount((uint)total - 1);
    public void Write(int part, int total) => WriteBits((uint)part, BitsNeededFor(total));
    private void WriteBits(uint value, int bits) { var freeChunkBits = ChunkBits - _writtenChunkBits; if (freeChunkBits >= bits) { _stream[_writeChunk] |= value << _writtenChunkBits; _writtenChunkBits += bits; } else { if (freeChunkBits > 0) _stream[_writeChunk] |= value << _writtenChunkBits; _stream[++_writeChunk] = value >> freeChunkBits; _writtenChunkBits = bits - freeChunkBits; } }
    public class Reader
    {
        private readonly uint[] _stream; private int _readChunk, _readChunkBits;
        public Reader(BitStream bs) { _stream = bs._stream; }
        public int Read(int total) => (int)ReadBits(BitsNeededFor(total));
        private uint ReadBits(int bits) { uint value; var availChunkBits = ChunkBits - _readChunkBits; if (availChunkBits >= bits) { value = _stream[_readChunk] >> _readChunkBits; _readChunkBits += bits; } else { value = availChunkBits > 0 ? _stream[_readChunk] >> _readChunkBits : 0; value |= _stream[++_readChunk] << availChunkBits; _readChunkBits = bits - availChunkBits; } return value & ((1U << bits) - 1); }
    }
}

class Foundation
{
    private sbyte _size; public Foundation() { _size = 0; } public Foundation(Foundation other) { _size = other._size; }
    public int Size => _size; public void Push(Card card) => ++_size; public bool Accepting(Card card) => card.Rank == (Rank)_size; public bool Has(Card card) => _size >= 1 && card.Rank <= (Rank)(_size - 1);
}

class Tableau
{
    private byte _index, _unsortedSize, _sortedSize; private ushort _stack; private Card _top;
    public static Card[][] InitTableau = new Card[8][];
    public Tableau() { }
    public Tableau(Tableau other) { _index = other._index; _unsortedSize = other._unsortedSize; _sortedSize = other._sortedSize; _stack = other._stack; _top = other._top; }
    public void SetCards(int index, int size) { _index = (byte)index; _unsortedSize = (byte)size; _sortedSize = 0; RecountSorted(); }
    public int Size() => _unsortedSize + _sortedSize; public int UnsortedSize() => _unsortedSize; public int SortedSize() => _sortedSize; public bool IsEmpty() => Size() == 0; public Card Top() => _top;
    public void Push(Card card) { if (_sortedSize++ > 0) PushTopToStack(); _top = card; }
    public int Pop() { if (_sortedSize > 1) { _top = GetCard(Size() - 2); _stack >>= 1; _sortedSize--; return 0; } _sortedSize--; return RecountSorted(); }
    public int Move(Tableau to, int superMoveSize = 999)
    {
        var count = CountMovable(to); if (count > superMoveSize) count = superMoveSize; if (!to.IsEmpty()) to.PushTopToStack();
        if (count > 1) to._stack = (ushort)((to._stack << (count - 1)) | (_stack & ((1 << (count - 1)) - 1)));
        to._top = _top; to._sortedSize += (byte)count;
        _top = Size() > count ? GetCard(Size() - count - 1) : new Card(-1);
        _stack >>= count; _sortedSize -= (byte)count;
        return RecountSorted();
    }
    public int CountMovable(Tableau target) { if (IsEmpty()) return 0; if (target.IsEmpty()) return _sortedSize; var lead = target.Top(); var top = Top(); var rankDiff = (int)lead.Rank - (int)top.Rank; if (rankDiff <= 0 || _sortedSize < rankDiff) return 0; if ((rankDiff & 1) == (top.Color == lead.Color ? 1 : 0)) return 0; return rankDiff; }
    public bool Accepting(Card card) => IsEmpty() || card.IsBelow(Top());
    public List<string> GetCardStrings()
    {
        var list = new List<string>(Size());
        for (int i = 0; i < Size(); i++)
        {
            list.Add(GetCard(i).ToString());
        }
        return list;
    }
    private Card GetCard(int i) { if (i < _unsortedSize) return InitTableau[_index][i]; if (i == Size() - 1) return _top; var depth = Size() - 2 - i; var isMajor = (_stack & (1 << depth)) != 0; var color = (depth & 1) != 0 ? _top.Color : (1 - _top.Color); var suit = isMajor ? (Suit)color : (Suit)(3 - (int)color); return new Card(suit, (Rank)((int)_top.Rank + depth + 1)); }
    private void PushTopToStack() => _stack = (ushort)((_stack << 1) | (_top.IsMajor() ? 1 : 0));
    private int RecountSorted()
    {
        if (_sortedSize > 0 || _unsortedSize == 0) return 0; 
        if(Size() == 0) return 0;
        _top = InitTableau[_index][Size() - 1]; _stack = 0;
        var numSorted = 1; while (numSorted < Size()) { var i = Size() - numSorted; if (!GetCard(i).IsBelow(GetCard(i - 1))) break; numSorted++; }
        for (var i = Size() - numSorted; i < Size() - 1; ++i) _stack = (ushort)((_stack << 1) | (GetCard(i).IsMajor() ? 1 : 0));
        _sortedSize += (byte)numSorted;
        _unsortedSize -= (byte)numSorted;
        return numSorted;
    }
    public bool Equals(Tableau t) { if (_unsortedSize != t._unsortedSize || _sortedSize != t._sortedSize) return false; if (_sortedSize == 0) return true; if (_top != t._top) return false; return _stack == t._stack; }
}

readonly struct Move
{
    public readonly MoveType Type; public readonly byte From; public readonly byte To;
    public Move(MoveType type, int from, int to = 0) { Type = type; From = (byte)from; To = (byte)to; }
    public string Encode() => Type switch
    {
        MoveType.TableauToFoundation => $"{(char)('0' + From)}h",
        MoveType.TableauToReserve => $"{(char)('0' + From)}{(char)('a' + To)}",
        MoveType.TableauToTableau => $"{(char)('0' + From)}{(char)('0' + To)}",
        MoveType.ReserveToFoundation => $"{(char)('a' + From)}h",
        MoveType.ReserveToTableau => $"{(char)('a' + From)}{(char)('0' + To)}",
        _ => ""
    };
}

class FreecellNode : IBeamSearchNode<FreecellNode>
{
    public FreecellNode After { get; set; } public FreecellNode Prev { get; set; } public FreecellNode Next { get; set; }
    public BitStream Moves { get; private set; } public Move LastMove { get; private set; } public int MovesPerformed => _movesPerformed; public uint Hash => _hash;
    private Foundation[] _foundations; private Tableau[] _tableaux; private Card[] _reserve;
    private byte _cardsUnsorted, _movesPerformed, _movesEstimated, _autoPlays; private uint _hash;
    private static List<uint> _reserveRand; private static List<List<uint>> _tableauUnsortedRand, _tableauSortedRand, _tableauTopRand;
    public FreecellNode()
    {
        _foundations = new Foundation[4]; for (var i = 0; i < 4; i++) _foundations[i] = new Foundation();
        _tableaux = new Tableau[8]; for (var i = 0; i < 8; i++) _tableaux[i] = new Tableau();
        _reserve = new Card[4];
        Array.Fill(_reserve, new Card(-1));
        Moves = new BitStream(512);
    }
    public void CopyFrom(FreecellNode other)
    {
        for (var i = 0; i < 4; i++) _foundations[i] = new Foundation(other._foundations[i]);
        for (var i = 0; i < 8; i++) _tableaux[i] = new Tableau(other._tableaux[i]);
        _reserve = (Card[])other._reserve.Clone();
        _cardsUnsorted = other._cardsUnsorted; _movesPerformed = other._movesPerformed; _movesEstimated = other._movesEstimated; _autoPlays = other._autoPlays; _hash = other._hash; LastMove = other.LastMove; Moves = new BitStream(other.Moves);
    }
    public void SetCards(List<Card> cards)
    {
        Tableau.InitTableau = new Card[8][];
        for (var i = 0; i < 8; i++) Tableau.InitTableau[i] = new Card[i < 4 ? 7 : 6];
        for (var i = 0; i < cards.Count; i++) Tableau.InitTableau[i % 8][i / 8] = cards[i];
        for (var i = 0; i < 8; i++) _tableaux[i].SetCards(i, i < 4 ? 7 : 6);
        _cardsUnsorted = 0;
        for (var i = 0; i < 8; i++) _cardsUnsorted += (byte)_tableaux[i].UnsortedSize();
        _movesPerformed = 0; _movesEstimated = 52;
    }
    public void InitializeFromFullState(List<List<string>> tableaux, string foundationsLine, string cellsLine)
    {
        _movesEstimated = 52;
        var foundationParts = foundationsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1);
        foreach (var part in foundationParts)
        {
            if (part.Length == 3 && part[1] == '-')
            {
                Suit suit = part[0] switch { 'S' => Suit.SPADE, 'H' => Suit.HEART, 'D' => Suit.DIAMOND, _ => Suit.CLUB };
                Rank rank = part[2] switch { 'A' => Rank.ACE, 'T' => Rank.R10, 'J' => Rank.RJ, 'Q' => Rank.RQ, 'K' => Rank.KING, _ => (Rank)(part[2] - '2' + 1) };
                int count = (int)rank + 1;
                for (int i = 0; i < count; i++) _foundations[(int)suit].Push(new Card());
                _movesEstimated -= (byte)count;
            }
        }

        Array.Fill(_reserve, new Card(-1));
        var cellTokens = cellsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1);
        int reserveIdx = 0;
        foreach (var token in cellTokens)
        {
            if (token != "-") _reserve[reserveIdx] = ParseCard(token);
            if(reserveIdx < 3) reserveIdx++;
        }

        _cardsUnsorted = 0;
        Tableau.InitTableau = new Card[8][];
        for (int i = 0; i < 8; i++)
        {
            var currentColumnCards = tableaux[i].Select(ParseCard).ToList();
            Tableau.InitTableau[i] = currentColumnCards.ToArray();
            _tableaux[i].SetCards(i, currentColumnCards.Count);
            _cardsUnsorted += (byte)_tableaux[i].UnsortedSize();
        }

        _movesPerformed = 0;
        AutoPlay();
    }
    
    private Card ParseCard(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length < 2) return new Card(-1);
        Rank rank = s[0] switch
        {
            'A' => Rank.ACE, 'T' => Rank.R10, 'J' => Rank.RJ, 'Q' => Rank.RQ, 'K' => Rank.KING,
            _ => (Rank)(s[0] - '2' + 1)
        };
        Suit suit = s[1] switch
        {
            'S' => Suit.SPADE, 'H' => Suit.HEART, 'D' => Suit.DIAMOND, 'C' => Suit.CLUB,
            _ => throw new ArgumentException()
        };
        return new Card(suit, rank);
    }
    public static void Initialize()
    {
        var rand = new Random(12345); InitializeHashRand(52, out _reserveRand, rand);
        _tableauUnsortedRand = new List<List<uint>>(8); _tableauSortedRand = new List<List<uint>>(8); _tableauTopRand = new List<List<uint>>(8);
        for (var i = 0; i < 8; ++i) { InitializeHashRand(8, out var u, rand); _tableauUnsortedRand.Add(u); InitializeHashRand(14, out var s, rand); _tableauSortedRand.Add(s); InitializeHashRand(52, out var t, rand); _tableauTopRand.Add(t); }
    }
    private static void InitializeHashRand(int count, out List<uint> target, Random rand) { target = new List<uint>(count); for (var i = 0; i < count; ++i) target.Add((uint)rand.Next() << 16 | (uint)rand.Next()); }
    public void ComputeHash()
    {
        _hash = 0;
        for (var i = 0; i < 4; ++i) if (_reserve[i].RawValue != -1) _hash += _reserveRand[_reserve[i].RawValue];
        for (var i = 0; i < 8; ++i) { _hash += _tableauUnsortedRand[i][_tableaux[i].UnsortedSize()]; _hash += _tableauSortedRand[i][_tableaux[i].SortedSize()]; if (!_tableaux[i].IsEmpty()) _hash += _tableauTopRand[i][_tableaux[i].Top().RawValue]; }
    }
    public int Bin() => _movesEstimated + _cardsUnsorted + _reserve.Count(c => c.RawValue != -1);
    public int MinTotalMoves() => _movesPerformed + _movesEstimated;
    public bool IsGoal() => _cardsUnsorted == 0 && _reserve.All(c => c.RawValue == -1);
    public void Expand(Pool<FreecellNode> pool, List<FreecellNode> outNodes)
    {
        for (var r = 0; r < 4; r++)
        {
            if (_reserve[r].RawValue == -1) continue;
            var card = _reserve[r];
            if (AllowReserveToFoundation())
            {
                var f = FindFoundation(card);
                if (f != -1 && AllowReserveToFoundation(f))
                {
                    outNodes.Add(pool.New(this).ReserveToFoundation(r).AutoPlay());
                }
            }
            var triedEmpty = false;
            for (var t = 0; t < 8; t++)
            {
                if (_tableaux[t].Accepting(card) && AllowReserveToTableau(r, t))
                {
                    if (!_tableaux[t].IsEmpty() || !triedEmpty)
                    {
                        outNodes.Add(pool.New(this).ReserveToTableau(r, t).AutoPlay());
                        if (_tableaux[t].IsEmpty()) triedEmpty = true;
                    }
                }
            }
        }
        for (var i = 0; i < 8; i++)
        {
            if (_tableaux[i].IsEmpty()) continue;
            if (AllowTableauToFoundation())
            {
                var f = FindFoundation(_tableaux[i].Top());
                if (f != -1 && AllowTableauToFoundation(i, f))
                {
                    outNodes.Add(pool.New(this).TableauToFoundation(i).AutoPlay());
                }
            }
            for (var j = 0; j < 8; j++)
            {
                if (i == j || !AllowTableauToTableau(i, j)) continue;
                var count = _tableaux[i].CountMovable(_tableaux[j]);
                if (count > 0)
                {
                    if (_tableaux[j].IsEmpty() || count <= MaxSuperMoveSize(i, j))
                    {
                        outNodes.Add(pool.New(this).TableauToTableau(i, j).AutoPlay());
                    }
                }
            }
            int freeSlot = Array.IndexOf(_reserve, new Card(-1));
            if (freeSlot != -1 && AllowTableauToReserve(i))
            {
                outNodes.Add(pool.New(this).TableauToReserve(i, freeSlot).AutoPlay());
            }
        }
        EncodeMoves(outNodes);
    }
    public void DebugState(string title)
    {
        if (!Solution.EnableDebug) return;
        var sb = new StringBuilder();
        sb.AppendLine($"--- {title} ---");
        sb.AppendLine($"Moves: {MovesPerformed}, Estimated: {_movesEstimated}, MinTotal: {MinTotalMoves()}, Unsorted: {_cardsUnsorted}, Reserve: {_reserve.Count(c => c.RawValue != -1)}, Auto: {_autoPlays}");
        sb.Append("Foundations:");
        char[] suitChars = { 'S', 'H', 'D', 'C' };
        for (int i = 0; i < 4; i++)
        {
            sb.Append($" {suitChars[i]}:");
            if (_foundations[i].Size > 0)
            {
                var rank = (Rank)(_foundations[i].Size - 1);
                char r = "A23456789TJQK"[(int)rank];
                sb.Append(r);
            }
            else
            {
                sb.Append("-");
            }
        }
        sb.AppendLine();
        sb.Append("Reserve:    ");
        for (int i = 0; i < 4; i++)
        {
            if (_reserve[i].RawValue != -1) sb.Append($"[{_reserve[i],-2}] ");
            else sb.Append("[  ] ");
        }
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Tableaux:");
        var tableauCardStrings = new List<List<string>>(8);
        int maxRows = 0;
        for (int i = 0; i < 8; i++)
        {
            var cardStrings = _tableaux[i].GetCardStrings();
            tableauCardStrings.Add(cardStrings);
            if (cardStrings.Count > maxRows) maxRows = cardStrings.Count;
        }
        for (int i = 0; i < 8; i++) sb.Append($"   {i + 1}   ");
        sb.AppendLine();
        for (int i = 0; i < 8; i++) sb.Append("-------");
        sb.AppendLine();
        for (int r = 0; r < maxRows; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (r < tableauCardStrings[c].Count) sb.Append($"  {tableauCardStrings[c][r],-3}  ");
                else sb.Append("       ");
            }
            sb.AppendLine();
        }
        sb.AppendLine("--------------------------------------------------------");
        Solution.Debug(sb.ToString());
    }
    public FreecellNode AutoPlay()
    {
        bool changed;
        do
        {
            changed = false;
            for (var r = 0; r < 4; r++)
            {
                if (_reserve[r].RawValue != -1 && CanAutoPlay(_reserve[r]))
                {
                    ReserveToFoundation(r, true);
                    _autoPlays++;
                    changed = true;
                }
            }
            for (var t = 0; t < 8; t++)
            {
                if (!_tableaux[t].IsEmpty() && CanAutoPlay(_tableaux[t].Top()))
                {
                    TableauToFoundation(t, true);
                    _autoPlays++;
                    changed = true;
                }
            }
        } while (changed);
        return this;
    }
    private void EncodeMoves(List<FreecellNode> newNodes) { var index = 0; foreach (var node in newNodes) node.Moves.Write(index++, newNodes.Count); }
    private int FindFoundation(Card card) { var suit = (int)card.Suit; return _foundations[suit].Accepting(card) ? suit : -1; }
    private int MaxSuperMoveSize(int from, int to) { var freeCells = _reserve.Count(c => c.RawValue == -1); if (!_tableaux[to].IsEmpty()) return freeCells + 1; int emptyTableaus = 0; for (int t = 0; t < 8; t++) if (t != from && t != to && _tableaux[t].IsEmpty()) emptyTableaus++; return (freeCells + 1) * (1 << emptyTableaus); }
    private bool AllowReserveToFoundation() => LastMove.Type != MoveType.ReserveToTableau && LastMove.Type != MoveType.TableauToTableau && LastMove.Type != MoveType.TableauToReserve;
    private bool AllowReserveToFoundation(int f) => !((LastMove.Type == MoveType.ReserveToFoundation && f != LastMove.To)) && !((LastMove.Type == MoveType.TableauToFoundation && f != LastMove.To));
    private bool AllowTableauToFoundation() => LastMove.Type != MoveType.ReserveToTableau;
    private bool AllowTableauToFoundation(int t, int f) => !(LastMove.Type == MoveType.TableauToFoundation && LastMove.To != f && LastMove.From > t) && !(LastMove.Type == MoveType.TableauToTableau && t != LastMove.From && t != LastMove.To) && !(LastMove.Type == MoveType.TableauToReserve && LastMove.From != t);
    private bool AllowReserveToTableau(int r, int t) => !(LastMove.Type == MoveType.TableauToTableau && t != LastMove.From && t != LastMove.To) && !(LastMove.Type == MoveType.TableauToReserve && r == LastMove.To && t == LastMove.From);
    private bool AllowTableauToTableau(int t1, int t2) => !(LastMove.Type == MoveType.TableauToReserve && LastMove.From != t1 && LastMove.From != t2);
    private bool AllowTableauToReserve(int t) => !(LastMove.Type == MoveType.TableauToReserve && LastMove.From > t) && !(LastMove.Type == MoveType.ReserveToTableau && LastMove.To == t);
    private bool CanAutoPlay(Card card) { if (!_foundations[(int)card.Suit].Accepting(card)) return false; if (card.Rank <= Rank.R2) return true; var requiredRank = (Rank)((int)card.Rank - 1); if (card.Color == CardColor.RED) return _foundations[(int)Suit.SPADE].Has(new Card(Suit.SPADE, requiredRank)) && _foundations[(int)Suit.CLUB].Has(new Card(Suit.CLUB, requiredRank)); else return _foundations[(int)Suit.HEART].Has(new Card(Suit.HEART, requiredRank)) && _foundations[(int)Suit.DIAMOND].Has(new Card(Suit.DIAMOND, requiredRank)); }
    public FreecellNode ReserveToFoundation(int r, bool autoPlay = false) { var card = _reserve[r]; _reserve[r] = new Card(-1); _foundations[(int)card.Suit].Push(card); _movesEstimated--; if (!autoPlay) { LastMove = new Move(MoveType.ReserveToFoundation, r, (int)card.Suit); _movesPerformed++; } return this; }
    public FreecellNode ReserveToTableau(int r, int t) { var card = _reserve[r]; _reserve[r] = new Card(-1); _tableaux[t].Push(card); LastMove = new Move(MoveType.ReserveToTableau, r, t); _movesPerformed++; return this; }
    public FreecellNode TableauToFoundation(int t, bool autoPlay = false) { var card = _tableaux[t].Top(); _foundations[(int)card.Suit].Push(card); _cardsUnsorted -= (byte)_tableaux[t].Pop(); _movesEstimated--; if (!autoPlay) { LastMove = new Move(MoveType.TableauToFoundation, t, (int)card.Suit); _movesPerformed++; } return this; }
    public FreecellNode TableauToReserve(int t, int reserveSlot) { var card = _tableaux[t].Top(); _reserve[reserveSlot] = card; _cardsUnsorted -= (byte)_tableaux[t].Pop(); LastMove = new Move(MoveType.TableauToReserve, t, reserveSlot); _movesPerformed++; return this; }
    public FreecellNode TableauToTableau(int s, int t) { var newSorted = _tableaux[s].Move(_tableaux[t], MaxSuperMoveSize(s, t)); _cardsUnsorted -= (byte)newSorted; LastMove = new Move(MoveType.TableauToTableau, s, t); _movesPerformed++; return this; }
    public bool StateEquals(FreecellNode n) { if (!_reserve.SequenceEqual(n._reserve)) return false; for (var f = 0; f < 4; ++f) if (_foundations[f].Size != n._foundations[f].Size) return false; for (var t = 0; t < 8; ++t) if (_tableaux[t].SortedSize() != n._tableaux[t].SortedSize() || _tableaux[t].Size() != n._tableaux[t].Size()) return false; for (var t = 0; t < 8; ++t) if (!_tableaux[t].Equals(n._tableaux[t])) return false; return true; }
}

class Pool<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    private readonly Stack<TNode> _pool = new Stack<TNode>(16384);
    public TNode New() => _pool.Count > 0 ? _pool.Pop() : new TNode();
    public TNode New(TNode other) { var node = New(); node.CopyFrom(other); return node; }
    public void Delete(TNode node) { if (node != null) _pool.Push(node); }
}

class Bucket<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    private TNode[] _bins; public int Min { get; private set; } public int Max { get; private set; } public int Count { get; private set; }
    public Bucket(int numBins) { _bins = new TNode[numBins]; Clear(); }
    public void Clear() { Array.Fill(_bins, null); Min = _bins.Length + 1; Max = -1; Count = 0; }
    public void Add(TNode node, int index) { node.After = _bins[index]; _bins[index] = node; if (node.After == null) { if (Min > index) Min = index; if (Max < index) Max = index; } Count++; }
    public TNode RemoveMax() { var node = _bins[Max]; _bins[Max] = node.After; if (node.After == null) FindNewMax(); Count--; return node; }
    private void FindNewMax() { while (Min <= Max && _bins[Max] == null) Max--; }
    public void Iterate(Action<TNode> work) { if (Count == 0) return; for (var i = Min; i <= Max; ++i) for (var c = _bins[i]; c != null; c = c.After) work(c); }
}

class HashTable<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    private TNode[] _bins; private int _binMask; public int Count { get; private set; }
    public HashTable(int numBins) { _bins = new TNode[numBins]; _binMask = numBins - 1; }
    public TNode Find(TNode node) { var index = node.Hash & _binMask; for (var c = _bins[index]; c != null; c = c.Next) if (node.Hash == c.Hash && node.StateEquals(c)) return c; return null; }
    public void Add(TNode node) { var index = node.Hash & _binMask; var next = _bins[index]; node.Next = next; node.Prev = null; if (next != null) next.Prev = node; _bins[index] = node; Count++; }
    public void Remove(TNode node) { var p = node.Prev; var n = node.Next; if (p != null) p.Next = n; else _bins[node.Hash & _binMask] = n; if (n != null) n.Prev = p; Count--; }
}

class Beam<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    private readonly int _beamSize; private int _upperbound; private List<Bucket<TNode>> _levels; private HashTable<TNode> _hashTable; private Pool<TNode> _pool = new Pool<TNode>(); private readonly int _maxMoves;
    public Beam(int beamSize, int maxMoves) { _beamSize = beamSize; _maxMoves = maxMoves; _upperbound = maxMoves + 1; _levels = new List<Bucket<TNode>>(maxMoves); for (var i = 0; i < maxMoves; ++i) _levels.Add(new Bucket<TNode>(maxMoves * 2)); _hashTable = new HashTable<TNode>(beamSize * 2); }
    public TNode BeamSearch(TNode layout)
    {
        var root = _pool.New(layout); root.ComputeHash(); _levels[0].Add(root, root.Bin()); _hashTable.Add(root);
        TNode solution = null;
        for (var i = 0; i < _levels.Count - 1; ++i)
        {
            if (_levels[i].Count == 0) break;
            var newSolution = CreateNewLevel(_levels[i], _levels[i + 1]);
            if (newSolution != null) { if (solution != null) _pool.Delete(solution); solution = newSolution; }
            if (i >= 1) { _levels[i - 1].Iterate(node => { _hashTable.Remove(node); _pool.Delete(node); }); _levels[i - 1].Clear(); }
        }
        return solution;
    }
    private TNode CreateNewLevel(Bucket<TNode> curLevel, Bucket<TNode> newLevel)
    {
        TNode solution = null;
        var newNodesBuffer = new List<TNode>(64);
        curLevel.Iterate(node => { if (node.MovesPerformed >= _upperbound - 1) return; newNodesBuffer.Clear(); node.Expand(_pool, newNodesBuffer); if (newNodesBuffer.Count == 0) return; foreach (var n in newNodesBuffer) n.ComputeHash(); TNode found = ProcessNewNodes(newNodesBuffer, newLevel); if (found != null) { if (solution != null) _pool.Delete(solution); solution = found; } });
        return solution;
    }
    private TNode ProcessNewNodes(List<TNode> newNodes, Bucket<TNode> newLevel)
    {
        TNode solution = null;
        foreach (var newNode in newNodes)
        {
            if (newNode.MinTotalMoves() >= _upperbound) { _pool.Delete(newNode); continue; }
            if (newNode.IsGoal() && newNode.MinTotalMoves() < _upperbound) { if (solution != null) _pool.Delete(solution); solution = newNode; _upperbound = solution.MinTotalMoves(); continue; }
            var existing = _hashTable.Find(newNode);
            if (existing != null) { _pool.Delete(newNode); continue; }
            if (newLevel.Count < _beamSize) { newLevel.Add(newNode, newNode.Bin()); _hashTable.Add(newNode); }
            else if (newNode.Bin() <= newLevel.Max) { var maxNode = newLevel.RemoveMax(); _hashTable.Remove(maxNode); _pool.Delete(maxNode); newLevel.Add(newNode, newNode.Bin()); _hashTable.Add(newNode); }
            else { _pool.Delete(newNode); }
        }
        return solution;
    }

    public string EncodeSolution(TNode start, TNode finish)
    {
        var result = new StringBuilder(); var reader = new BitStream.Reader(finish.Moves); var node = _pool.New(start); var newNodesBuffer = new List<TNode>(64);
        for (var i = 0; i < finish.MovesPerformed; i++)
        {
            newNodesBuffer.Clear(); node.Expand(_pool, newNodesBuffer); var moveIndex = reader.Read(newNodesBuffer.Count);
            for (var j = 0; j < newNodesBuffer.Count; j++) if (j != moveIndex) _pool.Delete(newNodesBuffer[j]);
            _pool.Delete(node); node = newNodesBuffer[moveIndex]; result.Append(node.LastMove.Encode());
        }
        _pool.Delete(node); return result.ToString();
    }

    public List<TNode> ReconstructSolutionPath(TNode start, TNode finish)
    {
        if (finish == null) return new List<TNode>();
        var path = new List<TNode>(finish.MovesPerformed + 1);
        var reader = new BitStream.Reader(finish.Moves);
        var node = _pool.New(start);
        var newNodesBuffer = new List<TNode>(64);
        path.Add(_pool.New(node));
        for (var i = 0; i < finish.MovesPerformed; i++)
        {
            newNodesBuffer.Clear();
            node.Expand(_pool, newNodesBuffer);
            var moveIndex = reader.Read(newNodesBuffer.Count);
            for (var j = 0; j < newNodesBuffer.Count; j++)
            {
                if (j != moveIndex) _pool.Delete(newNodesBuffer[j]);
            }
            _pool.Delete(node);
            node = newNodesBuffer[moveIndex];
            path.Add(_pool.New(node));
        }
        _pool.Delete(node);
        return path;
    }
}