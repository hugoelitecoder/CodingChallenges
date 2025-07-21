using System;
using System.Numerics;
using System.Globalization;


class Program
{
    static void Main()
    {
        var P  = BigInteger.Parse("3fddbf07bb3bc551", NumberStyles.HexNumber);
        var A  = BigInteger.Zero;
        var B  = new BigInteger(7);
        var GX = BigInteger.Parse("69d463ce83b758e", NumberStyles.HexNumber);
        var GY = BigInteger.Parse("287a120903f7ef5c", NumberStyles.HexNumber);

        var curve = new EllipticCurve(A, B, P);
        var G     = new Point(curve, GX, GY);

        Console.Error.WriteLine(curve.HasPoint(GX, GY));
        int n = int.Parse(Console.ReadLine()!);
        while (n-- > 0)
        {
            var raw = Console.ReadLine()!.Trim();
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                raw = raw[2..];

            var k = BigInteger.Parse("0" + raw, NumberStyles.HexNumber);
            var R = (Point)G.Mul(k);

            var hx = R.X.ToString("x").TrimStart('0');
            Console.WriteLine("0x" + (hx.Length == 0 ? "0" : hx));
        }
    }

    public record EllipticCurve(BigInteger A, BigInteger B, BigInteger P)
    {
        public bool HasPoint(BigInteger x, BigInteger y)
            => (y * y).Mod(P) == (BigInteger.Pow(x, 3) + A * x + B).Mod(P);
    }

    public abstract record ECPoint(EllipticCurve Curve)
    {
        public abstract ECPoint Add(ECPoint Q);

        public ECPoint Mul(BigInteger k)
        {
            ECPoint R = new Inf(Curve), T = this;
            while (k > 0)
            {
                if (!k.IsEven) R = R.Add(T);
                T = T.Add(T);
                k >>= 1;
            }
            return R;
        }
    }

    public record Point(EllipticCurve Curve, BigInteger X, BigInteger Y) : ECPoint(Curve)
    {
        public override ECPoint Add(ECPoint Q)
        {
            if (Q is Inf) return this;
            var q = (Point)Q;

            BigInteger m;
            if (Equals(q))
            {
                if (Y == 0)
                    return new Inf(Curve);

                m = ((3 * X * X + Curve.A)
                    * (2 * Y).InvMod(Curve.P))
                    .Mod(Curve.P);
            }
            else
            {
                m = ((q.Y - Y)
                    * (q.X - X).InvMod(Curve.P))
                    .Mod(Curve.P);
            }

            var xr = (m * m - X - q.X).Mod(Curve.P);
            var yr = (m * (X - xr) - Y)   .Mod(Curve.P);
            return new Point(Curve, xr, yr);
        }
    }

    public record Inf(EllipticCurve Curve) : ECPoint(Curve)
    {
        public override ECPoint Add(ECPoint Q) => Q;
    }

}

public static class BigIntegerExtensions
{
    public static BigInteger Mod(this BigInteger x, BigInteger m) 
        => (x % m + m) % m;

    public static BigInteger InvMod(this BigInteger a, BigInteger m)
    {
        var m0 = m;
        var x0 = BigInteger.Zero;
        var x1 = BigInteger.One;
        a = a.Mod(m);
        if (m == 1) return 0;
        while (a > 1)
        {
            BigInteger q = a / m;
            (a, m) = (m, a % m);
            var tmp = x0;
            x0 = x1 - q * x0;
            x1 = tmp;
        }
        return x1.Mod(m0);
    }
}
