using System;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var x0 = float.Parse(inputs[0], CultureInfo.InvariantCulture);
        var y0 = float.Parse(inputs[1], CultureInfo.InvariantCulture);
        inputs = Console.ReadLine().Split(' ');
        var x1 = float.Parse(inputs[0], CultureInfo.InvariantCulture);
        var y1 = float.Parse(inputs[1], CultureInfo.InvariantCulture);
        inputs = Console.ReadLine().Split(' ');
        var vx = float.Parse(inputs[0], CultureInfo.InvariantCulture);
        var vy = float.Parse(inputs[1], CultureInfo.InvariantCulture);
        var result = GetFinalPositions(x0, y0, x1, y1, vx, vy);
        WriteResult(result.x0, result.y0, result.x1, result.y1);
    }

    public static (float x0, float y0, float x1, float y1) GetFinalPositions(
        float x0, float y0, float x1, float y1, float vx, float vy)
    {
        const float R = 0.03075f;
        const float DR = 2 * R;
        const float DR2 = DR * DR;
        const float K = 0.8f;
        const float EPS = 1e-7f;
        const float EPS_DIST = 1e-9f;

        var vmag = (float)Math.Sqrt(vx * vx + vy * vy);
        if (vmag < EPS)
            return (x0, y0, x1, y1);

        var dx = x0 - x1;
        var dy = y0 - y1;
        var dist2 = dx * dx + dy * dy;
        if (Math.Abs(dist2 - DR2) < EPS_DIST)
        {
            var nx = (x1 - x0) / DR;
            var ny = (y1 - y0) / DR;
            var vrel = vx * nx + vy * ny;
            if (vrel > EPS)
                return SimulateCollision(x0, y0, x1, y1, vx, vy, 0f, K, DR);
        }
        else if (dist2 > DR2 + EPS_DIST)
        {
            var a = (vx / K) * (vx / K) + (vy / K) * (vy / K);
            var b = 2 * ((dx * vx + dy * vy) / K);
            var c = dist2 - DR2;
            if (Math.Abs(a) > EPS)
            {
                var D = b * b - 4 * a * c;
                if (D < 0 && D > -EPS * Math.Abs(4 * a * c))
                    D = 0;
                if (D >= 0)
                {
                    var sqrtD = (float)Math.Sqrt(D);
                    var t = float.PositiveInfinity;
                    foreach (var X in new[] { (-b + sqrtD) / (2 * a), (-b - sqrtD) / (2 * a) })
                    {
                        if (X >= -EPS && X < 1f - EPS)
                        {
                            var tt = X < EPS ? 0f : (float)Math.Log(1f - X) / -K;
                            if (tt >= -EPS)
                                t = Math.Min(t, Math.Max(0, tt));
                        }
                    }
                    if (t != float.PositiveInfinity && t * K <= 20)
                        return SimulateCollision(x0, y0, x1, y1, vx, vy, t, K, DR);
                }
            }
        }
        var xf = x0 + vx / K;
        var yf = y0 + vy / K;
        return (xf, yf, x1, y1);
    }

    static (float x0, float y0, float x1, float y1) SimulateCollision(
        float x0, float y0, float x1, float y1, float vx, float vy, float t, float K, float DR)
    {
        var expkt = (float)Math.Exp(-K * t);
        var x0c = x0 + vx / K * (1 - expkt);
        var y0c = y0 + vy / K * (1 - expkt);
        var vxc = vx * expkt;
        var vyc = vy * expkt;
        var ndx = x1 - x0c;
        var ndy = y1 - y0c;
        var nx = ndx / DR;
        var ny = ndy / DR;
        var vdot = vxc * nx + vyc * ny;
        var v0tx = vxc - vdot * nx;
        var v0ty = vyc - vdot * ny;
        var v1x = vdot * nx;
        var v1y = vdot * ny;
        var mag0 = (float)Math.Sqrt(v0tx * v0tx + v0ty * v0ty);
        var mag1 = (float)Math.Sqrt(v1x * v1x + v1y * v1y);
        var xf0 = mag0 < 1e-7f ? x0c : x0c + v0tx / K;
        var yf0 = mag0 < 1e-7f ? y0c : y0c + v0ty / K;
        var xf1 = mag1 < 1e-7f ? x1 : x1 + v1x / K;
        var yf1 = mag1 < 1e-7f ? y1 : y1 + v1y / K;
        return (xf0, yf0, xf1, yf1);
    }

    static void WriteResult(float x0, float y0, float x1, float y1)
    {
        var x0s = x0.ToString("0.0#", CultureInfo.InvariantCulture);
        var y0s = y0.ToString("0.0#", CultureInfo.InvariantCulture);
        var x1s = x1.ToString("0.0#", CultureInfo.InvariantCulture);
        var y1s = y1.ToString("0.0#", CultureInfo.InvariantCulture);
        Console.WriteLine($"{x0s} {y0s}");
        Console.WriteLine($"{x1s} {y1s}");
    }
}
