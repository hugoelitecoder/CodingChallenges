using System;
using System.Diagnostics;
using System.Globalization;

public class Solution
{
    public static void Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();
        
        var pex = ReadVector3();
        var vex = ReadVector3();
        var pgx = ReadVector3();
        var vp = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        
        var interceptor = new Interceptor(pex, vex, pgx, vp);
        var result = interceptor.CalculateIntercept();
        
        if (result.CanIntercept)
        {
            Console.WriteLine(result.ProjectileVelocity.ToString());
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:F4}", result.TimeToIntercept));
        }
        else
        {
            Console.WriteLine("Impossible");
        }
        
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Input p_e: ({pex.X}, {pex.Y}, {pex.Z})");
        Console.Error.WriteLine($"[DEBUG] Input v_e: ({vex.X}, {vex.Y}, {vex.Z})");
        Console.Error.WriteLine($"[DEBUG] Input p_g: ({pgx.X}, {pgx.Y}, {pgx.Z})");
        Console.Error.WriteLine($"[DEBUG] Input v_p: {vp}");
        Console.Error.WriteLine($"[DEBUG] Execution time: {sw.ElapsedMilliseconds} ms");
    }
    
    private static Vector3 ReadVector3()
    {
        var inputs = Console.ReadLine().Split(' ');
        var x = double.Parse(inputs[0], CultureInfo.InvariantCulture);
        var y = double.Parse(inputs[1], CultureInfo.InvariantCulture);
        var z = double.Parse(inputs[2], CultureInfo.InvariantCulture);
        return new Vector3(x, y, z);
    }
}

public struct Vector3
{
    public readonly double X;
    public readonly double Y;
    public readonly double Z;
    
    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public double LengthSquared()
    {
        return X * X + Y * Y + Z * Z;
    }
    
    public static double Dot(Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    
    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    public static Vector3 operator /(Vector3 v, double s)
    {
        return new Vector3(v.X / s, v.Y / s, v.Z / s);
    }
    
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:F4} {1:F4} {2:F4}", X, Y, Z);
    }
}

public struct InterceptionResult
{
    public readonly bool CanIntercept;
    public readonly double TimeToIntercept;
    public readonly Vector3 ProjectileVelocity;
    
    public InterceptionResult(bool canIntercept, double time, Vector3 velocity)
    {
        CanIntercept = canIntercept;
        TimeToIntercept = time;
        ProjectileVelocity = velocity;
    }
    
    public static InterceptionResult Impossible => new InterceptionResult(false, 0, new Vector3());
}

public class Interceptor
{
    private readonly Vector3 _planePos;
    private readonly Vector3 _planeVel;
    private readonly Vector3 _gunPos;
    private readonly double _projectileSpeed;
    private const double EPSILON = 1e-9;
    
    public Interceptor(Vector3 planePos, Vector3 planeVel, Vector3 gunPos, double projectileSpeed)
    {
        _planePos = planePos;
        _planeVel = planeVel;
        _gunPos = gunPos;
        _projectileSpeed = projectileSpeed;
    }
    
    public InterceptionResult CalculateIntercept()
    {
        var pRel = _planePos - _gunPos;
        
        var veMagSq = _planeVel.LengthSquared();
        var pRelMagSq = pRel.LengthSquared();
        var dotProd = Vector3.Dot(pRel, _planeVel);
        
        var a = veMagSq - _projectileSpeed * _projectileSpeed;
        var b = 2 * dotProd;
        var c = pRelMagSq;
        
        var t = -1.0;
        
        if (Math.Abs(a) < EPSILON)
        {
            if (Math.Abs(b) < EPSILON)
            {
                return InterceptionResult.Impossible;
            }
            t = -c / b;
        }
        else
        {
            var delta = b * b - 4 * a * c;
            
            if (delta < -EPSILON)
            {
                return InterceptionResult.Impossible;
            }
            
            if (delta < 0) delta = 0;
            
            var sqrtDelta = Math.Sqrt(delta);
            var t1 = (-b + sqrtDelta) / (2 * a);
            var t2 = (-b - sqrtDelta) / (2 * a);
            
            if (t1 > EPSILON && t2 > EPSILON)
            {
                t = Math.Min(t1, t2);
            }
            else if (t1 > EPSILON)
            {
                t = t1;
            }
            else if (t2 > EPSILON)
            {
                t = t2;
            }
            else
            {
                return InterceptionResult.Impossible;
            }
        }
        
        if (t <= EPSILON)
        {
            return InterceptionResult.Impossible;
        }
        
        var projectileVel = pRel / t + _planeVel;
        
        return new InterceptionResult(true, t, projectileVel);
    }
}
