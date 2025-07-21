using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        var ducks = new Duck[3];
        for (int i = 0; i < 3; i++)
            ducks[i] = Duck.Parse(Console.ReadLine());
        var bullet = DuckHunterSolver.CalculateShot(ducks[0], ducks[1], ducks[2]);
        Console.WriteLine(bullet);
    }
}

public static class DuckHunterSolver
{
    public static Bullet CalculateShot(Duck d0, Duck d1, Duck d2)
    {
        var (p0, v0) = d0;
        var p1_rel = d1.Position - p0;
        var v1_rel = d1.Velocity - v0;
        var p2_rel = d2.Position - p0;
        var v2_rel = d2.Velocity - v0;
        var bulletDir = Vector3R.Cross(Vector3R.Cross(p1_rel, v1_rel), Vector3R.Cross(p2_rel, v2_rel));
        var (t1, q1_rel) = SolveTimeAndPosition(p1_rel, v1_rel, bulletDir);
        var (t2, q2_rel) = SolveTimeAndPosition(p2_rel, v2_rel, bulletDir);
        var vb_rel = (q2_rel - q1_rel) / (t2 - t1);
        var pb_rel = q1_rel - vb_rel * t1;
        return new Bullet(pb_rel + p0, vb_rel + v0);

        static (BigRational, Vector3R) SolveTimeAndPosition(Vector3R p, Vector3R v, Vector3R d)
        {
            BigRational r;
            var det_xy = Det2(d.X, d.Y, v.X, v.Y);
            var det_xz = Det2(d.X, d.Z, v.X, v.Z);
            var det_yz = Det2(d.Y, d.Z, v.Y, v.Z);

            if (!det_xy.IsZero)
                r = Det2(p.X, p.Y, v.X, v.Y) / det_xy;
            else if (!det_xz.IsZero)
                r = Det2(p.X, p.Z, v.X, v.Z) / det_xz;
            else
                r = Det2(p.Y, p.Z, v.Y, v.Z) / det_yz;

            var q = d * r;
            var t = !v.X.IsZero ? (q.X - p.X) / v.X
                  : !v.Y.IsZero ? (q.Y - p.Y) / v.Y
                  : (q.Z - p.Z) / v.Z;
            return (t, q);

            static BigRational Det2(BigRational a, BigRational b, BigRational c, BigRational d) => a * d - b * c;
        }
    }
}

public record Duck(Vector3R Position, Vector3R Velocity)
{
    public static Duck Parse(string s)
    {
        var parts = s.Split(new[] { ' ', '@' }, StringSplitOptions.RemoveEmptyEntries);
        return new Duck(
            new Vector3R(BigRational.Parse(parts[0]), BigRational.Parse(parts[1]), BigRational.Parse(parts[2])),
            new Vector3R(BigRational.Parse(parts[3]), BigRational.Parse(parts[4]), BigRational.Parse(parts[5])));
    }
}

public record Bullet(Vector3R Position, Vector3R Velocity)
{
    public override string ToString()
        => $"{Position.X.ToLong()} {Position.Y.ToLong()} {Position.Z.ToLong()} " +
           $"{Velocity.X.ToLong()} {Velocity.Y.ToLong()} {Velocity.Z.ToLong()}";
}

public struct Vector3R
{
    public BigRational X, Y, Z;
    public Vector3R(BigRational x, BigRational y, BigRational z) { X = x; Y = y; Z = z; }
    public static Vector3R operator +(Vector3R a, Vector3R b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3R operator -(Vector3R a, Vector3R b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3R operator *(Vector3R a, BigRational s) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vector3R operator /(Vector3R a, BigRational s) => new(a.X / s, a.Y / s, a.Z / s);
    public static Vector3R Cross(Vector3R a, Vector3R b)
        => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
}

public readonly struct BigRational : IEquatable<BigRational>
{
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }
    public bool IsZero => Numerator == 0;

    public static BigRational Parse(string s) => new(BigInteger.Parse(s));
    public BigRational(BigInteger n) : this(n, 1) { }
    public BigRational(BigInteger n, BigInteger d)
    {
        if (d == 0) throw new DivideByZeroException();
        if (n == 0) { Numerator = 0; Denominator = 1; return; }
        if (d < 0) { n = -n; d = -d; }
        var g = BigInteger.GreatestCommonDivisor(n, d);
        Numerator = n / g; Denominator = d / g;
    }
    public static BigRational operator +(BigRational a, BigRational b) => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static BigRational operator -(BigRational a, BigRational b) => new(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static BigRational operator *(BigRational a, BigRational b) => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    public static BigRational operator /(BigRational a, BigRational b) => new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    public static bool operator ==(BigRational a, BigRational b) => a.Equals(b);
    public static bool operator !=(BigRational a, BigRational b) => !a.Equals(b);
    public long ToLong() => Denominator == 1 ? (long)Numerator : (long)Math.Round((double)Numerator / (double)Denominator);
    public override bool Equals(object obj) => obj is BigRational r && Equals(r);
    public bool Equals(BigRational other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
}
