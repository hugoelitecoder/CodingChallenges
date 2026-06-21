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
    private const int HORIZONTAL_SPEED_DEADBAND = 2;
    private const int MAX_ROTATION_CHANGE_PER_TURN = 12;
    private readonly Pid _pidV = new(0.55, 0, 0);
    private readonly Pid _pidH = new(5.0, 0, 0);
    private readonly Pid _pidAngle = new(5.0, 0, 0);
    private int _rotate, _power;

    public (int rotate, int power) Step(int hSpeed, int hSetpoint, int vSpeed, int vSetpoint)
    {
        if (Math.Abs(hSetpoint - hSpeed) > HORIZONTAL_SPEED_DEADBAND)
        {
            _power = 4;
            int hGain = _pidH.Regulate(hSetpoint, hSpeed);
            int angleGain = _pidAngle.Regulate(vSetpoint, vSpeed) - 23;
            if (hGain > 0) angleGain = -angleGain;
            if (hGain < 0 && angleGain > 0) angleGain = 0;
            else if (hGain > 0 && angleGain < 0) angleGain = 0;
            if (hGain > 0 && hGain > angleGain) hGain = angleGain;
            else if (hGain < 0 && hGain < angleGain) hGain = angleGain;

            int desiredRotate = -Math.Clamp(hGain, -90, 90);
            _rotate = MoveToward(_rotate, desiredRotate, MAX_ROTATION_CHANGE_PER_TURN);
        }
        else
        {
            _rotate = MoveToward(_rotate, 0, MAX_ROTATION_CHANGE_PER_TURN);
            int g = -_pidV.Regulate(vSetpoint, vSpeed);
            _power = g switch
            {
                < 1 => 4,
                < 2 => 3,
                < 3 => 2,
                < 4 => 1,
                _ => 0
            };
            if (Math.Abs(_rotate) > MAX_ROTATION_CHANGE_PER_TURN) _power = 4;
        }
        return (_rotate, _power);
    }

    private static int MoveToward(int value, int target, int maximumChange)
    {
        if (value < target) return Math.Min(value + maximumChange, target);
        if (value > target) return Math.Max(value - maximumChange, target);
        return value;
    }
}

// ------------------------------------------------------------
// Main player
// ------------------------------------------------------------
public class Player
{
    // Physics / game rules -----------------------------------------------
    // These are facts from the game world / CodinGame Mars Lander rules.
    private const double MARS_GRAVITY = 3.711;              // m/s^2 downward
    private const double MAX_THRUST_ACCELERATION = 4.0;     // m/s^2 at power 4

    // Safe touchdown limits. Stored as positive magnitudes for readability.
    private const double MAX_SAFE_HORIZONTAL_SPEED = 20.0;  // m/s
    private const double MAX_SAFE_DESCENT_SPEED = 40.0;     // m/s downward

    // Controller policy --------------------------------------------------
    // These are intentional behavior choices, not hidden physics constants.
    private const double MAX_PLANNED_TILT_DEGREES = 45.0;
    private const double FINAL_APPROACH_LOOKAHEAD_SECONDS = 1.9;
    private const double PANIC_LOOKAHEAD_SECONDS = 60.0;

    // Keep waypoint approach speed conservative so final landing starts with manageable lateral speed.
    private const int MAX_WAYPOINT_HORIZONTAL_SPEED = 35;

    // Do not dive toward a waypoint faster than the current horizontal travel time can support.
    // This keeps the previous horizontal-speed fix, but prevents low waypoint targets from
    // pulling the lander into terrain before it reaches them.
    private const double WAYPOINT_MIN_TIME_TO_TARGET_SECONDS = 1.0;
    private const double WAYPOINT_VERTICAL_PROFILE_GAIN = 1.0;

    // Keep the target just below the legal landing limit: 40 - 1.4 = 38.6 m/s.
    private const double TARGET_DESCENT_SPEED_MARGIN = 1.4;

    // Start the final unpowered coast calculation around 30 m/s downward.
    private const double COAST_START_DESCENT_SPEED = 0.75 * MAX_SAFE_DESCENT_SPEED;

    // Safety margins around physically-derived thresholds.
    private const double WAYPOINT_RADIUS_SAFETY_FACTOR = 1.5;
    private const double HORIZONTAL_SETTLE_SAFETY_FACTOR = 1.25;
    private const double HORIZONTAL_BRAKE_LOOKAHEAD_SECONDS = 2.75;
    private const double VERTICAL_RESPONSE_SAFETY_FACTOR = 1.35;
    private const double PLANNED_VERTICAL_ACCELERATION = 1.8 * MARS_GRAVITY;
    private const double STEEP_TERRAIN_SIDE_MARGIN_FACTOR = 0.45;

