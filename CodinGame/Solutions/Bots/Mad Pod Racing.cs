using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

class Player
{
    const int Horizon = 8;
    const int Population = 8;
    const int Shield = -1;
    const int Boost = -2;
    const double Deg = Math.PI / 180.0;
    static readonly double[] Cos = new double[360];
    static readonly double[] Sin = new double[360];
    static readonly Plan[] Pool = new Plan[Population];
    static double[] Prefix;
    static readonly double[] CpX = new double[8];
    static readonly double[] CpY = new double[8];
    static readonly bool Trace = Environment.GetEnvironmentVariable("MPR_TRACE") == "1";
    static readonly int FixedEvaluations = ReadOption("MPR_EVALS", 0);
    static readonly int Strategy = ReadOption("MPR_STRATEGY", 1);
    static readonly int Budget = ReadOption("MPR_MS", 55);
    static int Count;
    static int Finish;
    static int Turn;
    static int Racer;
    static int Enemy;
    static int Evaluations;
    static bool Attack;
    static uint RandomState = 2463534242;
    static World Current;
    static Plan Scratch = new Plan();

    public struct Pod
    {
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public int Angle;
        public int Next;
        public int Passed;
        public int Cooldown;
    }

    public struct World
    {
        public Pod A;
        public Pod B;
        public Pod C;
        public Pod D;
        public bool Used;
        public bool EnemyUsed;
    }

    public struct Move
    {
        public int Rotation;
        public int Power;

        public Move(int rotation, int power)
        {
            Rotation = rotation;
            Power = power;
        }
    }

    class Plan
    {
        public readonly Move[] Moves = new Move[Horizon * 2];
        public readonly World[] States = new World[Horizon + 1];
        public readonly double[] Values = new double[Horizon + 1];
        public double Score;
    }

