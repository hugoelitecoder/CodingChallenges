using System;
using System.Collections.Generic;


class Player
{
    private const double MARS_GRAVITY = 3.711;
    private const double MAX_HORIZONTAL_SPEED = 20.0;
    private const double MAX_VERTICAL_SPEED = 40.0;
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
    private const double VERTICAL_CORRECTION_TIMESCALE = (MAX_VERTICAL_SPEED / REFERENCE_VERTICAL_ACCELERATION) * VERTICAL_TIMESCALE_TUNING_FACTOR;
    private const double HORIZONTAL_TIMESCALE_TUNING_FACTOR = 3.3;
    private const double HORIZONTAL_CORRECTION_TIMESCALE = (MAX_HORIZONTAL_SPEED / MAX_THRUST_ACCELERATION) * HORIZONTAL_TIMESCALE_TUNING_FACTOR;

    private const double HORIZONTAL_BRAKING_DEAD_ZONE_RATIO = 0.39;
    private static double HORIZONTAL_BRAKING_SPEED_THRESHOLD = MAX_HORIZONTAL_SPEED * HORIZONTAL_BRAKING_DEAD_ZONE_RATIO;
    private const double FINAL_APPROACH_TIME_SECONDS = 1.9;
    private static double ALTITUDE_THRESHOLD_FOR_FINAL_APPROACH = MAX_VERTICAL_SPEED * FINAL_APPROACH_TIME_SECONDS;

    private const double ENGINE_CUTOFF_VERTICAL_SPEED_RATIO = 0.75;
    private const double TARGET_LANDING_VERTICAL_SPEED_RATIO = 0.965;
    private static double TARGET_LANDING_VERTICAL_SPEED = MAX_VERTICAL_SPEED * TARGET_LANDING_VERTICAL_SPEED_RATIO;
    private static double ENGINE_CUTOFF_VERTICAL_SPEED = -(MAX_VERTICAL_SPEED * ENGINE_CUTOFF_VERTICAL_SPEED_RATIO);
    private static double ENGINE_CUTOFF_ALTITUDE = (TARGET_LANDING_VERTICAL_SPEED * TARGET_LANDING_VERTICAL_SPEED - ENGINE_CUTOFF_VERTICAL_SPEED * ENGINE_CUTOFF_VERTICAL_SPEED) / (2 * MARS_GRAVITY);

    private const double PANIC_MANEUVER_TIME_SECONDS = 60.0;
    private static double CRITICAL_HORIZONTAL_DISTANCE = MAX_HORIZONTAL_SPEED * PANIC_MANEUVER_TIME_SECONDS;
    private static double NET_UPWARD_ACCELERATION = MAX_THRUST_ACCELERATION - MARS_GRAVITY;
    private static double CRITICAL_ALTITUDE_BUFFER = Math.Abs((-MAX_VERTICAL_SPEED * PANIC_MANEUVER_TIME_SECONDS) + 0.5 * NET_UPWARD_ACCELERATION * PANIC_MANEUVER_TIME_SECONDS * PANIC_MANEUVER_TIME_SECONDS);

    private static int ALTITUDE_THRESHOLD_FOR_FINAL_APPROACH_INT = (int)ALTITUDE_THRESHOLD_FOR_FINAL_APPROACH;
    private static int ENGINE_CUTOFF_ALTITUDE_INT = (int)ENGINE_CUTOFF_ALTITUDE;
    private static int ENGINE_CUTOFF_VERTICAL_SPEED_INT = (int)ENGINE_CUTOFF_VERTICAL_SPEED;
    private static int CRITICAL_HORIZONTAL_DISTANCE_INT = (int)CRITICAL_HORIZONTAL_DISTANCE;
    private static int CRITICAL_ALTITUDE_BUFFER_INT = (int)CRITICAL_ALTITUDE_BUFFER;

    private static int _landingPadLeftX, _landingPadRightX, _landingPadY;
    private static int _desiredRotation;
    private static PIDController _angleController;
    private static PIDController _verticalThrustController;

    public static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var surface = new Point[n];
        for (int i = 0; i < n; i++)
        {
            var s = Console.ReadLine().Split(' ');
            surface[i] = new Point(int.Parse(s[0]), int.Parse(s[1]));
        }
        var pad = FindLandingPad(surface);
        _landingPadLeftX = pad[0].X; _landingPadRightX = pad[1].X; _landingPadY = pad[0].Y;

        double angleKp = POSITIONAL_P_GAIN * ANGLE_CONTROL_AGGRESSIVENESS;
        double angleKd = DERIVATIVE_D_GAIN * ANGLE_CONTROL_AGGRESSIVENESS;
        _angleController = new PIDController(angleKp, 0, angleKd);

        double verticalKp = 1.0 / VERTICAL_CORRECTION_TIMESCALE;
        _verticalThrustController = new PIDController(verticalKp, 0, 0);

        while (true)
        {
            var d = Console.ReadLine().Split(' ');
            int x = int.Parse(d[0]), y = int.Parse(d[1]),
                hSpeed = int.Parse(d[2]), vSpeed = int.Parse(d[3]),
                rotation = int.Parse(d[5]);

            CalculateNextMove(x, y, hSpeed, vSpeed, rotation);
        }
    }

    private static void CalculateNextMove(int x, int y, int hSpeed, int vSpeed, int rotation)
    {
        _desiredRotation = GetDesiredRotation(x, y, hSpeed);
        int desiredThrust = GetDesiredThrust(x, y, hSpeed, vSpeed, rotation);
        Console.WriteLine($"{_desiredRotation} {desiredThrust}");
    }

    private static int GetDesiredRotation(int x, int y, int hSpeed)
    {
        if (IsInPanicState(x, y) && hSpeed != 0) return 0;

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
        return Math.Min(horizontalThrust + verticalThrust, 4);
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

    private static bool ShouldCutoffForTouchdown(int x, int y, int vSpeed)
        => GetHorizontalDistanceFromPad(x) == 0 && _desiredRotation == 0 &&
           y - _landingPadY < ENGINE_CUTOFF_ALTITUDE_INT && vSpeed > ENGINE_CUTOFF_VERTICAL_SPEED_INT;

    private static bool IsInPanicState(int x, int y)
        => (y - _landingPadY) < CRITICAL_ALTITUDE_BUFFER_INT && Math.Abs(GetHorizontalDistanceFromPad(x)) > CRITICAL_HORIZONTAL_DISTANCE_INT;

    private static int GetHorizontalDistanceFromPad(int x)
    {
        if (x < _landingPadLeftX) return x - _landingPadLeftX;
        if (x > _landingPadRightX) return x - _landingPadRightX;
        return 0;
    }

    private static Point[] FindLandingPad(IReadOnlyList<Point> surface)
    {
        for (int i = 0; i < surface.Count - 1; ++i)
            if (surface[i].Y == surface[i + 1].Y) return new[] { surface[i], surface[i + 1] };
        throw new InvalidOperationException("Landing pad not found in surface data.");
    }
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

public readonly struct Point
{
    public int X { get; }
    public int Y { get; }
    public Point(int x, int y) { X = x; Y = y; }
}
