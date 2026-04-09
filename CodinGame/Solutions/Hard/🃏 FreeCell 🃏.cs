using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        node.SetCards(GetDeal(tableaux));
        (var plan, var solutionPath) = Solve(node);
        var planIndex = 0;
        var tactical = new List<string>();
        var turn = 1;
        if (solutionPath.Count > 0) solutionPath[0].DebugState($"Planner's Expected State for Turn 1");
        PlayMove(moves, ref plan, ref solutionPath, ref planIndex, tactical, ref turn);
        while (true)
        {
            if (Console.ReadLine() == null) break;
            Console.ReadLine();
            var foundationsLine = Console.ReadLine();
            var cellsLine = Console.ReadLine();
            var actualTableaux = new List<List<string>>(8);
            for (var i = 0; i < 8; i++) actualTableaux.Add(Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList());
            Console.ReadLine();
            var movesLine = Console.ReadLine();
            DebugActualState(turn, foundationsLine, cellsLine, actualTableaux);
            if (solutionPath != null && solutionPath.Count > turn - 1) solutionPath[turn - 1].DebugState($"Planner's Expected State for Turn {turn}");
            var currentMoves = ParseMovesLine(movesLine);
            PlayMove(currentMoves, ref plan, ref solutionPath, ref planIndex, tactical, ref turn, actualTableaux, foundationsLine, cellsLine);
        }
    }
    private static List<string> ReadFirstInput(List<List<string>> tableaux)
    {
        for (var i = 0; i < 4; i++) Console.ReadLine();
        for (var i = 0; i < 8; i++) tableaux.Add(Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList());
        Console.ReadLine();
        var moves = ParseMovesLine(Console.ReadLine());
        Debug($"Initial possible moves: {string.Join(" ", moves)}");
        return moves;
    }
    private static List<string> ParseMovesLine(string line) => new List<string>(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    private static List<Card> GetDeal(List<List<string>> tableaux)
    {
        var cards = new List<Card>(52);
        var maxRows = tableaux.Any() ? tableaux.Max(t => t.Count) : 0;
        for (var r = 0; r < maxRows; r++)
            for (var c = 0; c < 8; c++)
                if (c < tableaux.Count && r < tableaux[c].Count)
                    cards.Add(CardHelper.Parse(tableaux[c][r]));
        return cards;
    }
    private static (string, List<FreecellNode>) Solve(FreecellNode startNode)
    {
        var beam = new Beam<FreecellNode>(512, 128);
        var stopwatch = Stopwatch.StartNew();
        var solutionNode = beam.BeamSearch(startNode);
        stopwatch.Stop();
        Debug($"Solver time: {stopwatch.ElapsedMilliseconds} ms");
        if (solutionNode == null)
        {
            Debug("Solver failed to find a solution.");
            return (string.Empty, new List<FreecellNode>());
        }
        var plan = beam.EncodeSolution(solutionNode);
        var path = beam.ReconstructSolutionPath(startNode, solutionNode);
        Debug($"New plan generated: {plan}");
        return (plan, path);
    }
    private static void DebugActualState(int turn, string foundationsLine, string cellsLine, List<List<string>> tableaux)
    {
        if (!EnableDebug) return;
        var sb = new StringBuilder();
        sb.AppendLine($"--- ACTUAL GAME STATE - TURN {turn} ---");
        var foundationRanks = new Dictionary<char, char> { { 'S', '-' }, { 'H', '-' }, { 'D', '-' }, { 'C', '-' } };
        var foundationParts = foundationsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (foundationParts.Length > 1) foreach (var part in foundationParts.Skip(1)) if (part.Length == 3 && part[1] == '-') foundationRanks[part[0]] = part[2];
        sb.Append("Foundations:");
        sb.Append($" S:{foundationRanks['S']} H:{foundationRanks['H']} D:{foundationRanks['D']} C:{foundationRanks['C']}");
        sb.AppendLine();
        var cellTokens = cellsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
        sb.Append("Reserve:    ");
        for (int i = 0; i < 4; i++) sb.Append(i < cellTokens.Length && cellTokens[i] != "-" ? $"[{cellTokens[i],-2}] " : "[  ] ");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Tableaux:");
        int maxRows = tableaux.Any() ? tableaux.Max(t => t.Count) : 0;
        for (int i = 0; i < 8; i++) sb.Append($"   {i + 1}   ");
        sb.AppendLine();
        for (int i = 0; i < 8; i++) sb.Append("-------");
        sb.AppendLine();
        for (int r = 0; r < maxRows; r++)
        {
            for (int c = 0; c < 8; c++) sb.Append(c < tableaux.Count && r < tableaux[c].Count ? $"  {tableaux[c][r],-3}  " : "       ");
            sb.AppendLine();
        }
        sb.AppendLine("--------------------------------------------------------");
        Debug(sb.ToString());
    }
    private static void PlayMove(List<string> moves, ref string plan, ref List<FreecellNode> solutionPath, ref int planIndex, List<string> tactical, ref int turn, List<List<string>> currentTableaux = null, string foundationsLine = null, string cellsLine = null)
    {
        Debug($"\nTURN {turn}");
        Debug($"Current possible moves: {string.Join(" ", moves)}");
        Debug($"PlanIndex={planIndex} / PlanLen={plan.Length}");
        var replan = false;
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
        else if (planIndex + 1 < plan.Length)
        {
            var mapped = MapMoveToOutput(plan[planIndex], plan[planIndex + 1]);
            Debug($"[STRATEGY] PlanMove='{plan[planIndex]}{plan[planIndex + 1]}' mapped to '{mapped}'");
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
            if (currentTableaux == null || moves.Count == 0 || foundationsLine == null || cellsLine == null)
            {
                var fallback = moves.Count > 0 ? moves[0] : "1h";
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
                if (plan.Length == 0)
                {
                    Debug("[REPLAN-FAIL] Solver could not find a solution from the current state. Playing fallback.");
                    Console.WriteLine(moves.Count > 0 ? moves[0] : "1h");
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
    private static string MapMoveToOutput(char from, char to)
    {
        char a = char.IsDigit(from) ? (char)(from + 1) : from;
        char b = char.IsDigit(to) ? (char)(to + 1) : to == 'h' ? 'h' : to;
        return $"{a}{b}";
    }
    public static void Debug(string msg) { if (EnableDebug) Console.Error.WriteLine("[DEBUG] " + msg); }
}
enum Suit { SPADE, HEART, DIAMOND, CLUB }
enum CardColor { BLACK, RED }
enum Rank { ACE, R2, R3, R4, R5, R6, R7, R8, R9, R10, RJ, RQ, KING }
enum MoveType { None, TableauToFoundation, TableauToReserve, TableauToTableau, ReserveToFoundation, ReserveToTableau }
static class CardHelper
{
    public static Card Parse(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length < 2) return new Card(-1);
        var rank = s[0] switch { 'A' => Rank.ACE, 'T' => Rank.R10, 'J' => Rank.RJ, 'Q' => Rank.RQ, 'K' => Rank.KING, _ => (Rank)(s[0] - '2' + 1) };
        var suit = s[1] switch { 'S' => Suit.SPADE, 'H' => Suit.HEART, 'D' => Suit.DIAMOND, 'C' => Suit.CLUB, _ => throw new ArgumentException() };
        return new Card(suit, rank);
    }
    public static char RankToChar(int rank) => "A23456789TJQK"[rank];
}
readonly struct Card : IEquatable<Card>
{
    private readonly sbyte _card;
    public Card(int card) => _card = (sbyte)card;
    public Card(Suit suit, Rank rank) => _card = (sbyte)((int)suit + ((int)rank << 2));
    public int RawValue => _card;
    public Suit Suit => (Suit)(_card & 3);
    public Rank Rank => (Rank)(_card >> 2);
    public CardColor Color => Suit == Suit.SPADE || Suit == Suit.CLUB ? CardColor.BLACK : CardColor.RED;
    public bool IsMajor() => Suit == Suit.SPADE || Suit == Suit.HEART;
    public bool IsAbove(Card foundationTop) => Suit == foundationTop.Suit && Rank == foundationTop.Rank + 1;
    public bool IsBelow(Card tableauTop) => Color != tableauTop.Color && Rank + 1 == tableauTop.Rank;
    public bool Equals(Card other) => _card == other._card;
    public override bool Equals(object obj) => obj is Card other && Equals(other);
    public override int GetHashCode() => _card;
    public static bool operator ==(Card left, Card right) => left.Equals(right);
    public static bool operator !=(Card left, Card right) => !left.Equals(right);
    public override string ToString() => RawValue == -1 ? "--" : $"{CardHelper.RankToChar((int)Rank)}{"SHDC"[(int)Suit]}";
}
struct Tableau
{
    private byte _index, _unsortedSize, _sortedSize;
    private ushort _stack;
    private Card _top;
    public static Card[][] InitTableau = new Card[8][];
    public void SetCards(int index, int size)
    {
        _index = (byte)index; _unsortedSize = (byte)size; _sortedSize = 0; _stack = 0; _top = new Card(-1);
        RecountSorted();
    }
    public int Size() => _unsortedSize + _sortedSize;
    public int UnsortedSize() => _unsortedSize;
    public int SortedSize() => _sortedSize;
    public bool IsEmpty() => Size() == 0;
    public Card Top() => _top;
    public void Push(Card card)
    {
        if (_sortedSize > 0) PushTopToStack();
        _sortedSize++;
        _top = card;
    }
    public int Pop()
    {
        if (_sortedSize > 1)
        {
            _top = GetCard(Size() - 2);
            _stack >>= 1;
            _sortedSize--;
            return 0;
        }
        _sortedSize--;
        return RecountSorted();
    }
    public int MoveTo(ref Tableau to, int superMoveSize = 999)
    {
        var count = Math.Min(CountMovable(in to), superMoveSize);
        if (!to.IsEmpty()) to.PushTopToStack();
        if (count > 1) to._stack = (ushort)((to._stack << (count - 1)) | (_stack & ((1 << (count - 1)) - 1)));
        to._top = _top;
        to._sortedSize += (byte)count;
        _top = Size() > count ? GetCard(Size() - count - 1) : new Card(-1);
        _stack >>= count;
        _sortedSize -= (byte)count;
        return RecountSorted();
    }
    public int CountMovable(in Tableau target)
    {
        if (IsEmpty()) return 0;
        if (target.IsEmpty()) return _sortedSize;
        var lead = target.Top();
        var rankDiff = (int)lead.Rank - (int)_top.Rank;
        if (rankDiff <= 0 || _sortedSize < rankDiff || (rankDiff & 1) == (_top.Color == lead.Color ? 1 : 0)) return 0;
        return rankDiff;
    }
    public bool Accepting(Card card) => IsEmpty() || card.IsBelow(Top());
    public List<string> GetCardStrings()
    {
        var list = new List<string>(Size());
        for (var i = 0; i < Size(); i++) list.Add(GetCard(i).ToString());
        return list;
    }
    public bool StateEquals(in Tableau other) => _unsortedSize == other._unsortedSize && _sortedSize == other._sortedSize && (_sortedSize == 0 || (_top == other._top && _stack == other._stack));
    private Card GetCard(int i)
    {
        if (i < _unsortedSize) return InitTableau[_index][i];
        if (i == Size() - 1) return _top;
        var depth = Size() - 2 - i;
        var isMajor = (_stack & (1 << depth)) != 0;
        var color = (depth & 1) != 0 ? _top.Color : (CardColor)(1 - (int)_top.Color);
        return new Card(isMajor ? (Suit)color : (Suit)(3 - (int)color), (Rank)((int)_top.Rank + depth + 1));
    }
    private void PushTopToStack() => _stack = (ushort)((_stack << 1) | (_top.IsMajor() ? 1 : 0));
    private int RecountSorted()
    {
        if (_sortedSize > 0 || _unsortedSize == 0) return 0;
        _top = InitTableau[_index][_unsortedSize - 1];
        _stack = 0;
        var numSorted = 1;
        while (numSorted < _unsortedSize && GetCard(_unsortedSize - numSorted).IsBelow(GetCard(_unsortedSize - numSorted - 1))) numSorted++;
        for (var i = _unsortedSize - numSorted; i < _unsortedSize - 1; i++) _stack = (ushort)((_stack << 1) | (GetCard(i).IsMajor() ? 1 : 0));
        _sortedSize = (byte)numSorted;
        _unsortedSize -= (byte)numSorted;
        return numSorted;
    }
}
readonly struct Move
{
    public readonly MoveType Type;
    public readonly byte From, To;
    public Move(MoveType type, int from, int to = 0) { Type = type; From = (byte)from; To = (byte)to; }
    public void EncodeTo(char[] chars, int index)
    {
        chars[index] = Type == MoveType.ReserveToFoundation || Type == MoveType.ReserveToTableau ? (char)('a' + From) : (char)('0' + From);
        chars[index + 1] = Type == MoveType.TableauToFoundation || Type == MoveType.ReserveToFoundation ? 'h' : Type == MoveType.TableauToReserve ? (char)('a' + To) : (char)('0' + To);
    }
}
interface IBeamSearchNode<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    TNode After { get; set; }
    TNode Prev { get; set; }
    TNode Next { get; set; }
    Move LastMove { get; }
    int MovesPerformed { get; }
    int PathId { get; set; }
    void CopyFrom(TNode other);
    void Expand(Pool<TNode> pool, List<TNode> outNodes);
    void ApplyMove(Move move);
    void ComputeHash();
    uint Hash { get; }
    bool StateEquals(TNode other);
    bool IsGoal();
    int Bin();
    int MinTotalMoves();
}
class FreecellNode : IBeamSearchNode<FreecellNode>
{
    public FreecellNode After { get; set; }
    public FreecellNode Prev { get; set; }
    public FreecellNode Next { get; set; }
    public Move LastMove { get; private set; }
    public int MovesPerformed => _movesPerformed;
    public uint Hash => _hash;
    public int PathId { get; set; }
    private readonly byte[] _foundations = new byte[4];
    private readonly Tableau[] _tableaux = new Tableau[8];
    private readonly Card[] _reserve = new Card[4];
    private byte _cardsUnsorted, _movesPerformed, _movesEstimated, _autoPlays, _reserveUsed;
    private uint _hash;
    private static uint[] _reserveRand;
    private static uint[][] _tableauUnsortedRand, _tableauSortedRand, _tableauTopRand;
    public FreecellNode() { for (var i = 0; i < 4; i++) _reserve[i] = new Card(-1); }
    public void CopyFrom(FreecellNode other)
    {
        Array.Copy(other._foundations, _foundations, 4);
        Array.Copy(other._tableaux, _tableaux, 8);
        Array.Copy(other._reserve, _reserve, 4);
        _cardsUnsorted = other._cardsUnsorted; _movesPerformed = other._movesPerformed; _movesEstimated = other._movesEstimated;
        _autoPlays = other._autoPlays; _reserveUsed = other._reserveUsed; _hash = other._hash; LastMove = other.LastMove; PathId = other.PathId;
    }
    public void SetCards(List<Card> cards)
    {
        Array.Clear(_foundations, 0, 4);
        for (var i = 0; i < 4; i++) _reserve[i] = new Card(-1);
        _reserveUsed = _cardsUnsorted = _movesPerformed = _autoPlays = 0; _movesEstimated = 52; _hash = 0; LastMove = default; PathId = 0;
        Tableau.InitTableau = new Card[8][];
        for (var i = 0; i < 8; i++) Tableau.InitTableau[i] = new Card[i < 4 ? 7 : 6];
        for (var i = 0; i < cards.Count; i++) Tableau.InitTableau[i % 8][i / 8] = cards[i];
        for (var i = 0; i < 8; i++) { _tableaux[i].SetCards(i, i < 4 ? 7 : 6); _cardsUnsorted += (byte)_tableaux[i].UnsortedSize(); }
    }
    public void InitializeFromFullState(List<List<string>> tableaux, string foundationsLine, string cellsLine)
    {
        Array.Clear(_foundations, 0, 4);
        for (var i = 0; i < 4; i++) _reserve[i] = new Card(-1);
        _reserveUsed = _cardsUnsorted = _movesPerformed = _autoPlays = 0; _movesEstimated = 52; _hash = 0; LastMove = default; PathId = 0;
        var foundationParts = foundationsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 1; i < foundationParts.Length; i++)
        {
            if (foundationParts[i].Length == 3 && foundationParts[i][1] == '-')
            {
                var suit = foundationParts[i][0] switch { 'S' => Suit.SPADE, 'H' => Suit.HEART, 'D' => Suit.DIAMOND, _ => Suit.CLUB };
                var count = foundationParts[i][2] == '-' ? 0 : (int)CardHelper.Parse($"{foundationParts[i][2]}{foundationParts[i][0]}").Rank + 1;
                _foundations[(int)suit] = (byte)count; _movesEstimated -= (byte)count;
            }
        }
        var cellTokens = cellsLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 1; i < cellTokens.Length && i - 1 < 4; i++)
        {
            _reserve[i - 1] = cellTokens[i] != "-" ? CardHelper.Parse(cellTokens[i]) : new Card(-1);
            if (cellTokens[i] != "-") _reserveUsed++;
        }
        Tableau.InitTableau = new Card[8][];
        for (var i = 0; i < 8; i++)
        {
            var arr = new Card[tableaux[i].Count];
            for (var j = 0; j < tableaux[i].Count; j++) arr[j] = CardHelper.Parse(tableaux[i][j]);
            Tableau.InitTableau[i] = arr; _tableaux[i].SetCards(i, arr.Length); _cardsUnsorted += (byte)_tableaux[i].UnsortedSize();
        }
        AutoPlay();
    }
    public static void Initialize()
    {
        var rand = new Random(12345);
        _reserveRand = CreateHashRand(52, rand);
        _tableauUnsortedRand = new uint[8][]; _tableauSortedRand = new uint[8][]; _tableauTopRand = new uint[8][];
        for (var i = 0; i < 8; i++) { _tableauUnsortedRand[i] = CreateHashRand(8, rand); _tableauSortedRand[i] = CreateHashRand(14, rand); _tableauTopRand[i] = CreateHashRand(52, rand); }
    }
    private static uint[] CreateHashRand(int count, Random rand)
    {
        var result = new uint[count];
        for (var i = 0; i < count; i++) result[i] = ((uint)rand.Next() << 16) ^ (uint)rand.Next();
        return result;
    }
    public void ComputeHash()
    {
        _hash = 0;
        for (var i = 0; i < 4; i++) if (_reserve[i].RawValue != -1) _hash += _reserveRand[_reserve[i].RawValue];
        for (var i = 0; i < 8; i++) { _hash += _tableauUnsortedRand[i][_tableaux[i].UnsortedSize()] + _tableauSortedRand[i][_tableaux[i].SortedSize()]; if (!_tableaux[i].IsEmpty()) _hash += _tableauTopRand[i][_tableaux[i].Top().RawValue]; }
    }
    public int Bin() => _movesEstimated + _cardsUnsorted + _reserveUsed;
    public int MinTotalMoves() => _movesPerformed + _movesEstimated;
    public bool IsGoal() => _cardsUnsorted == 0 && _reserveUsed == 0;
    public void Expand(Pool<FreecellNode> pool, List<FreecellNode> outNodes)
    {
        for (var r = 0; r < 4; r++)
        {
            if (_reserve[r].RawValue == -1) continue;
            if (AllowReserveToFoundation() && FindFoundation(_reserve[r]) != -1 && AllowReserveToFoundation(FindFoundation(_reserve[r]))) outNodes.Add(pool.New(this).ReserveToFoundation(r).AutoPlay());
            var triedEmpty = false;
            for (var t = 0; t < 8; t++)
                if (_tableaux[t].Accepting(_reserve[r]) && AllowReserveToTableau(r, t) && (!_tableaux[t].IsEmpty() || !triedEmpty))
                { outNodes.Add(pool.New(this).ReserveToTableau(r, t).AutoPlay()); if (_tableaux[t].IsEmpty()) triedEmpty = true; }
        }
        for (var i = 0; i < 8; i++)
        {
            if (_tableaux[i].IsEmpty()) continue;
            if (AllowTableauToFoundation() && FindFoundation(_tableaux[i].Top()) != -1 && AllowTableauToFoundation(i, FindFoundation(_tableaux[i].Top()))) outNodes.Add(pool.New(this).TableauToFoundation(i).AutoPlay());
            for (var j = 0; j < 8; j++)
            {
                if (i == j || !AllowTableauToTableau(i, j)) continue;
                var count = _tableaux[i].CountMovable(in _tableaux[j]);
                if (count > 0 && (_tableaux[j].IsEmpty() || count <= MaxSuperMoveSize(i, j))) outNodes.Add(pool.New(this).TableauToTableau(i, j).AutoPlay());
            }
            var freeSlot = GetFirstFreeReserveSlot();
            if (freeSlot != -1 && AllowTableauToReserve(i)) outNodes.Add(pool.New(this).TableauToReserve(i, freeSlot).AutoPlay());
        }
    }
    public void ApplyMove(Move move)
    {
        switch (move.Type)
        {
            case MoveType.TableauToFoundation: TableauToFoundation(move.From); break;
            case MoveType.TableauToReserve: TableauToReserve(move.From, move.To); break;
            case MoveType.TableauToTableau: TableauToTableau(move.From, move.To); break;
            case MoveType.ReserveToFoundation: ReserveToFoundation(move.From); break;
            case MoveType.ReserveToTableau: ReserveToTableau(move.From, move.To); break;
        }
        AutoPlay();
    }
    public void DebugState(string title)
    {
        if (!Solution.EnableDebug) return;
        var sb = new StringBuilder();
        sb.AppendLine($"--- {title} ---");
        sb.AppendLine($"Moves: {MovesPerformed}, Estimated: {_movesEstimated}, MinTotal: {MinTotalMoves()}, Unsorted: {_cardsUnsorted}, Reserve: {_reserveUsed}, Auto: {_autoPlays}");
        sb.Append("Foundations:");
        char[] suitChars = { 'S', 'H', 'D', 'C' };
        for (int i = 0; i < 4; i++) sb.Append($" {suitChars[i]}:").Append(_foundations[i] > 0 ? "A23456789TJQK"[_foundations[i] - 1].ToString() : "-");
        sb.AppendLine();
        sb.Append("Reserve:    ");
        for (int i = 0; i < 4; i++) sb.Append(_reserve[i].RawValue != -1 ? $"[{_reserve[i],-2}] " : "[  ] ");
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
            for (int c = 0; c < 8; c++) sb.Append(r < tableauCardStrings[c].Count ? $"  {tableauCardStrings[c][r],-3}  " : "       ");
            sb.AppendLine();
        }
        sb.AppendLine("--------------------------------------------------------");
        Solution.Debug(sb.ToString());
    }
    public FreecellNode AutoPlay()
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            for (var r = 0; r < 4; r++) if (_reserve[r].RawValue != -1 && CanAutoPlay(_reserve[r])) { ReserveToFoundation(r, true); _autoPlays++; changed = true; }
            for (var t = 0; t < 8; t++) if (!_tableaux[t].IsEmpty() && CanAutoPlay(_tableaux[t].Top())) { TableauToFoundation(t, true); _autoPlays++; changed = true; }
        }
        return this;
    }
    private int GetFirstFreeReserveSlot() { for (var i = 0; i < 4; i++) if (_reserve[i].RawValue == -1) return i; return -1; }
    private int FindFoundation(Card card) => _foundations[(int)card.Suit] == (int)card.Rank ? (int)card.Suit : -1;
    private int MaxSuperMoveSize(int from, int to)
    {
        if (!_tableaux[to].IsEmpty()) return 5 - _reserveUsed;
        var emptyTableaus = 0;
        for (var t = 0; t < 8; t++) if (t != from && t != to && _tableaux[t].IsEmpty()) emptyTableaus++;
        return (5 - _reserveUsed) * (1 << emptyTableaus);
    }
    private bool AllowReserveToFoundation() => LastMove.Type != MoveType.ReserveToTableau && LastMove.Type != MoveType.TableauToTableau && LastMove.Type != MoveType.TableauToReserve;
    private bool AllowReserveToFoundation(int f) => !(LastMove.Type == MoveType.ReserveToFoundation && f != LastMove.To) && !(LastMove.Type == MoveType.TableauToFoundation && f != LastMove.To);
    private bool AllowTableauToFoundation() => LastMove.Type != MoveType.ReserveToTableau;
    private bool AllowTableauToFoundation(int t, int f) => !(LastMove.Type == MoveType.TableauToFoundation && LastMove.To != f && LastMove.From > t) && !(LastMove.Type == MoveType.TableauToTableau && t != LastMove.From && t != LastMove.To) && !(LastMove.Type == MoveType.TableauToReserve && LastMove.From != t);
    private bool AllowReserveToTableau(int r, int t) => !(LastMove.Type == MoveType.TableauToTableau && t != LastMove.From && t != LastMove.To) && !(LastMove.Type == MoveType.TableauToReserve && r == LastMove.To && t == LastMove.From);
    private bool AllowTableauToTableau(int t1, int t2) => !(LastMove.Type == MoveType.TableauToReserve && LastMove.From != t1 && LastMove.From != t2);
    private bool AllowTableauToReserve(int t) => !(LastMove.Type == MoveType.TableauToReserve && LastMove.From > t) && !(LastMove.Type == MoveType.ReserveToTableau && LastMove.To == t);
    private bool CanAutoPlay(Card card)
    {
        var suit = (int)card.Suit; var rank = (int)card.Rank;
        if (_foundations[suit] != rank) return false;
        if (rank <= (int)Rank.R2) return true;
        return card.Color == CardColor.RED ? _foundations[(int)Suit.SPADE] >= rank && _foundations[(int)Suit.CLUB] >= rank : _foundations[(int)Suit.HEART] >= rank && _foundations[(int)Suit.DIAMOND] >= rank;
    }
    public FreecellNode ReserveToFoundation(int r, bool autoPlay = false)
    {
        _foundations[(int)_reserve[r].Suit]++; _reserve[r] = new Card(-1); _reserveUsed--; _movesEstimated--;
        if (!autoPlay) { LastMove = new Move(MoveType.ReserveToFoundation, r, (int)_reserve[r].Suit); _movesPerformed++; }
        return this;
    }
    public FreecellNode ReserveToTableau(int r, int t)
    {
        _tableaux[t].Push(_reserve[r]); _reserve[r] = new Card(-1); _reserveUsed--; LastMove = new Move(MoveType.ReserveToTableau, r, t); _movesPerformed++;
        return this;
    }
    public FreecellNode TableauToFoundation(int t, bool autoPlay = false)
    {
        _foundations[(int)_tableaux[t].Top().Suit]++; _cardsUnsorted -= (byte)_tableaux[t].Pop(); _movesEstimated--;
        if (!autoPlay) { LastMove = new Move(MoveType.TableauToFoundation, t, (int)_tableaux[t].Top().Suit); _movesPerformed++; }
        return this;
    }
    public FreecellNode TableauToReserve(int t, int reserveSlot)
    {
        _reserve[reserveSlot] = _tableaux[t].Top(); _reserveUsed++; _cardsUnsorted -= (byte)_tableaux[t].Pop(); LastMove = new Move(MoveType.TableauToReserve, t, reserveSlot); _movesPerformed++;
        return this;
    }
    public FreecellNode TableauToTableau(int s, int t)
    {
        _cardsUnsorted -= (byte)_tableaux[s].MoveTo(ref _tableaux[t], MaxSuperMoveSize(s, t)); LastMove = new Move(MoveType.TableauToTableau, s, t); _movesPerformed++;
        return this;
    }
    public bool StateEquals(FreecellNode other)
    {
        for (var i = 0; i < 4; i++) if (_reserve[i] != other._reserve[i] || _foundations[i] != other._foundations[i]) return false;
        for (var i = 0; i < 8; i++) if (_tableaux[i].SortedSize() != other._tableaux[i].SortedSize() || _tableaux[i].Size() != other._tableaux[i].Size() || !_tableaux[i].StateEquals(in other._tableaux[i])) return false;
        return true;
    }
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
    private readonly TNode[] _bins;
    public int Min { get; private set; }
    public int Max { get; private set; }
    public int Count { get; private set; }
    public Bucket(int numBins) { _bins = new TNode[numBins]; Clear(); }
    public void Clear() { Array.Clear(_bins, 0, _bins.Length); Min = _bins.Length + 1; Max = -1; Count = 0; }
    public void Add(TNode node, int index)
    {
        node.After = _bins[index]; _bins[index] = node;
        if (node.After == null) { if (Min > index) Min = index; if (Max < index) Max = index; }
        Count++;
    }
    public TNode RemoveMax()
    {
        var node = _bins[Max]; _bins[Max] = node.After;
        if (node.After == null) while (Min <= Max && _bins[Max] == null) Max--;
        Count--; return node;
    }
    public void Iterate(Action<TNode> work)
    {
        if (Count == 0) return;
        for (var i = Min; i <= Max; i++) for (var c = _bins[i]; c != null; c = c.After) work(c);
    }
}
class HashTable<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    private readonly TNode[] _bins;
    private readonly int _binMask;
    public HashTable(int numBins)
    {
        var size = 1;
        while (size < numBins) size <<= 1;
        _bins = new TNode[size]; _binMask = size - 1;
    }
    public TNode Find(TNode node)
    {
        for (var c = _bins[node.Hash & (uint)_binMask]; c != null; c = c.Next) if (node.Hash == c.Hash && node.StateEquals(c)) return c;
        return null;
    }
    public void Add(TNode node)
    {
        var index = node.Hash & (uint)_binMask; var next = _bins[index];
        node.Next = next; node.Prev = null; if (next != null) next.Prev = node;
        _bins[index] = node;
    }
    public void Remove(TNode node)
    {
        if (node.Prev != null) node.Prev.Next = node.Next; else _bins[node.Hash & (uint)_binMask] = node.Next;
        if (node.Next != null) node.Next.Prev = node.Prev;
    }
}
class MoveStore
{
    private int[] _parents;
    private Move[] _moves;
    private int _count;
    public MoveStore(int capacity = 1024) { _parents = new int[capacity]; _moves = new Move[capacity]; Reset(); }
    public void Reset() { _count = 1; _parents[0] = -1; _moves[0] = default; }
    public int Add(int parentId, Move move)
    {
        if (_count == _parents.Length) { var newSize = _parents.Length << 1; Array.Resize(ref _parents, newSize); Array.Resize(ref _moves, newSize); }
        _parents[_count] = parentId; _moves[_count] = move; return _count++;
    }
    public string BuildPlan(int pathId)
    {
        var moves = BuildMoveArray(pathId);
        if (moves.Length == 0) return string.Empty;
        var chars = new char[moves.Length * 2];
        for (var i = 0; i < moves.Length; i++) moves[i].EncodeTo(chars, i * 2);
        return new string(chars);
    }
    public List<Move> BuildMoves(int pathId) => new List<Move>(BuildMoveArray(pathId));
    private Move[] BuildMoveArray(int pathId)
    {
        var count = 0;
        for (var i = pathId; i > 0; i = _parents[i]) count++;
        if (count == 0) return Array.Empty<Move>();
        var result = new Move[count]; var index = count;
        for (var i = pathId; i > 0; i = _parents[i]) result[--index] = _moves[i];
        return result;
    }
}
class Beam<TNode> where TNode : class, IBeamSearchNode<TNode>, new()
{
    private readonly int _beamSize;
    private int _upperbound;
    private readonly List<Bucket<TNode>> _levels;
    private readonly HashTable<TNode> _hashTable;
    private readonly Pool<TNode> _pool = new Pool<TNode>();
    private readonly MoveStore _moveStore = new MoveStore(65536);
    public Beam(int beamSize, int maxMoves)
    {
        _beamSize = beamSize; _upperbound = maxMoves + 1; _levels = new List<Bucket<TNode>>(maxMoves);
        for (var i = 0; i < maxMoves; i++) _levels.Add(new Bucket<TNode>(maxMoves * 2));
        _hashTable = new HashTable<TNode>(beamSize * 2);
    }
    public TNode BeamSearch(TNode layout)
    {
        _moveStore.Reset(); var root = _pool.New(layout); root.PathId = 0; root.ComputeHash();
        if (root.IsGoal()) return root;
        _levels[0].Add(root, root.Bin()); _hashTable.Add(root); TNode solution = null;
        for (var i = 0; i < _levels.Count - 1; i++)
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
        TNode solution = null; var newNodesBuffer = new List<TNode>(64);
        curLevel.Iterate(node =>
        {
            if (node.MovesPerformed >= _upperbound - 1) return;
            newNodesBuffer.Clear(); node.Expand(_pool, newNodesBuffer);
            if (newNodesBuffer.Count == 0) return;
            for (var i = 0; i < newNodesBuffer.Count; i++) newNodesBuffer[i].ComputeHash();
            var found = ProcessNewNodes(newNodesBuffer, newLevel);
            if (found != null) { if (solution != null) _pool.Delete(solution); solution = found; }
        });
        return solution;
    }
    private TNode ProcessNewNodes(List<TNode> newNodes, Bucket<TNode> newLevel)
    {
        TNode solution = null;
        for (var i = 0; i < newNodes.Count; i++)
        {
            var newNode = newNodes[i];
            if (newNode.MinTotalMoves() >= _upperbound || _hashTable.Find(newNode) != null) { _pool.Delete(newNode); continue; }
            newNode.PathId = _moveStore.Add(newNode.PathId, newNode.LastMove);
            if (newNode.IsGoal()) { if (solution != null) _pool.Delete(solution); solution = newNode; _upperbound = solution.MinTotalMoves(); continue; }
            if (newLevel.Count < _beamSize) { newLevel.Add(newNode, newNode.Bin()); _hashTable.Add(newNode); }
            else if (newNode.Bin() <= newLevel.Max) { var maxNode = newLevel.RemoveMax(); _hashTable.Remove(maxNode); _pool.Delete(maxNode); newLevel.Add(newNode, newNode.Bin()); _hashTable.Add(newNode); }
            else _pool.Delete(newNode);
        }
        return solution;
    }
    public string EncodeSolution(TNode finish) => finish == null ? string.Empty : _moveStore.BuildPlan(finish.PathId);
    public List<TNode> ReconstructSolutionPath(TNode start, TNode finish)
    {
        if (finish == null) return new List<TNode>();
        var moves = _moveStore.BuildMoves(finish.PathId); var path = new List<TNode>(moves.Count + 1); var node = _pool.New(start);
        path.Add(_pool.New(node));
        for (var i = 0; i < moves.Count; i++) { node.ApplyMove(moves[i]); path.Add(_pool.New(node)); }
        _pool.Delete(node); return path;
    }
}