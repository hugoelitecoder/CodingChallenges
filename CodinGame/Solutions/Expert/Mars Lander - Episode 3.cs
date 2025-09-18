using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class Player
{
    private const double MARS_GRAVITY = 3.711;
    private const double MAX_HORIZONTAL_SPEED = 20.0;
    private const double MAX_VERTICAL_SPEED = 40.0;
    private const int WAYPOINT_ALTITUDE_CLEARANCE = 300;
    private const int APPROACH_ALTITUDE_CLEARANCE = 1200;
    private const int WAYPOINT_REACHED_THRESHOLD = 200;

    private const double MAX_THRUST_ACCELERATION = 4.0;
    private const double REFERENCE_VERTICAL_ACCELERATION = 1.8 * MARS_GRAVITY;
    private const double ANGLE_CONTROL_AGGRESSIVENESS = 2.76123;
    private const double POSITIONAL_REFERENCE_DISTANCE = 185.0;
    private const double POSITIONAL_BASE_TILT_DEGREES = 2.8;
    private const double POSITIONAL_P_GAIN = POSITIONAL_BASE_TILT_DEGREES / POSITIONAL_REFERENCE_DISTANCE;
    private const double DERIVATIVE_REFERENCE_SPEED = 24.7;
    private const double DERIVATIVE_BASE_TILT_DEGREES = 8.2;
    private const double DERIVATIVE_D_GAIN = DERIVATIVE_BASE_TILT_DEGREES / DERIVATIVE_REFERENCE_SPEED;
    private const double VERTICAL_TIMESCALE_TUNING_FACTOR = 1.35;
    private const double HORIZONTAL_TIMESCALE_TUNING_FACTOR = 3.3;
    private const double HORIZONTAL_BRAKING_DEAD_ZONE_RATIO = 0.39;
    private const double FINAL_APPROACH_TIME_SECONDS = 1.9;
    private const double ENGINE_CUTOFF_VERTICAL_SPEED_RATIO = 0.75;
    private const double TARGET_LANDING_VERTICAL_SPEED_RATIO = 0.965;
    private const double PANIC_MANEUVER_TIME_SECONDS = 60.0;

    private static readonly double VERTICAL_CORRECTION_TIMESCALE = (MAX_VERTICAL_SPEED / REFERENCE_VERTICAL_ACCELERATION) * VERTICAL_TIMESCALE_TUNING_FACTOR;
    private static readonly double HORIZONTAL_CORRECTION_TIMESCALE = (MAX_HORIZONTAL_SPEED / MAX_THRUST_ACCELERATION) * HORIZONTAL_TIMESCALE_TUNING_FACTOR;
    private static readonly double HORIZONTAL_BRAKING_SPEED_THRESHOLD = MAX_HORIZONTAL_SPEED * HORIZONTAL_BRAKING_DEAD_ZONE_RATIO;
    private static readonly double ALTITUDE_THRESHOLD_FOR_FINAL_APPROACH = MAX_VERTICAL_SPEED * FINAL_APPROACH_TIME_SECONDS;
    private static readonly double TARGET_LANDING_VERTICAL_SPEED = MAX_VERTICAL_SPEED * TARGET_LANDING_VERTICAL_SPEED_RATIO;
    private static readonly double ENGINE_CUTOFF_VERTICAL_SPEED = -(MAX_VERTICAL_SPEED * ENGINE_CUTOFF_VERTICAL_SPEED_RATIO);
    private static readonly double ENGINE_CUTOFF_ALTITUDE = (TARGET_LANDING_VERTICAL_SPEED * TARGET_LANDING_VERTICAL_SPEED - ENGINE_CUTOFF_VERTICAL_SPEED * ENGINE_CUTOFF_VERTICAL_SPEED) / (2 * MARS_GRAVITY);
    private static readonly double NET_UPWARD_ACCELERATION = MAX_THRUST_ACCELERATION - MARS_GRAVITY;
    private static readonly double CRITICAL_HORIZONTAL_DISTANCE = MAX_HORIZONTAL_SPEED * PANIC_MANEUVER_TIME_SECONDS;
    private static readonly double CRITICAL_ALTITUDE_BUFFER = Math.Abs((-MAX_VERTICAL_SPEED * PANIC_MANEUVER_TIME_SECONDS) + 0.5 * NET_UPWARD_ACCELERATION * PANIC_MANEUVER_TIME_SECONDS * PANIC_MANEUVER_TIME_SECONDS);

    private static readonly int ALTITUDE_THRESHOLD_FOR_FINAL_APPROACH_INT = (int)ALTITUDE_THRESHOLD_FOR_FINAL_APPROACH;
    private static readonly int ENGINE_CUTOFF_ALTITUDE_INT = (int)ENGINE_CUTOFF_ALTITUDE;
    private static readonly int ENGINE_CUTOFF_VERTICAL_SPEED_INT = (int)ENGINE_CUTOFF_VERTICAL_SPEED;
    private static readonly int CRITICAL_HORIZONTAL_DISTANCE_INT = (int)CRITICAL_HORIZONTAL_DISTANCE;
    private static readonly int CRITICAL_ALTITUDE_BUFFER_INT = (int)CRITICAL_ALTITUDE_BUFFER;

    private static readonly Queue<Point> _flightPlan = new Queue<Point>();
    private static Point _currentTarget;
    private static int _landingPadLeftX, _landingPadRightX, _landingPadY;
    private static bool _isFinalApproach = false;
    private static int _desiredRotation;
    private static PIDController _angleController;
    private static PIDController _verticalThrustController;

    static void Main(string[] args)
    {
        string[] inputs;
        int surfaceN = int.Parse(Console.ReadLine());
        var surfacePoints = new List<Point>();

        for (int i = 0; i < surfaceN; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]);
            int landY = int.Parse(inputs[1]);
            surfacePoints.Add(new Point(landX, landY));
        }

        Point beginLandingSite = null;
        Point endLandingSite = null;
        for (int i = 0; i < surfaceN - 1; i++)
        {
            if (surfacePoints[i].y == surfacePoints[i + 1].y)
            {
                beginLandingSite = surfacePoints[i];
                endLandingSite = surfacePoints[i + 1];
                _landingPadLeftX = beginLandingSite.x;
                _landingPadRightX = endLandingSite.x;
                _landingPadY = beginLandingSite.y;
                break;
            }
        }

        double angleKp = POSITIONAL_P_GAIN * ANGLE_CONTROL_AGGRESSIVENESS;
        double angleKd = DERIVATIVE_D_GAIN * ANGLE_CONTROL_AGGRESSIVENESS;
        _angleController = new PIDController(angleKp, 0, angleKd);

        double verticalKp = 1.0 / VERTICAL_CORRECTION_TIMESCALE;
        _verticalThrustController = new PIDController(verticalKp, 0, 0);

        var controlLander = new ControlLander();
        bool firstTurn = true;
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]);
            int Y = int.Parse(inputs[1]);
            int hSpeed = int.Parse(inputs[2]);
            int vSpeed = int.Parse(inputs[3]);
            int fuel = int.Parse(inputs[4]);
            int rotate = int.Parse(inputs[5]);
            int power = int.Parse(inputs[6]);

            if (firstTurn)
            {
                var waypointsForDebug = CreateFlightPlan(new Point(X, Y), surfacePoints.ToArray(), beginLandingSite, endLandingSite);
                PrintDebugMap(new Point(X, Y), surfacePoints, waypointsForDebug);
                UpdateCurrentTarget();
                firstTurn = false;
            }

            double distToTarget = Math.Sqrt(Math.Pow(X - _currentTarget.x, 2) + Math.Pow(Y - _currentTarget.y, 2));
            if (!_isFinalApproach && distToTarget < WAYPOINT_REACHED_THRESHOLD)
            {
                Console.Error.WriteLine($"Waypoint ({_currentTarget.x},{_currentTarget.y}) reached. Updating target.");
                UpdateCurrentTarget();
            }

            int finalRotate;
            int finalPower;
            if (_isFinalApproach)
            {
                _desiredRotation = GetDesiredRotation(X, Y, hSpeed);
                finalRotate = _desiredRotation;
                finalPower = GetDesiredThrust(X, Y, hSpeed, vSpeed, rotate);
                PrintLandingTelemetry(X, Y, hSpeed, vSpeed, finalRotate, finalPower);
            }
            else
            {
                Console.Error.WriteLine($"NAV: Target=({_currentTarget.x}, {_currentTarget.y}), Dist={distToTarget:F0}m");
                int hSpeedSetpoint = (int)((_currentTarget.x - X) * 0.15);
                int vSpeedSetpoint = (int)((_currentTarget.y - Y) * 0.15);
                hSpeedSetpoint = Math.Clamp(hSpeedSetpoint, -80, 80);
                vSpeedSetpoint = Math.Clamp(vSpeedSetpoint, -40, 40);

                int[] command = controlLander.ControlHorVerSpeed(hSpeed, hSpeedSetpoint, vSpeed, vSpeedSetpoint);
                finalRotate = command[0];
                finalPower = command[1];
            }

            Console.WriteLine(finalRotate + " " + finalPower);
        }
    }

    private static int GetDesiredRotation(int x, int y, int hSpeed)
    {
        if (IsInPanicState(x, y) && hSpeed != 0)
        {
            return 0;
        }

        double horizontalError = GetHorizontalDistanceFromPad(x);
        double effectiveHSpeed = hSpeed;
        if (Math.Abs(hSpeed) <= HORIZONTAL_BRAKING_SPEED_THRESHOLD || y <= _landingPadY + ALTITUDE_THRESHOLD_FOR_FINAL_APPROACH_INT)
        {
            effectiveHSpeed = 0;
        }

        double pAngle = _angleController.CalculateProportionalTerm(horizontalError);
        double dAngle = _angleController.CalculateDerivativeTerm(effectiveHSpeed);

        return Math.Clamp((int)Math.Round(pAngle + dAngle), -90, 90);
    }

    private static int GetDesiredThrust(int x, int y, int hSpeed, int vSpeed, int rotation)
    {
        if (IsInPanicState(x, y) && vSpeed < -1) return 4;
        if (ShouldCutoffForTouchdown(x, y, vSpeed)) return 0;

        int horizontalThrust = CalculateHorizontalThrustComponent(hSpeed, rotation);
        int verticalThrust = CalculateVerticalThrustComponent(vSpeed);
        int requiredPower = Math.Min(horizontalThrust + verticalThrust, 4);

        return Math.Clamp(requiredPower, 0, 4);
    }

    private static int CalculateHorizontalThrustComponent(int hSpeed, int rotation)
    {
        bool isBraking = (rotation < 0 && hSpeed < 0) || (rotation > 0 && hSpeed > 0);
        return isBraking ? Math.Abs((int)Math.Round(hSpeed / HORIZONTAL_CORRECTION_TIMESCALE)) : 0;
    }

    private static int CalculateVerticalThrustComponent(int vSpeed)
    {
        double verticalSpeedError = 0 - vSpeed;
        double thrust = _verticalThrustController.CalculateProportionalTerm(verticalSpeedError);
        return (int)Math.Round(thrust);
    }

    private static bool ShouldCutoffForTouchdown(int x, int y, int vSpeed) =>
        GetHorizontalDistanceFromPad(x) == 0 && _desiredRotation == 0 &&
        y - _landingPadY < ENGINE_CUTOFF_ALTITUDE_INT && vSpeed > ENGINE_CUTOFF_VERTICAL_SPEED_INT;

    private static bool IsInPanicState(int x, int y) =>
        (y - _landingPadY) < CRITICAL_ALTITUDE_BUFFER_INT && Math.Abs(GetHorizontalDistanceFromPad(x)) > CRITICAL_HORIZONTAL_DISTANCE_INT;

    private static int GetHorizontalDistanceFromPad(int x)
    {
        if (x < _landingPadLeftX) return x - _landingPadLeftX;
        if (x > _landingPadRightX) return x - _landingPadRightX;
        return 0;
    }

    private static void UpdateCurrentTarget()
    {
        if (_flightPlan.Count > 1)
        {
            _currentTarget = _flightPlan.Dequeue();
        }
        else
        {
            _isFinalApproach = true;
            _currentTarget = _flightPlan.Dequeue();
            Console.Error.WriteLine($"Safer approach point reached. Engaging precision landing controller.");
        }
    }

    private static void PrintLandingTelemetry(int x, int y, int hSpeed, int vSpeed, int finalRotate, int finalPower)
    {
        Console.Error.WriteLine("--- PRECISION LANDING ---");
        double altitude = y - _landingPadY;
        double posError = GetHorizontalDistanceFromPad(x) * -1;
        Console.Error.WriteLine($"  Altitude: {altitude:F0}m | Pos Error: {posError:F0}m");
        Console.Error.WriteLine($"  VSpeed:   {vSpeed}m/s | HSpeed:   {hSpeed}m/s");
        Console.Error.WriteLine($"  Output:   Angle={finalRotate}, Power={finalPower}");
    }

    private static void PrintDebugMap(Point startPos, List<Point> surface, List<Point> waypoints)
    {
        const int MAP_WIDTH = 90, MAP_HEIGHT = 35;
        const int GAME_WIDTH = 7000, GAME_HEIGHT = 3000;
        char[,] map = new char[MAP_HEIGHT, MAP_WIDTH];
        for (int r = 0; r < MAP_HEIGHT; r++) for (int c = 0; c < MAP_WIDTH; c++) map[r, c] = ' ';
        for (int i = 0; i < surface.Count - 1; i++)
        {
            Point p1 = surface[i]; Point p2 = surface[i + 1];
            char surfaceChar = (p1.y == p2.y) ? '=' : '#';
            int x1 = p1.x * MAP_WIDTH / GAME_WIDTH; int y1 = (GAME_HEIGHT - p1.y) * MAP_HEIGHT / GAME_HEIGHT;
            int x2 = p2.x * MAP_WIDTH / GAME_WIDTH; int y2 = (GAME_HEIGHT - p2.y) * MAP_HEIGHT / GAME_HEIGHT;
            int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
            int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
            int err = dx + dy, e2;
            for (; ; )
            {
                if (y1 >= 0 && y1 < MAP_HEIGHT && x1 >= 0 && x1 < MAP_WIDTH) map[y1, x1] = surfaceChar;
                if (x1 == x2 && y1 == y2) break; e2 = 2 * err;
                if (e2 >= dy) { err += dy; x1 += sx; }
                if (e2 <= dx) { err += dx; y1 += sy; }
            }
        }
        for (int i = 0; i < waypoints.Count; i++)
        {
            Point p = waypoints[i];
            int mapX = p.x * MAP_WIDTH / GAME_WIDTH; int mapY = (GAME_HEIGHT - p.y) * MAP_HEIGHT / GAME_HEIGHT;
            char waypointChar = (i == waypoints.Count - 1) ? 'A' : 'W';
            if (mapY >= 0 && mapY < MAP_HEIGHT && mapX >= 0 && mapX < MAP_WIDTH) map[mapY, mapX] = waypointChar;
        }
        int startX = startPos.x * MAP_WIDTH / GAME_WIDTH; int startY = (GAME_HEIGHT - startPos.y) * MAP_HEIGHT / GAME_HEIGHT;
        if (startY >= 0 && startY < MAP_HEIGHT && startX >= 0 && startX < MAP_WIDTH) map[startY, startX] = 'L';
        Console.Error.WriteLine("--- Flight Plan Visualization ---");
        for (int r = 0; r < MAP_HEIGHT; r++)
        {
            var sb = new StringBuilder();
            for (int c = 0; c < MAP_WIDTH; c++) sb.Append(map[r, c]);
            Console.Error.WriteLine(sb.ToString());
        }
        Console.Error.WriteLine("L: Lander Start, W: Waypoint, A: Final Approach Point, =: Landing Pad");
        Console.Error.WriteLine("---------------------------------");
    }

    private static List<Point> CreateFlightPlan(Point startPosition, Point[] surface, Point landingPadLeft, Point landingPadRight)
    {
        var waypoints = GenerateAdvancedFlightPlan(surface, startPosition, WAYPOINT_ALTITUDE_CLEARANCE);
        _flightPlan.Clear();
        foreach (var waypoint in waypoints) { _flightPlan.Enqueue(waypoint); }
        if (_flightPlan.Count > 1 && startPosition.y > _flightPlan.Peek().y) { _flightPlan.Dequeue(); }
        return _flightPlan.ToList();
    }

    private static List<Point> GenerateAdvancedFlightPlan(Point[] surface, Point currentPosition, double safeAltitude)
    {
        var waypoints = GenerateStrategicWaypoints(surface, safeAltitude); if (waypoints.Count == 0) return new List<Point>();
        var adjacencyList = new Dictionary<int, List<int>>(); for (int i = 0; i < waypoints.Count; i++) { adjacencyList[i] = new List<int>(); }
        for (int i = 0; i < waypoints.Count; i++) { for (int j = i + 1; j < waypoints.Count; j++) { if (!IsPathObstructed(waypoints[i], waypoints[j], surface)) { adjacencyList[i].Add(j); adjacencyList[j].Add(i); } } }
        int padIndex = FindLandingPadIndex(surface); if (padIndex == -1) return new List<Point>(); int targetNode = padIndex + 1; int startNode = FindClosestWaypointIndex(waypoints, currentPosition);
        var dist = new double[waypoints.Count]; var prev = new int?[waypoints.Count]; var unvisited = new HashSet<int>(); for (int i = 0; i < waypoints.Count; i++) { dist[i] = double.PositiveInfinity; prev[i] = null; unvisited.Add(i); }
        dist[startNode] = 0;
        while (unvisited.Count > 0)
        {
            int u = unvisited.OrderBy(n => dist[n]).First(); unvisited.Remove(u); if (u == targetNode) break; foreach (int v in adjacencyList[u])
            {
                if (!unvisited.Contains(v)) continue; double alt = dist[u] + Distance(waypoints[u], waypoints[v]); if (alt < dist[v]) { dist[v] = alt; prev[v] = u; }
            }
        }
        var path = new List<Point>(); int? current = targetNode; while (current.HasValue) { path.Add(waypoints[current.Value]); current = prev[current.Value]; }
        path.Reverse(); return SimplifyFlightPath(path, surface);
    }

    private static List<Point> SimplifyFlightPath(List<Point> path, Point[] surface)
    {
        if (path.Count < 3) return path; var simplifiedPath = new List<Point> { path[0] }; int currentIndex = 0;
        while (currentIndex < path.Count - 1)
        {
            int lastVisibleIndex = currentIndex + 1; for (int lookAheadIndex = currentIndex + 2; lookAheadIndex < path.Count; lookAheadIndex++)
            {
                if (IsPathObstructed(path[currentIndex], path[lookAheadIndex], surface)) break; else lastVisibleIndex = lookAheadIndex;
            }
            simplifiedPath.Add(path[lastVisibleIndex]); currentIndex = lastVisibleIndex;
        }
        return simplifiedPath;
    }

    private static bool IsPathObstructed(Point p1, Point p2, Point[] surface)
    {
        for (int i = 0; i < surface.Length - 1; i++) { if (LineSegmentsIntersect(p1, p2, surface[i], surface[i + 1])) return true; }
        return false;
    }

    private static List<Point> GenerateStrategicWaypoints(Point[] surface, double safeAltitude)
    {
        if (surface.Length < 2) return new List<Point>(); var waypoints = new List<Point>(); Vector2D firstSegment = (Vector2D)surface[1] - (Vector2D)surface[0]; waypoints.Add((new Vector2D(-firstSegment.Y, firstSegment.X).Normalize() * safeAltitude + (Vector2D)surface[0]).ToPlayerPoint());
        for (int i = 1; i < surface.Length - 1; i++) { Vector2D seg1 = (Vector2D)surface[i] - (Vector2D)surface[i - 1]; Vector2D seg2 = (Vector2D)surface[i + 1] - (Vector2D)surface[i]; Vector2D normal1 = new Vector2D(-seg1.Y, seg1.X).Normalize(); Vector2D normal2 = new Vector2D(-seg2.Y, seg2.X).Normalize(); waypoints.Add(((normal1 + normal2).Normalize() * safeAltitude + (Vector2D)surface[i]).ToPlayerPoint()); }
        Vector2D lastSegment = (Vector2D)surface[surface.Length - 1] - (Vector2D)surface[surface.Length - 2]; waypoints.Add((new Vector2D(-lastSegment.Y, lastSegment.X).Normalize() * safeAltitude + (Vector2D)surface[surface.Length - 1]).ToPlayerPoint()); return waypoints;
    }

    private static int FindLandingPadIndex(Point[] surface) { for (int i = 0; i < surface.Length - 1; i++) if (surface[i].y == surface[i + 1].y) return i; return -1; }
    private static int FindClosestWaypointIndex(List<Point> waypoints, Point position)
    {
        int closestIndex = 0; double minDistanceSq = double.MaxValue; for (int i = 0; i < waypoints.Count; i++)
        {
            double distSq = Math.Pow(waypoints[i].x - position.x, 2) + Math.Pow(waypoints[i].y - position.y, 2); if (distSq < minDistanceSq) { minDistanceSq = distSq; closestIndex = i; }
        }
        return closestIndex;
    }
    private static double Distance(Point p1, Point p2) => Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
    private static bool LineSegmentsIntersect(Point p1, Point q1, Point p2, Point q2) { int o1 = Orientation(p1, q1, p2); int o2 = Orientation(p1, q1, q2); int o3 = Orientation(p2, q2, p1); int o4 = Orientation(p2, q2, q1); return o1 != o2 && o3 != o4; }
    private static int Orientation(Point p, Point q, Point r) { long val = (long)(q.y - p.y) * (r.x - q.x) - (long)(q.x - p.x) * (r.y - q.y); if (val == 0) return 0; return (val > 0) ? 1 : 2; }

    public class Point
    {
        public int x; public int y; public Point() { }
        public Point(int x, int y) { this.x = x; this.y = y; }
    }

    class PIDControllerNav
    {
        public float k, ki, kd; private float prevDev, devSum;
        public PIDControllerNav(float k, float ki, float kd) { this.k = k; this.ki = ki; this.kd = kd; }
        public int PIDRegulator(int setpoint, int feedback) { float deviation = setpoint - feedback; float p = k * deviation; devSum += deviation; float i = ki * devSum; float d = kd * (deviation - prevDev); prevDev = deviation; float pidGain = p + i + d; return (int)Math.Round(pidGain, 0); }
    }

    class ControlLander
    {
        private PIDControllerNav pidV = new PIDControllerNav(0.55F, 0, 0);
        private PIDControllerNav pidH = new PIDControllerNav(5.0F, 0, 0);
        private PIDControllerNav pidAngle = new PIDControllerNav(5.0F, 0, 0);
        private int rotateCommand = 0, powerCommand = 0, prevHSpeed = 0;
        public int[] ControlHorVerSpeed(int hSpeed, int hSpeedSetpoint, int vSpeed, int vSpeedSetpoint)
        {
            int controllerGain, angleGain;
            if (hSpeed != hSpeedSetpoint || prevHSpeed != hSpeed)
            {
                powerCommand = 4; controllerGain = pidH.PIDRegulator(hSpeedSetpoint, hSpeed); angleGain = pidAngle.PIDRegulator(vSpeedSetpoint, vSpeed) - 23;
                if (controllerGain > 0) angleGain = -angleGain; if (controllerGain < 0 && angleGain > 0) angleGain = 0; else if (controllerGain > 0 && angleGain < 0) angleGain = 0;
                if (controllerGain > 0 && controllerGain > angleGain) controllerGain = angleGain; else if (controllerGain < 0 && controllerGain < angleGain) controllerGain = angleGain;
                controllerGain = Math.Clamp(controllerGain, -90, 90); rotateCommand = -controllerGain; prevHSpeed = hSpeed;
            }
            else
            {
                rotateCommand = 0; controllerGain = -pidV.PIDRegulator(vSpeedSetpoint, (vSpeed));
                if ((controllerGain >= 0 && controllerGain < 1) || controllerGain < 0) powerCommand = 4;
                else if (controllerGain >= 2 && controllerGain < 3) powerCommand = 2;
                else if (controllerGain >= 3 && controllerGain < 4) powerCommand = 1; else if (controllerGain >= 1 && controllerGain < 2) powerCommand = 3; else if (controllerGain >= 4) powerCommand = 0;
            }
            return new int[] { rotateCommand, powerCommand };
        }
    }
}

public readonly struct Vector2D
{
    public double X { get; }
    public double Y { get; }
    public Vector2D(double x, double y) { X = x; Y = y; }
    public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
    public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
    public static Vector2D operator *(Vector2D a, double scalar) => new Vector2D(a.X * scalar, a.Y * scalar);
    public double Length() => Math.Sqrt(X * X + Y * Y);
    public Vector2D Normalize() { double len = Length(); return len == 0 ? new Vector2D(0, 0) : new Vector2D(X / len, Y / len); }
    public Player.Point ToPlayerPoint() => new Player.Point((int)X, (int)Y);
    public static implicit operator Vector2D(Player.Point p) => new Vector2D(p.x, p.y);
}

public class PIDController
{
    public double Kp { get; set; }
    public double Ki { get; set; }
    public double Kd { get; set; }

    public PIDController(double kp, double ki, double kd)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
    }

    public double CalculateProportionalTerm(double error)
    {
        return Kp * error;
    }

    public double CalculateDerivativeTerm(double derivativeOfProcessVariable)
    {
        return Kd * derivativeOfProcessVariable;
    }
}