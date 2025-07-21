using System;

class Player
{
    private const double MouseSpeed = 10.0;
    private const double PoolRadius = 500.0;
    private const double MinCatDist = 80.0;
    private const double AimRadiusBuffer = 5.0;
    private const double ExitSwitchDistance = MouseSpeed;

    static void Main(string[] args)
    {
        int catSpeed = int.Parse(Console.ReadLine());
        double normalAimRadius = CalculateAimRadius(catSpeed);
        double poolCircumference = 2 * Math.PI * PoolRadius;
        double catRimFraction = catSpeed / poolCircumference;
        double exitStep = MouseSpeed + catSpeed / catRimFraction;
        bool useExit = false;

        while (true)
        {
            var tokens = Console.ReadLine().Split(' ');
            double mouseX = double.Parse(tokens[0]);
            double mouseY = double.Parse(tokens[1]);
            double catX   = double.Parse(tokens[2]);
            double catY   = double.Parse(tokens[3]);

            var target = useExit
                ? ComputeExitTarget(mouseX, mouseY, catX, catY, exitStep)
                : ComputeOppositeTarget(catX, catY, normalAimRadius);

            Console.WriteLine($"{(int)Math.Round(target.X)} {(int)Math.Round(target.Y)} Escape the cat");
            if (!useExit)
            {
                double dist = Distance(mouseX, mouseY, target.X, target.Y);
                double threshold = normalAimRadius < PoolRadius / 2 ? ExitSwitchDistance : MinCatDist;
                if (dist <= threshold)
                    useExit = true;
            }
        }
    }

    private static double CalculateAimRadius(int catSpeed)
    {
        double poolCircumference = 2 * Math.PI * PoolRadius;
        double catFraction = catSpeed / poolCircumference;
        double neededCircumference = MouseSpeed / catFraction;
        double mouseRadius = neededCircumference / (2 * Math.PI);
        return Math.Min(mouseRadius - AimRadiusBuffer, PoolRadius);
    }

    private static (double X, double Y) ComputeOppositeTarget(double catX, double catY, double radius)
    {
        double dx = -catX;
        double dy = -catY;
        double length = Math.Sqrt(dx * dx + dy * dy);
        if (length == 0) return (radius, 0);
        double ux = dx / length;
        double uy = dy / length;
        return (ux * radius, uy * radius);
    }

    private static (double X, double Y) ComputeExitTarget(
        double mouseX, double mouseY,
        double catX, double catY,
        double exitStep)
    {
        double distCenter = Math.Sqrt(mouseX * mouseX + mouseY * mouseY);
        double radUx = distCenter > 0 ? mouseX / distCenter : 1;
        double radUy = distCenter > 0 ? mouseY / distCenter : 0;
        double dx = mouseX - catX;
        double dy = mouseY - catY;
        double distMC = Math.Sqrt(dx * dx + dy * dy);
        double acUx = distMC > 0 ? dx / distMC : 1;
        double acUy = distMC > 0 ? dy / distMC : 0;
        double bx = radUx + acUx;
        double by = radUy + acUy;
        double bLen = Math.Sqrt(bx * bx + by * by);
        double ux = bLen > 0 ? bx / bLen : radUx;
        double uy = bLen > 0 ? by / bLen : radUy;
        return (mouseX + ux * exitStep, mouseY + uy * exitStep);
    }

    private static double Distance(double x1, double y1, double x2, double y2)
        => Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
}
