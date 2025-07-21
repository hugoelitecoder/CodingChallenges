using System;
using System.Globalization;

namespace Geometry
{
    public struct Point2D
    {
        public double X { get; }
        public double Y { get; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Point2D other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public class Disk
    {
        public Point2D Center { get; }
        public double Radius { get; }

        public Disk(Point2D center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public double IntersectionArea(Disk other)
        {
            double d = Center.DistanceTo(other.Center);
            if (d >= Radius + other.Radius)
                return 0.0;

            if (d + Math.Min(Radius, other.Radius) <= Math.Max(Radius, other.Radius))
            {
                double rMin = Math.Min(Radius, other.Radius);
                return Math.PI * rMin * rMin;
            }
            double r1 = Radius;
            double r2 = other.Radius;

            double phi = 2 * Math.Acos((r1 * r1 + d * d - r2 * r2) / (2 * r1 * d));
            double theta = 2 * Math.Acos((r2 * r2 + d * d - r1 * r1) / (2 * r2 * d));

            double area1 = 0.5 * r1 * r1 * (phi - Math.Sin(phi));
            double area2 = 0.5 * r2 * r2 * (theta - Math.Sin(theta));

            return area1 + area2;
        }
    }
}

namespace Application
{
    using Geometry;

    class Program
    {
        static void Main()
        {
            var parts = Console.ReadLine().Split();
            Point2D p1 = new Point2D(
                double.Parse(parts[0], CultureInfo.InvariantCulture),
                double.Parse(parts[1], CultureInfo.InvariantCulture));
            double r1 = double.Parse(parts[2], CultureInfo.InvariantCulture);

            parts = Console.ReadLine().Split();
            Point2D p2 = new Point2D(
                double.Parse(parts[0], CultureInfo.InvariantCulture),
                double.Parse(parts[1], CultureInfo.InvariantCulture));
            double r2 = double.Parse(parts[2], CultureInfo.InvariantCulture);

            var d1 = new Disk(p1, r1);
            var d2 = new Disk(p2, r2);

            double area = d1.IntersectionArea(d2);
            Console.WriteLine(area.ToString("F2", CultureInfo.InvariantCulture));
        }
    }
}