    // Angle PID scaling: at the reference position and speed errors, request ~30 degrees.
    private const double FULL_POSITION_ERROR_METERS = 185.0;
    private const double FULL_SPEED_ERROR = 24.7;
    private const double POSITION_CONTROL_WEIGHT = 2.8;
    private const double SPEED_CONTROL_WEIGHT = 8.2;
    private const double MAX_HORIZONTAL_CONTROL_ANGLE_DEGREES = 30.37;

    private static readonly double POSITIONAL_P_GAIN =
        POSITION_CONTROL_WEIGHT / FULL_POSITION_ERROR_METERS;

    private static readonly double DERIVATIVE_D_GAIN =
        SPEED_CONTROL_WEIGHT / FULL_SPEED_ERROR;

    private static readonly double ANGLE_CONTROL_AGGRESSIVENESS =
        MAX_HORIZONTAL_CONTROL_ANGLE_DEGREES / (POSITION_CONTROL_WEIGHT + SPEED_CONTROL_WEIGHT);

    // Derived physics ----------------------------------------------------
    private static readonly double MAX_PLANNED_TILT_RADIANS = DegToRad(MAX_PLANNED_TILT_DEGREES);

    private static readonly double NET_UP_ACCELERATION =
        MAX_THRUST_ACCELERATION - MARS_GRAVITY;

    // Horizontal acceleration available while still exactly compensating gravity vertically.
    private static readonly double MAX_HOVERABLE_HORIZONTAL_ACCELERATION = Math.Sqrt(
        MAX_THRUST_ACCELERATION * MAX_THRUST_ACCELERATION -
        MARS_GRAVITY * MARS_GRAVITY);

    private static readonly double MAX_AGGRESSIVE_HORIZONTAL_ACCELERATION =
        MAX_THRUST_ACCELERATION * Math.Sin(MAX_PLANNED_TILT_RADIANS);

    private static readonly double DOWNWARD_ACCELERATION_DURING_AGGRESSIVE_BRAKE = Math.Max(
        0.0,
        MARS_GRAVITY - MAX_THRUST_ACCELERATION * Math.Cos(MAX_PLANNED_TILT_RADIANS));

    // Derived thresholds -------------------------------------------------
    private static readonly double V_TIMESCALE =
        (MAX_SAFE_DESCENT_SPEED / PLANNED_VERTICAL_ACCELERATION) * VERTICAL_RESPONSE_SAFETY_FACTOR;

    private static readonly double H_TIMESCALE =
        (MAX_SAFE_HORIZONTAL_SPEED / MAX_HOVERABLE_HORIZONTAL_ACCELERATION) * HORIZONTAL_SETTLE_SAFETY_FACTOR;

    private static readonly double H_BRAKE_THR =
        MAX_AGGRESSIVE_HORIZONTAL_ACCELERATION * HORIZONTAL_BRAKE_LOOKAHEAD_SECONDS;

    private static readonly int WAYPOINT_REACHED_THRESHOLD = (int)Math.Ceiling(
        StoppingDistance(MAX_SAFE_HORIZONTAL_SPEED, MAX_HOVERABLE_HORIZONTAL_ACCELERATION) *
        WAYPOINT_RADIUS_SAFETY_FACTOR);

    private static readonly int WAYPOINT_ALTITUDE_CLEARANCE = (int)Math.Ceiling(
        AltitudeLostDuringHorizontalBrake(
            MAX_SAFE_HORIZONTAL_SPEED,
            MAX_SAFE_DESCENT_SPEED,
            MAX_AGGRESSIVE_HORIZONTAL_ACCELERATION,
            DOWNWARD_ACCELERATION_DURING_AGGRESSIVE_BRAKE));

    private static readonly int FINAL_APPROACH_ALT =
        (int)Math.Ceiling(MAX_SAFE_DESCENT_SPEED * FINAL_APPROACH_LOOKAHEAD_SECONDS);

    private static readonly double TARGET_DESCENT_SPEED =
        MAX_SAFE_DESCENT_SPEED - TARGET_DESCENT_SPEED_MARGIN;

    private static readonly int CUTOFF_VSPEED =
        -(int)Math.Round(COAST_START_DESCENT_SPEED);

    private static readonly int CUTOFF_ALT = (int)Math.Ceiling(
        FreeFallDistanceToSpeed(COAST_START_DESCENT_SPEED, TARGET_DESCENT_SPEED, MARS_GRAVITY));

    private static readonly int CRITICAL_HDIST =
        (int)Math.Ceiling(MAX_SAFE_HORIZONTAL_SPEED * PANIC_LOOKAHEAD_SECONDS);