    static void Main()
    {
        var input = new Reader();
        int laps = input.NextInt();
        if (laps == int.MinValue) return;
        Count = input.NextInt();
        Finish = laps * Count;
        Prefix = new double[Finish + Horizon + Count + 2];
        for (int i = 0; i < Count; i++)
        {
            CpX[i] = input.NextInt();
            CpY[i] = input.NextInt();
        }
        for (int i = 0; i < 360; i++)
        {
            Cos[i] = Math.Cos(i * Deg);
            Sin[i] = Math.Sin(i * Deg);
        }
        for (int i = 1; i < Prefix.Length; i++)
        {
            int a = i % Count;
            int b = (a + 1) % Count;
            Prefix[i] = Prefix[i - 1] + Length(CpX[a] - CpX[b], CpY[a] - CpY[b]);
        }
        for (int i = 0; i < Population; i++) Pool[i] = new Plan();
        while (true)
        {
            World old = Current;
            for (int i = 0; i < 4; i++)
            {
                ref Pod p = ref At(ref Current, i);
                int x = input.NextInt();
                if (x == int.MinValue) return;
                int previous = p.Next;
                p.X = x;
                p.Y = input.NextInt();
                p.Vx = input.NextInt();
                p.Vy = input.NextInt();
                p.Angle = input.NextInt();
                p.Next = input.NextInt();
                if (Turn > 0) p.Passed += (p.Next - previous + Count) % Count;
                if (p.Angle < 0) p.Angle = Normalize((int)Math.Round(Math.Atan2(CpY[p.Next] - p.Y, CpX[p.Next] - p.X) / Deg));
                p.Angle = Normalize(p.Angle);
            }
            long start = Stopwatch.GetTimestamp();
            if (Turn > 0) InferOpponent(in old);
            Racer = Progress(in Current.A) >= Progress(in Current.B) ? 0 : 1;
            Enemy = Progress(in Current.C) >= Progress(in Current.D) ? 2 : 3;
            ref Pod lead = ref At(ref Current, Racer);
            ref Pod support = ref At(ref Current, 1 - Racer);
            Attack = Strategy != 0 && Progress(in lead) - Progress(in support) > 2200;
            Plan best = Search(start);
            for (int i = 0; i < 2; i++)
            {
                ref Pod p = ref At(ref Current, i);
                Move move = best.Moves[i];
                int angle = Normalize(p.Angle + move.Rotation);
                int x = (int)Math.Floor(p.X + Cos[angle] * 100000 + 0.5);
                int y = (int)Math.Floor(p.Y + Sin[angle] * 100000 + 0.5);
                string power;
                if (move.Power == Shield)
                {
                    power = "SHIELD";
                    p.Cooldown = 4;
                }
                else if (move.Power == Boost && !Current.Used)
                {
                    power = "BOOST";
                    Current.Used = true;
                }
                else power = Math.Max(0, move.Power == Boost ? 200 : move.Power).ToString();
                if (p.Cooldown > 0) p.Cooldown--;
                Console.WriteLine(x + " " + y + " " + power);
            }
            if (Trace) Console.Error.WriteLine("turn=" + Turn + " ms=" + ((Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " evals=" + Evaluations + " score=" + best.Score.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
            Turn++;
        }
    }

    static Plan Search(long start)
    {
        Evaluations = 0;
        if (Turn > 0)
        {
            for (int i = 0; i < Population; i++)
            {
                Array.Copy(Pool[i].Moves, 2, Pool[i].Moves, 0, (Horizon - 1) * 2);
                Evaluate(Pool[i], null, 0, -2);
            }
        }
        else
        {
            for (int i = 0; i < Population; i++) Evaluate(Pool[i], null, 0, i);
        }
        for (int i = 0; i < 4; i++)
        {
            Evaluate(Scratch, null, 0, i);
            Insert(-1, 0);
        }
        long limit = start + (long)((Turn == 0 ? 800 : Budget) * (Stopwatch.Frequency / 1000.0));
        int iterations = 0;
        while (FixedEvaluations > 0 ? Evaluations < FixedEvaluations : Stopwatch.GetTimestamp() < limit)
        {
            int parent = Random(Population);
            if ((iterations & 3) == 0) parent = BestIndex();
            Plan source = Pool[parent];
            Array.Copy(source.Moves, Scratch.Moves, Horizon * 2);
            int first = Horizon;
            int mutations = 1 + Random(3);
            for (int m = 0; m < mutations; m++)
            {
                int t = Random(Horizon);
                if (Random(4) == 0) t = Random(Math.Min(4, Horizon));
                int pod = Random(2);
                int index = t * 2 + pod;
                Move a = Scratch.Moves[index];
                int kind = Random(12);
                if (kind < 7)
                {
                    a.Rotation = Clamp(a.Rotation + (Random(2) == 0 ? -1 : 1) * (1 + Random(12)), -18, 18);
                    if (kind == 0) a.Rotation = Random(3) * 18 - 18;
                }
                else if (kind < 10) a.Power = Random(5) * 50;
                else if (kind == 10) a.Power = Random(4) == 0 ? Shield : 200;
                else a.Power = Boost;
                Scratch.Moves[index] = a;
                if (t < first) first = t;
                if (kind == 1 && t + 1 < Horizon)
                {
                    Scratch.Moves[index + 2].Rotation = a.Rotation;
                }
            }
            Evaluate(Scratch, source, first, -1);
            Insert(parent, first);
            iterations++;
        }
        return Pool[BestIndex()];
    }

    static void Insert(int parent, int first)
    {
        int worst = 0;
        for (int i = 1; i < Population; i++) if (Pool[i].Score < Pool[worst].Score) worst = i;
        if (Scratch.Score <= Pool[worst].Score) return;
        if (parent >= 0 && first > 0)
        {
            Array.Copy(Pool[parent].States, Scratch.States, first);
            Array.Copy(Pool[parent].Values, Scratch.Values, first);
        }
        Plan temporary = Pool[worst];
        Pool[worst] = Scratch;
        Scratch = temporary;
    }

    static void Evaluate(Plan plan, Plan parent, int first, int seed)
    {
        World state = parent == null ? Current : parent.States[first];
        double sum = parent == null ? 0 : parent.Values[first];
        for (int t = first; t < Horizon; t++)
        {
            plan.States[t] = state;
            plan.Values[t] = sum;
            Move a;
            Move b;
            if (seed >= 0 || seed == -2 && t == Horizon - 1)
            {
                a = Policy(in state, 0, seed < 0 ? 1 : seed);
                b = Policy(in state, 1, seed < 0 ? 1 : seed);
                plan.Moves[t * 2] = a;
                plan.Moves[t * 2 + 1] = b;
            }
            else
            {
                a = plan.Moves[t * 2];
                b = plan.Moves[t * 2 + 1];
            }
            Move c = Drive(in state.C, 1, false);
            Move d = Drive(in state.D, 1, false);
            Step(ref state, a, b, c, d, 302500);
            double value = EvaluateState(in state);
            sum += value * 0.035;
            if (state.A.Passed >= Finish || state.B.Passed >= Finish)
            {
                sum += 10000000 + (Horizon - t) * 100000;
            }
            if (state.C.Passed >= Finish || state.D.Passed >= Finish)
            {
                sum -= 15000000 + (Horizon - t) * 100000;
            }
        }
        plan.States[Horizon] = state;
        plan.Values[Horizon] = sum;
        plan.Score = EvaluateState(in state) + sum;
        Evaluations++;
    }

    static Move Policy(in World state, int id, int style)
    {
        Pod p = id == 0 ? state.A : state.B;
        if (Attack && id != Racer && style != 3) return Intercept(in state, in p);
        Move move = Drive(in p, style, !state.Used && id == Racer);
        if (style == 2 && !state.Used && id == Racer && p.Cooldown == 0 && Math.Abs(move.Rotation) < 9) move.Power = Boost;
        return move;
    }

    static Move Drive(in Pod p, int style, bool boost)
    {
        int next = p.Next;
        double x = CpX[next];
        double y = CpY[next];
        double dx = x - p.X;
        double dy = y - p.Y;
        double distance = Length(dx, dy);
        double speed = Length(p.Vx, p.Vy);
        double drift = style == 0 ? 2.0 : style == 2 ? 4.0 : 3.0;
        int following = next + 1 == Count ? 0 : next + 1;
        if (Swept(p.X, p.Y, p.X + p.Vx, p.Y + p.Vy, next, 302500) && p.Passed + 1 < Finish)
        {
            x = CpX[following];
            y = CpY[following];
            drift = 2.0;
        }
        else if (style != 0)
        {
            double ox = CpX[following] - x;
            double oy = CpY[following] - y;
            double length = Length(ox, oy);
            x += ox * 220.0 / length;
            y += oy * 220.0 / length;
        }
        double desired = Math.Atan2(y - p.Y - p.Vy * drift, x - p.X - p.Vx * drift) / Deg;
        double difference = Difference(desired, p.Angle);
        int rotation = Clamp((int)Math.Round(difference), -18, 18);
        int thrust = Math.Abs(difference) > 100 ? 0 : 200;
        if (Math.Abs(difference) > 65 && speed > 450 && distance < 2500) thrust = 0;
        if (boost && p.Cooldown == 0 && distance > 6000 && Math.Abs(difference) < 5) thrust = Boost;
        return new Move(rotation, thrust);
    }

    static Move Intercept(in World state, in Pod p)
    {
        Pod target = Enemy == 2 ? state.C : state.D;
        double distance = Length(target.X - p.X, target.Y - p.Y);
        double turns = Math.Min(7, Math.Max(1, distance / 1000));
        double x = target.X + target.Vx * turns;
        double y = target.Y + target.Vy * turns;
        if (distance > 5500)
        {
            x = CpX[target.Next];
            y = CpY[target.Next];
        }
        double angle = Math.Atan2(y - p.Y - p.Vy * 2, x - p.X - p.Vx * 2) / Deg;
        double difference = Difference(angle, p.Angle);
        return new Move(Clamp((int)Math.Round(difference), -18, 18), Math.Abs(difference) > 95 ? 0 : 200);
    }

    static double EvaluateState(in World state)
    {
        double a = Progress(in state.A);
        double b = Progress(in state.B);
        double c = Progress(in state.C);
        double d = Progress(in state.D);
        double value = Math.Max(a, b) + Math.Min(a, b) * 0.12 - Math.Max(c, d) * 0.85;
        if (Attack)
        {
            Pod support = Racer == 0 ? state.B : state.A;
            Pod target = Enemy == 2 ? state.C : state.D;
            value -= Length(support.X - target.X - target.Vx * 2, support.Y - target.Y - target.Vy * 2) * 0.09;
        }
        return value;
    }

    static double Progress(in Pod p)
    {
        double dx = CpX[p.Next] - p.X;
        double dy = CpY[p.Next] - p.Y;
        double distance = Math.Max(1, Length(dx, dy));
        int passed = Math.Min(p.Passed, Prefix.Length - 1);
        return Prefix[passed] + passed * 30000 - distance + (p.Vx * dx + p.Vy * dy) / distance * 1.8;
    }

    public static void Step(ref World state, Move a, Move b, Move c, Move d, double safeRadiusSquared = 360000)
    {
        Accelerate(ref state.A, a, ref state.Used);
        Accelerate(ref state.B, b, ref state.Used);
        Accelerate(ref state.C, c, ref state.EnemyUsed);
        Accelerate(ref state.D, d, ref state.EnemyUsed);
        double remaining = 1;
        int events = 0;
        while (remaining > 0)
        {
            double time = remaining;
            int one = -1;
            int two = -1;
            for (int i = 3; i > 0; i--)
            {
                ref Pod p = ref At(ref state, i);
                for (int j = i - 1; j >= 0; j--)
                {
                    ref Pod q = ref At(ref state, j);
                    double hit = Collision(in p, in q);
                    if (hit <= time)
                    {
                        time = hit;
                        one = i;
                        two = j;
                    }
                }
            }
            bool final = time >= remaining;
            for (int i = 0; i < 4; i++)
            {
                ref Pod p = ref At(ref state, i);
                double x = p.X + p.Vx * time;
                double y = p.Y + p.Vy * time;
                if (final)
                {
                    x = Math.Floor(x + 0.5);
                    y = Math.Floor(y + 0.5);
                }
                if (p.Passed < Finish && Swept(p.X, p.Y, x, y, p.Next, i < 2 ? safeRadiusSquared : 360000))
                {
                    p.Passed++;
                    if (++p.Next == Count) p.Next = 0;
                }
                p.X = x;
                p.Y = y;
            }
            remaining -= time;
            if (one >= 0)
            {
                ref Pod p = ref At(ref state, one);
                ref Pod q = ref At(ref state, two);
                Bounce(ref p, ref q);
            }
            if (++events > 256)
            {
                for (int i = 0; i < 4; i++)
                {
                    ref Pod p = ref At(ref state, i);
                    p.X = Math.Floor(p.X + p.Vx * remaining + 0.5);
                    p.Y = Math.Floor(p.Y + p.Vy * remaining + 0.5);
                }
                break;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            ref Pod p = ref At(ref state, i);
            p.X = Math.Floor(p.X + 0.5);
            p.Y = Math.Floor(p.Y + 0.5);
            p.Vx = (int)(p.Vx * 0.85);
            p.Vy = (int)(p.Vy * 0.85);
            if (p.Cooldown > 0) p.Cooldown--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Accelerate(ref Pod p, Move move, ref bool used)
    {
        p.Angle = Normalize(p.Angle + move.Rotation);
        int power = move.Power;
        if (power == Shield)
        {
            p.Cooldown = 4;
            power = 0;
        }
        else if (power == Boost)
        {
            power = used ? 200 : 650;
            used = true;
        }
        if (p.Cooldown > 0) return;
        p.Vx += Cos[p.Angle] * power;
        p.Vy += Sin[p.Angle] * power;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool Swept(double x, double y, double nx, double ny, int checkpoint, double radiusSquared = 360000)
    {
        double dx = nx - x;
        double dy = ny - y;
        double square = dx * dx + dy * dy;
        double t = square > 0 ? ((CpX[checkpoint] - x) * dx + (CpY[checkpoint] - y) * dy) / square : 0;
        t = Math.Max(0, Math.Min(1, t));
        dx = x + dx * t - CpX[checkpoint];
        dy = y + dy * t - CpY[checkpoint];
        return dx * dx + dy * dy < radiusSquared;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double Collision(in Pod a, in Pod b)
    {
        double x = b.X - a.X;
        double y = b.Y - a.Y;
        double square = x * x + y * y;
        if (square <= 640000) return 0;
        double vx = b.Vx - a.Vx;
        double vy = b.Vy - a.Vy;
        double dot = x * vx + y * vy;
        if (dot >= 0) return 2;
        double speed = vx * vx + vy * vy;
        double discriminant = dot * dot - speed * (square - 640000);
        if (discriminant < 0 || speed < 1e-12) return 2;
        return (-dot - Math.Sqrt(discriminant)) / speed;
    }

    static void Bounce(ref Pod a, ref Pod b)
    {
        double x = b.X - a.X;
        double y = b.Y - a.Y;
        double length = Length(x, y);
        if (length < 1e-12)
        {
            x = 1;
            y = 0;
            length = 1;
        }
        x /= length;
        y /= length;
        double one = a.Cooldown == 4 ? 0.1 : 1;
        double two = b.Cooldown == 4 ? 0.1 : 1;
        double force = (x * (a.Vx - b.Vx) + y * (a.Vy - b.Vy)) / (one + two);
        force += Math.Max(120, force);
        a.Vx -= x * force * one;
        a.Vy -= y * force * one;
        b.Vx += x * force * two;
        b.Vy += y * force * two;
        if (length <= 800)
        {
            double gap = (800 - length) * 0.5 + 0.00001;
            a.X -= x * gap;
            a.Y -= y * gap;
            b.X += x * gap;
            b.Y += y * gap;
        }
    }

    static void InferOpponent(in World old)
    {
        for (int i = 2; i < 4; i++)
        {
            Pod before = i == 2 ? old.C : old.D;
            ref Pod now = ref At(ref Current, i);
            double ax = now.Vx / 0.85 - before.Vx;
            double ay = now.Vy / 0.85 - before.Vy;
            bool isolated = true;
            for (int j = 0; j < 4; j++)
            {
                if (i == j) continue;
                ref Pod other = ref At(ref Current, j);
                if (Length(now.X - other.X, now.Y - other.Y) < 1800) isolated = false;
            }
            double acceleration = Length(ax, ay);
            if (isolated && acceleration > 400) Current.EnemyUsed = true;
            if (now.Cooldown > 0) now.Cooldown--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ref Pod At(ref World state, int id)
    {
        if (id == 0) return ref state.A;
        if (id == 1) return ref state.B;
        if (id == 2) return ref state.C;
        return ref state.D;
    }

    static int BestIndex()
    {
        int best = 0;
        for (int i = 1; i < Population; i++) if (Pool[i].Score > Pool[best].Score) best = i;
        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Random(int limit)
    {
        RandomState ^= RandomState << 13;
        RandomState ^= RandomState >> 17;
        RandomState ^= RandomState << 5;
        return (int)(RandomState % (uint)limit);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Normalize(int angle)
    {
        if (angle < 0) angle += 360;
        if (angle >= 360) angle -= 360;
        return angle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double Difference(double target, int angle)
    {
        double difference = target - angle;
        if (difference > 180) difference -= 360;
        if (difference < -180) difference += 360;
        return difference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double Length(double x, double y)
    {
        return Math.Sqrt(x * x + y * y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Clamp(int value, int minimum, int maximum)
    {
        return Math.Max(minimum, Math.Min(maximum, value));
    }

    static int ReadOption(string name, int fallback)
    {
        int value;
        return int.TryParse(Environment.GetEnvironmentVariable(name), out value) ? value : fallback;
    }

    class Reader
    {
        readonly Stream Input = Console.OpenStandardInput();
        readonly byte[] Buffer = new byte[4096];
        int Position;
        int Size;

        int Read()
        {
            if (Position >= Size)
            {
                Size = Input.Read(Buffer, 0, Buffer.Length);
                Position = 0;
                if (Size == 0) return -1;
            }
            return Buffer[Position++];
        }

        public int NextInt()
        {
            int c;
            do c = Read(); while (c >= 0 && c <= 32);
            if (c < 0) return int.MinValue;
            bool negative = c == 45;
            if (negative) c = Read();
            int value = 0;
            while (c >= 48 && c <= 57)
            {
                value = value * 10 + c - 48;
                c = Read();
            }
            return negative ? -value : value;
        }
    }
}
