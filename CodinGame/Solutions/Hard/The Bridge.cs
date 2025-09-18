using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    private const int LaneCount = 4;
    private const string CMD_SPEED = "SPEED";
    private const string CMD_SLOW = "SLOW";
    private const string CMD_JUMP = "JUMP";
    private const string CMD_WAIT = "WAIT";
    private const string CMD_UP = "UP";
    private const string CMD_DOWN = "DOWN";

    private static readonly string[] lanes = new string[LaneCount];
    private static int trackLength, lastHoleIndex;

    struct GameState { public uint Speed; public int Pos; public List<int> ActiveLanes; }
    struct Move { public string Name; public int SpeedDelta, LaneDelta; public bool IsJump; }

    private static readonly Move[] BaseMoves = new[] {
        new Move { Name = CMD_SPEED, SpeedDelta = 1 },
        new Move { Name = CMD_WAIT },
        new Move { Name = CMD_JUMP, IsJump = true },
        new Move { Name = CMD_UP, LaneDelta = -1 },
        new Move { Name = CMD_DOWN, LaneDelta = 1 },
        new Move { Name = CMD_SLOW, SpeedDelta = -1 }
    };

    static void Main()
    {
        int M = int.Parse(Console.ReadLine());
        _ = Console.ReadLine();
        for (int i = 0; i < LaneCount; i++)
            lanes[i] = Console.ReadLine();
        InitializeTrack();

        while (true)
        {
            uint speed = uint.Parse(Console.ReadLine());
            int pos = 0;
            var active = new List<int>();
            for (int i = 0; i < M; i++)
            {
                var parts = Console.ReadLine().Split();
                if (parts[2] == "1")
                {
                    pos = int.Parse(parts[0]);
                    active.Add(int.Parse(parts[1]));
                }
            }

            var state = new GameState { Speed = speed, Pos = pos, ActiveLanes = active };
            var (_, move) = Decide(state);
            Console.WriteLine(move);
        }
    }

    static void InitializeTrack()
    {
        trackLength = lanes[0].Length;
        lastHoleIndex = Math.Max(0, lanes.Max(l => l.LastIndexOf('0')));
    }

    static (uint casualties, string move) Decide(GameState state)
    {
        if (state.Pos >= lastHoleIndex) return (0, CMD_WAIT);
        if (state.Speed == 0) return (0, CMD_SPEED);

        uint bestCas = (uint)state.ActiveLanes.Count;
        string bestMove = CMD_WAIT;

        foreach (var m in BaseMoves)
        {
            if (!IsMoveAllowed(m, state)) continue;
            var (next, lost) = ApplyMove(state, m);
            if (next.ActiveLanes.Count == 0) continue;
            var (futureLost, _) = Decide(next);
            uint total = (uint)lost + futureLost;
            if (total == 0) return (0, m.Name);
            if (total < bestCas) { bestCas = total; bestMove = m.Name; }
        }
        return (bestCas, bestMove);
    }

    static bool IsMoveAllowed(Move m, GameState s)
        => (m.Name != CMD_SLOW || s.Speed > 1)
        && (m.Name != CMD_UP   || !s.ActiveLanes.Contains(0))
        && (m.Name != CMD_DOWN || !s.ActiveLanes.Contains(LaneCount - 1));

    static (GameState next, int lost) ApplyMove(GameState s, Move m)
    {
        int newSpeed = (int)s.Speed + m.SpeedDelta;
        int newPos = Math.Min(s.Pos + newSpeed, trackLength - 1);
        var survivors = new List<int>(); int lost = 0;
        foreach (var ln in s.ActiveLanes)
        {
            if (Survives(s.Pos, newPos, ln, m)) survivors.Add(ln + m.LaneDelta);
            else lost++;
        }
        return (new GameState { Speed = (uint)newSpeed, Pos = newPos, ActiveLanes = survivors }, lost);
    }

    static bool Survives(int start, int end, int lane, Move m)
    {
        bool hole(int ln, int x) => lanes[ln][x] == '0';
        int offset = m.LaneDelta;
        if (m.IsJump) return !hole(lane + offset, end);

        int s = start + 1;
        if (offset != 0)
        {
            for (int x = s; x < end; x++) if (hole(lane, x)) return false;
            for (int x = s; x <= end; x++) if (hole(lane + offset, x)) return false;
            return true;
        }
        for (int x = s; x <= end; x++) if (hole(lane, x)) return false;
        return true;
    }
}
