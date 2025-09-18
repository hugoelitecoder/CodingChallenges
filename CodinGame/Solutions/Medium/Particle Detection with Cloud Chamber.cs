using System;
using System.Collections.Generic;

namespace ParticleTracker
{
    struct Point { public double X, Y; public Point(double x, double y) { X = x; Y = y; } }

    class Program
    {
        const double c = 299_792_458.0; 

        static void Main()
        {
            int w = int.Parse(Console.ReadLine());
            int h = int.Parse(Console.ReadLine());
            double B = double.Parse(Console.ReadLine());
            double V = double.Parse(Console.ReadLine());
            var lines = new string[h];
            for (int i = 0; i < h; i++)
                lines[i] = Console.ReadLine();

            Console.Error.WriteLine($"w = {w}");
            Console.Error.WriteLine($"h = {h}");
            Console.Error.WriteLine($"V = {V}");
            Console.Error.WriteLine(string.Join("", lines));

            var points = new List<Point>();
            for (int y = 0; y < h; y++)
            {
                string row = lines[y];
                int idx = row.IndexOf(' ');
                while (idx != -1)
                {
                    points.Add(new Point(idx, y));
                    idx = row.IndexOf(' ', idx + 1);
                }
            }

            var pairs = new List<(int i, int j)>();
            for (int i = 0; i < points.Count; i++)
                for (int j = i + 1; j < points.Count; j++)
                    pairs.Add((i, j));

            double x = 0, yC = 0;
            foreach (var p in points) { x += p.X; yC += p.Y; }
            x /= points.Count;
            yC /= points.Count;

            double S = double.PositiveInfinity, prevS = 0, relChange = double.PositiveInfinity;
            int iter = 0;
            while (S > 1e-6 && relChange > 1e-3 && iter < 100)
            {
                int n = points.Count;
                var d = new double[n];
                for (int i = 0; i < n; i++)
                {
                    var p = points[i];
                    d[i] = Distance(x, yC, p.X, p.Y);
                }

                double dSdx = 0, dSdy = 0;
                foreach (var (i, j) in pairs)
                {
                    double di = d[i], dj = d[j];
                    double diff = di - dj;
                    var pi = points[i];
                    var pj = points[j];
                    dSdx += 2 * diff * ((x - pi.X) / di - (x - pj.X) / dj);
                    dSdy += 2 * diff * ((yC - pi.Y) / di - (yC - pj.Y) / dj);
                }

                double norm = Math.Sqrt(dSdx * dSdx + dSdy * dSdy);
                double dx = -dSdx / norm, dy = -dSdy / norm;

                double alpha = 0, deltaAlpha = 1e-9, D = 0.1, oldDS = 0;
                for (int jt = 0; jt < 20 && Math.Abs(D) > 1e-6; jt++)
                {
                    double S1 = ComputeS(x + (alpha - deltaAlpha) * dx, yC + (alpha - deltaAlpha) * dy, points, pairs);
                    double S2 = ComputeS(x + (alpha + deltaAlpha) * dx, yC + (alpha + deltaAlpha) * dy, points, pairs);
                    double dS = S2 - S1;
                    if (jt > 0 && dS * oldDS < 0) D *= -0.5;
                    oldDS = dS;
                    alpha += D;
                }

                x += alpha * dx;
                yC += alpha * dy;

                prevS = S;
                S = ComputeS(x, yC, points, pairs);
                if (iter > 0)
                    relChange = prevS != 0 ? Math.Abs((S - prevS) / prevS) : Math.Abs(S - prevS);

                iter++;
            }

            Console.Error.WriteLine($"\n{iter} iterations");

            double R = 0;
            foreach (var p in points)
                R += Distance(x, yC, p.X, p.Y);
            R /= points.Count;

            double sumLine = 0;
            var p0 = points[0]; var pN = points[points.Count - 1];
            foreach (var p in points)
                sumLine += DistancePointToLine(p0, pN, p);
            Console.Error.WriteLine($"sum_distances_to_line = {sumLine}");

            double sumCircle = 0;
            foreach (var p in points)
                sumCircle += Math.Abs(Distance(x, yC, p.X, p.Y) - R);
            Console.Error.WriteLine($"sum_distances_to_circle = {sumCircle}");

            var particleData = new List<(string Name, double q, double m)>
            {
                ("e-",   -1,    0.511),
                ("p+",    1,  938.0),
                ("n0",    0,  940.0),
                ("alpha", 2, 3727.0),
                ("pi+",   1,  140.0)
            };

            if (sumLine <= sumCircle)
            {
                Console.WriteLine("n0 inf");
            }
            else
            {
                double measured = Gamma(V) * V * 1e6 / (B * R * c);
                Console.Error.WriteLine($"    Mesure : |q|/m = {measured:0.######e+0}");

                double minGap = double.PositiveInfinity;
                string best = null;
                foreach (var (Name, q, m) in particleData)
                {
                    if (q == 0) continue;
                    double ratio = Math.Abs(q) / m;
                    double gap = Math.Abs(ratio - measured) / ratio;
                    Console.Error.WriteLine($"    {Name,-5} : |q|/m = {ratio:0.######e+0} ; gap = {gap:0.######e+0}");
                    if (gap < minGap && gap < 0.5)
                    {
                        minGap = gap;
                        best = Name;
                    }
                }

                int roundedR = (int)(Math.Round(R / 10.0) * 10);
                if (best == null)
                    Console.WriteLine("I just won the Nobel prize in physics !");
                else
                    Console.WriteLine($"{best} {roundedR}");
            }
        }

        static double Distance(double x1, double y1, double x2, double y2)
            => Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

        static double ComputeS(double x, double y, List<Point> pts, List<(int i, int j)> pairs)
        {
            double S = 0;
            var d = new double[pts.Count];
            for (int i = 0; i < pts.Count; i++)
                d[i] = Distance(x, y, pts[i].X, pts[i].Y);

            foreach (var (i, j) in pairs)
            {
                double diff = d[i] - d[j];
                S += diff * diff;
            }
            return S;
        }

        static double DistancePointToLine(Point a, Point b, Point p)
        {
            double num = Math.Abs((b.Y - a.Y) * p.X - (b.X - a.X) * p.Y + b.X * a.Y - a.X * b.Y);
            double den = Math.Sqrt((b.X - a.X)*(b.X - a.X) + (b.Y - a.Y)*(b.Y - a.Y));
            return num / den;
        }

        static double Gamma(double beta) => 1.0 / Math.Sqrt(1.0 - beta*beta);
    }
}
