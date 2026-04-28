using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ------------------------------------------------------------
// Geometry
// ------------------------------------------------------------
public readonly record struct Point(int X, int Y)
{
    public static double Distance(Point a, Point b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public readonly record struct Vec(double X, double Y)
{
    public static Vec operator +(Vec a, Vec b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec operator *(Vec a, double s) => new(a.X * s, a.Y * s);
    public double Length => Math.Sqrt(X * X + Y * Y);
    public Vec Normalized() { var l = Length; return l == 0 ? default : new(X / l, Y / l); }
    public static implicit operator Vec(Point p) => new(p.X, p.Y);
    public static implicit operator Point(Vec v) => new((int)v.X, (int)v.Y);
}

// ------------------------------------------------------------
// Single PID (proportional / derivative / integral on demand)
// ------------------------------------------------------------
public sealed class Pid
{
    public double Kp { get; }
    public double Ki { get; }
    public double Kd { get; }
    private double _prevErr, _sumErr;

    public Pid(double kp, double ki = 0, double kd = 0) { Kp = kp; Ki = ki; Kd = kd; }

    public double P(double error) => Kp * error;
    public double D(double derivative) => Kd * derivative;

    public int Regulate(int setpoint, int feedback)
    {
        double err = setpoint - feedback;
        _sumErr += err;
        double output = Kp * err + Ki * _sumErr + Kd * (err - _prevErr);
        _prevErr = err;
        return (int)Math.Round(output);
    }
}

// ------------------------------------------------------------
// Approach controller (formerly ControlLander)
// ------------------------------------------------------------
public sealed class ApproachController
{
    private readonly Pid _pidV = new(0.55, 0, 0);
    private readonly Pid _pidH = new(5.0, 0, 0);
    private readonly Pid _pidAngle = new(5.0, 0, 0);
    private int _rotate, _power, _prevHSpeed;

    public (int rotate, int power) Step(int hSpeed, int hSetpoint, int vSpeed, int vSetpoint)
    {
        if (hSpeed != hSetpoint || _prevHSpeed != hSpeed)
        {
            _power = 4;
            int hGain = _pidH.Regulate(hSetpoint, hSpeed);
            int angleGain = _pidAngle.Regulate(vSetpoint, vSpeed) - 23;
            if (hGain > 0) angleGain = -angleGain;
            // Cancel opposing signs
            if (hGain < 0 && angleGain > 0) angleGain = 0;
            else if (hGain > 0 && angleGain < 0) angleGain = 0;
            // Clamp toward angleGain when same sign
            if (hGain > 0 && hGain > angleGain) hGain = angleGain;
            else if (hGain < 0 && hGain < angleGain) hGain = angleGain;

            _rotate = -Math.Clamp(hGain, -90, 90);
            _prevHSpeed = hSpeed;
        }
        else
        {
            _rotate = 0;
            int g = -_pidV.Regulate(vSetpoint, vSpeed);
            _power = g switch
            {
                < 1 => 4,
                < 2 => 3,
                < 3 => 2,
                < 4 => 1,
                _ => 0
            };
        }
        return (_rotate, _power);
    }
}

// ------------------------------------------------------------
// Main player
// ------------------------------------------------------------
public class Player
{
    // Physics & tuning ---------------------------------------------------
    private const double MARS_GRAVITY = 3.711;
    private const double MAX_HORIZONTAL_SPEED = 20.0;
    private const double MAX_VERTICAL_SPEED = 40.0;
    private const int WAYPOINT_ALTITUDE_CLEARANCE = 300;
    private const int WAYPOINT_REACHED_THRESHOLD = 200;

    private const double MAX_THRUST_ACCELERATION = 4.0;
    private const double REFERENCE_VERTICAL_ACCELERATION = 1.8 * MARS_GRAVITY;
    private const double ANGLE_CONTROL_AGGRESSIVENESS = 2.76123;
    private const double POSITIONAL_P_GAIN = 2.8 / 185.0;
    private const double DERIVATIVE_D_GAIN = 8.2 / 24.7;
    private const double VERTICAL_TIMESCALE_TUNING = 1.35;
    private const double HORIZONTAL_TIMESCALE_TUNING = 3.3;
    private const double HORIZONTAL_BRAKING_DEAD_ZONE_RATIO = 0.39;
    private const double FINAL_APPROACH_TIME_SECONDS = 1.9;
    private const double ENGINE_CUTOFF_VSPEED_RATIO = 0.75;
    private const double TARGET_LANDING_VSPEED_RATIO = 0.965;
    private const double PANIC_TIME_SECONDS = 60.0;

    private static readonly double V_TIMESCALE = (MAX_VERTICAL_SPEED / REFERENCE_VERTICAL_ACCELERATION) * VERTICAL_TIMESCALE_TUNING;
    private static readonly double H_TIMESCALE = (MAX_HORIZONTAL_SPEED / MAX_THRUST_ACCELERATION) * HORIZONTAL_TIMESCALE_TUNING;
    private static readonly double H_BRAKE_THR = MAX_HORIZONTAL_SPEED * HORIZONTAL_BRAKING_DEAD_ZONE_RATIO;
    private static readonly int FINAL_APPROACH_ALT = (int)(MAX_VERTICAL_SPEED * FINAL_APPROACH_TIME_SECONDS);
    private static readonly double TARGET_VSPEED = MAX_VERTICAL_SPEED * TARGET_LANDING_VSPEED_RATIO;
    private static readonly int CUTOFF_VSPEED = (int)(-MAX_VERTICAL_SPEED * ENGINE_CUTOFF_VSPEED_RATIO);
    private static readonly int CUTOFF_ALT = (int)((TARGET_VSPEED * TARGET_VSPEED - CUTOFF_VSPEED * CUTOFF_VSPEED) / (2 * MARS_GRAVITY));
    private static readonly double NET_UP_ACC = MAX_THRUST_ACCELERATION - MARS_GRAVITY;
    private static readonly int CRITICAL_HDIST = (int)(MAX_HORIZONTAL_SPEED * PANIC_TIME_SECONDS);
    private static readonly int CRITICAL_ALT_BUFFER = (int)Math.Abs(
        -MAX_VERTICAL_SPEED * PANIC_TIME_SECONDS + 0.5 * NET_UP_ACC * PANIC_TIME_SECONDS * PANIC_TIME_SECONDS);

    // State --------------------------------------------------------------
    private static readonly Queue<Point> _flightPlan = new();
    private static Point _target;
    private static int _padLeftX, _padRightX, _padY;
    private static bool _finalApproach;
    private static int _desiredRotation;
    private static Pid _angle = null!;
    private static Pid _verticalThrust = null!;
    private static ApproachController _approach = null!;

    // ===================================================================
    static void Main()
    {
        var surface = ReadSurface(out var padBegin, out var padEnd);
        _padLeftX = padBegin.X; _padRightX = padEnd.X; _padY = padBegin.Y;

        _angle = new Pid(POSITIONAL_P_GAIN * ANGLE_CONTROL_AGGRESSIVENESS, 0,
                        DERIVATIVE_D_GAIN * ANGLE_CONTROL_AGGRESSIVENESS);
        _verticalThrust = new Pid(1.0 / V_TIMESCALE);
        _approach = new ApproachController();

        bool first = true;
        while (true)
        {
            var (X, Y, hSpeed, vSpeed, _, rotate, _) = ReadTelemetry();

            if (first)
            {
                BuildFlightPlan(new Point(X, Y), surface);
                PrintDebugMap(new Point(X, Y), surface, _flightPlan.ToList());
                AdvanceTarget();
                first = false;
            }

            if (!_finalApproach && Point.Distance(new Point(X, Y), _target) < WAYPOINT_REACHED_THRESHOLD)
            {
                Console.Error.WriteLine($"Waypoint ({_target.X},{_target.Y}) reached.");
                AdvanceTarget();
            }

            (int finalRotate, int finalPower) = _finalApproach
                ? PrecisionLand(X, Y, hSpeed, vSpeed, rotate)
                : NavigateToWaypoint(X, Y, hSpeed, vSpeed);

            Console.WriteLine($"{finalRotate} {finalPower}");
        }
    }

    // ---- Frame controllers --------------------------------------------
    private static (int rotate, int power) PrecisionLand(int X, int Y, int hSpeed, int vSpeed, int rotate)
    {
        _desiredRotation = DesiredRotation(X, Y, hSpeed);
        int power = DesiredThrust(X, Y, hSpeed, vSpeed, rotate);
        PrintLandingTelemetry(X, Y, hSpeed, vSpeed, _desiredRotation, power);
        return (_desiredRotation, power);
    }

    private static (int rotate, int power) NavigateToWaypoint(int X, int Y, int hSpeed, int vSpeed)
    {
        Console.Error.WriteLine($"NAV: Target=({_target.X},{_target.Y})");
        int hSet = Math.Clamp((int)((_target.X - X) * 0.15), -80, 80);
        int vSet = Math.Clamp((int)((_target.Y - Y) * 0.15), -40, 40);
        return _approach.Step(hSpeed, hSet, vSpeed, vSet);
    }

    // ---- Precision-landing math (unchanged) ---------------------------
    private static int DesiredRotation(int x, int y, int hSpeed)
    {
        if (IsPanic(x, y) && hSpeed != 0) return 0;

        double horizontalError = HorizontalDistanceFromPad(x);
        double effectiveH = (Math.Abs(hSpeed) > H_BRAKE_THR && y > _padY + FINAL_APPROACH_ALT) ? hSpeed : 0;
        return Math.Clamp((int)Math.Round(_angle.P(horizontalError) + _angle.D(effectiveH)), -90, 90);
    }

    private static int DesiredThrust(int x, int y, int hSpeed, int vSpeed, int rotation)
    {
        if (IsPanic(x, y) && vSpeed < -1) return 4;
        if (ShouldCutoff(x, y, vSpeed)) return 0;

        bool braking = (rotation < 0 && hSpeed < 0) || (rotation > 0 && hSpeed > 0);
        int hThrust = braking ? Math.Abs((int)Math.Round(hSpeed / H_TIMESCALE)) : 0;
        int vThrust = (int)Math.Round(_verticalThrust.P(-vSpeed));
        return Math.Clamp(hThrust + vThrust, 0, 4);
    }

    private static bool ShouldCutoff(int x, int y, int vSpeed) =>
        HorizontalDistanceFromPad(x) == 0 &&
        _desiredRotation == 0 &&
        y - _padY < CUTOFF_ALT &&
        vSpeed > CUTOFF_VSPEED;

    private static bool IsPanic(int x, int y) =>
        (y - _padY) < CRITICAL_ALT_BUFFER &&
        Math.Abs(HorizontalDistanceFromPad(x)) > CRITICAL_HDIST;

    private static int HorizontalDistanceFromPad(int x) =>
        x < _padLeftX ? x - _padLeftX :
        x > _padRightX ? x - _padRightX : 0;

    // ---- Flight plan ---------------------------------------------------
    private static void AdvanceTarget()
    {
        _target = _flightPlan.Dequeue();
        if (_flightPlan.Count == 0)
        {
            _finalApproach = true;
            Console.Error.WriteLine("Engaging precision landing controller.");
        }
    }

    private static void BuildFlightPlan(Point start, Point[] surface)
    {
        _flightPlan.Clear();
        foreach (var w in PlanPath(surface, start, WAYPOINT_ALTITUDE_CLEARANCE))
            _flightPlan.Enqueue(w);
        if (_flightPlan.Count > 1 && start.Y > _flightPlan.Peek().Y) _flightPlan.Dequeue();
    }

    private static List<Point> PlanPath(Point[] surface, Point start, double safeAltitude)
    {
        var waypoints = StrategicWaypoints(surface, safeAltitude);
        if (waypoints.Count == 0) return new();

        int padIndex = FindPadIndex(surface);
        if (padIndex == -1) return new();

        int target = padIndex + 1;
        int source = ClosestWaypoint(waypoints, start);

        // Dijkstra with priority queue
        var dist = Enumerable.Repeat(double.PositiveInfinity, waypoints.Count).ToArray();
        var prev = new int[waypoints.Count];
        Array.Fill(prev, -1);
        dist[source] = 0;

        var pq = new PriorityQueue<int, double>();
        pq.Enqueue(source, 0);

        while (pq.TryDequeue(out int u, out double d))
        {
            if (d > dist[u]) continue;
            if (u == target) break;
            for (int v = 0; v < waypoints.Count; v++)
            {
                if (v == u) continue;
                if (IsObstructed(waypoints[u], waypoints[v], surface)) continue;
                double alt = dist[u] + Point.Distance(waypoints[u], waypoints[v]);
                if (alt < dist[v]) { dist[v] = alt; prev[v] = u; pq.Enqueue(v, alt); }
            }
        }

        var path = new List<Point>();
        for (int c = target; c != -1; c = prev[c]) path.Add(waypoints[c]);
        path.Reverse();
        return Simplify(path, surface);
    }

    private static List<Point> Simplify(List<Point> path, Point[] surface)
    {
        if (path.Count < 3) return path;
        var simplified = new List<Point> { path[0] };
        int i = 0;
        while (i < path.Count - 1)
        {
            int last = i + 1;
            for (int k = i + 2; k < path.Count; k++)
            {
                if (IsObstructed(path[i], path[k], surface)) break;
                last = k;
            }
            simplified.Add(path[last]);
            i = last;
        }
        return simplified;
    }

    private static bool IsObstructed(Point p1, Point p2, Point[] surface)
    {
        for (int i = 0; i < surface.Length - 1; i++)
            if (SegmentsIntersect(p1, p2, surface[i], surface[i + 1])) return true;
        return false;
    }

    private static List<Point> StrategicWaypoints(Point[] surface, double safeAltitude)
    {
        if (surface.Length < 2) return new();
        var result = new List<Point>();

        Vec Normal(int from, int to)
        {
            var seg = new Vec(surface[to].X - surface[from].X, surface[to].Y - surface[from].Y);
            return new Vec(-seg.Y, seg.X).Normalized();
        }

        result.Add((Vec)surface[0] + Normal(0, 1) * safeAltitude);

        for (int i = 1; i < surface.Length - 1; i++)
        {
            Vec n = (Normal(i - 1, i) + Normal(i, i + 1)).Normalized();
            result.Add((Vec)surface[i] + n * safeAltitude);
        }

        int last = surface.Length - 1;
        result.Add((Vec)surface[last] + Normal(last - 1, last) * safeAltitude);
        return result;
    }

    private static int FindPadIndex(Point[] surface)
    {
        for (int i = 0; i < surface.Length - 1; i++)
            if (surface[i].Y == surface[i + 1].Y) return i;
        return -1;
    }

    private static int ClosestWaypoint(List<Point> wps, Point pos)
    {
        int best = 0;
        double minSq = double.MaxValue;
        for (int i = 0; i < wps.Count; i++)
        {
            double dx = wps[i].X - pos.X, dy = wps[i].Y - pos.Y;
            double sq = dx * dx + dy * dy;
            if (sq < minSq) { minSq = sq; best = i; }
        }
        return best;
    }

    private static bool SegmentsIntersect(Point p1, Point q1, Point p2, Point q2)
    {
        int o1 = Orient(p1, q1, p2), o2 = Orient(p1, q1, q2);
        int o3 = Orient(p2, q2, p1), o4 = Orient(p2, q2, q1);
        return o1 != o2 && o3 != o4;
    }

    private static int Orient(Point p, Point q, Point r)
    {
        long v = (long)(q.Y - p.Y) * (r.X - q.X) - (long)(q.X - p.X) * (r.Y - q.Y);
        return v == 0 ? 0 : v > 0 ? 1 : 2;
    }

    // ---- I/O -----------------------------------------------------------
    private static Point[] ReadSurface(out Point padBegin, out Point padEnd)
    {
        int n = int.Parse(Console.ReadLine()!);
        var pts = new Point[n];
        for (int i = 0; i < n; i++)
        {
            var t = Console.ReadLine()!.Split(' ');
            pts[i] = new Point(int.Parse(t[0]), int.Parse(t[1]));
        }
        padBegin = padEnd = default;
        for (int i = 0; i < n - 1; i++)
            if (pts[i].Y == pts[i + 1].Y) { padBegin = pts[i]; padEnd = pts[i + 1]; break; }
        return pts;
    }

    private static (int X, int Y, int hSpeed, int vSpeed, int fuel, int rotate, int power) ReadTelemetry()
    {
        var t = Console.ReadLine()!.Split(' ');
        return (int.Parse(t[0]), int.Parse(t[1]), int.Parse(t[2]),
                int.Parse(t[3]), int.Parse(t[4]), int.Parse(t[5]), int.Parse(t[6]));
    }

    private static void PrintLandingTelemetry(int x, int y, int hSpeed, int vSpeed, int rot, int pwr)
    {
        Console.Error.WriteLine("--- PRECISION LANDING ---");
        Console.Error.WriteLine($"  Altitude:{y - _padY}m  PosErr:{-HorizontalDistanceFromPad(x)}m");
        Console.Error.WriteLine($"  VSpeed:{vSpeed}  HSpeed:{hSpeed}");
        Console.Error.WriteLine($"  Output: Angle={rot} Power={pwr}");
    }

    private static void PrintDebugMap(Point start, Point[] surface, List<Point> waypoints)
    {
        const int W = 90, H = 35, GW = 7000, GH = 3000;
        var map = new char[H, W];
        for (int r = 0; r < H; r++) for (int c = 0; c < W; c++) map[r, c] = ' ';

        void Plot(int x, int y, char ch)
        {
            if (y >= 0 && y < H && x >= 0 && x < W) map[y, x] = ch;
        }
        (int mx, int my) ToMap(Point p) => (p.X * W / GW, (GH - p.Y) * H / GH);

        for (int i = 0; i < surface.Length - 1; i++)
        {
            var (x1, y1) = ToMap(surface[i]);
            var (x2, y2) = ToMap(surface[i + 1]);
            char ch = surface[i].Y == surface[i + 1].Y ? '=' : '#';
            int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
            int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                Plot(x1, y1, ch);
                if (x1 == x2 && y1 == y2) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x1 += sx; }
                if (e2 <= dx) { err += dx; y1 += sy; }
            }
        }

        for (int i = 0; i < waypoints.Count; i++)
        {
            var (mx, my) = ToMap(waypoints[i]);
            Plot(mx, my, i == waypoints.Count - 1 ? 'A' : 'W');
        }
        var (sx0, sy0) = ToMap(start); Plot(sx0, sy0, 'L');

        Console.Error.WriteLine("--- Flight Plan Visualization ---");
        var sb = new StringBuilder();
        for (int r = 0; r < H; r++)
        {
            sb.Clear();
            for (int c = 0; c < W; c++) sb.Append(map[r, c]);
            Console.Error.WriteLine(sb);
        }
        Console.Error.WriteLine("L:Start W:Waypoint A:Approach =:Pad");
    }
}