    private static readonly int CRITICAL_ALT_BUFFER = (int)Math.Ceiling(
        AltitudeLostWhileRecoveringVerticalSpeed(
            MAX_SAFE_DESCENT_SPEED,
            NET_UP_ACCELERATION,
            PANIC_LOOKAHEAD_SECONDS));

    private static double DegToRad(double degrees) => degrees * Math.PI / 180.0;

    private static double StoppingDistance(double speed, double acceleration) =>
        speed * speed / (2.0 * acceleration);

    private static double FreeFallDistanceToSpeed(
        double fromDownwardSpeed,
        double toDownwardSpeed,
        double downwardAcceleration) =>
        (toDownwardSpeed * toDownwardSpeed - fromDownwardSpeed * fromDownwardSpeed) /
        (2.0 * downwardAcceleration);

    private static double AltitudeLostDuringHorizontalBrake(
        double horizontalSpeed,
        double downwardSpeed,
        double horizontalAcceleration,
        double extraDownwardAcceleration)
    {
        double t = horizontalSpeed / horizontalAcceleration;
        return downwardSpeed * t + 0.5 * extraDownwardAcceleration * t * t;
    }

    private static double AltitudeLostWhileRecoveringVerticalSpeed(
        double initialDownwardSpeed,
        double upwardAcceleration,
        double seconds) =>
        Math.Max(0.0, initialDownwardSpeed * seconds - 0.5 * upwardAcceleration * seconds * seconds);

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

            if (!_finalApproach &&
                (Point.Distance(new Point(X, Y), _target) < WAYPOINT_REACHED_THRESHOLD ||
                 HasPassedTargetX(X, hSpeed, Y)))
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

        int hSet = Math.Clamp((int)((_target.X - X) * 0.12),
                              -MAX_WAYPOINT_HORIZONTAL_SPEED,
                               MAX_WAYPOINT_HORIZONTAL_SPEED);

        int vSet = DesiredWaypointVerticalSpeed(X, Y, hSet);
        return _approach.Step(hSpeed, hSet, vSpeed, vSet);
    }

    private static int DesiredWaypointVerticalSpeed(int x, int y, int hSet)
    {
        int dy = _target.Y - y;

        // Original behavior: proportional descent/climb toward the waypoint.
        int proportional = Math.Clamp((int)(dy * 0.15), -40, 40);

        // New guard: when the waypoint is still far away horizontally, descend only
        // fast enough to arrive near its altitude when we also arrive near its X.
        // Without this, the lander can hold HSpeed=-35 and VSpeed=-40, hitting
        // terrain before it ever reaches a low waypoint.
        double travelSpeed = Math.Max(MAX_SAFE_HORIZONTAL_SPEED, Math.Abs(hSet));
        double timeToTargetX = Math.Max(
            WAYPOINT_MIN_TIME_TO_TARGET_SECONDS,
            Math.Abs(_target.X - x) / travelSpeed);

        int profiled = Math.Clamp(
            (int)Math.Round((dy / timeToTargetX) * WAYPOINT_VERTICAL_PROFILE_GAIN),
            -40,
             40);

        // For downward travel both values are negative; taking Max chooses the safer,
        // slower descent. For upward travel both are positive; taking Min avoids an
        // over-aggressive climb command.
        return dy < 0 ? Math.Max(proportional, profiled)
                      : Math.Min(proportional, profiled);
    }

    private static bool HasPassedTargetX(int x, int hSpeed, int y)
    {
        int previousX = x - hSpeed; // telemetry is roughly one-second steps

        bool crossed =
            (previousX <= _target.X && x >= _target.X) ||
            (previousX >= _target.X && x <= _target.X);

        bool closeEnoughVertically =
            y <= _target.Y + WAYPOINT_REACHED_THRESHOLD * 5;

        return crossed && closeEnoughVertically;
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

        Point OffsetWaypoint(Point point, Vec normal)
        {
            double clearance = safeAltitude * (1.0 + STEEP_TERRAIN_SIDE_MARGIN_FACTOR * Math.Abs(normal.X));
            return (Vec)point + normal * clearance;
        }

        result.Add(OffsetWaypoint(surface[0], Normal(0, 1)));

        for (int i = 1; i < surface.Length - 1; i++)
        {
            Vec n = (Normal(i - 1, i) + Normal(i, i + 1)).Normalized();
            result.Add(OffsetWaypoint(surface[i], n));
        }

        int last = surface.Length - 1;
        result.Add(OffsetWaypoint(surface[last], Normal(last - 1, last)));
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