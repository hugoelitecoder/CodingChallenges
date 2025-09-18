using System;

class Solution
{
    static void Main()
    {
        // Input
        var cam = Console.ReadLine().Split();
        int X = int.Parse(cam[0]);
        int Y = int.Parse(cam[1]);
        int A = int.Parse(cam[2]);
        int N = int.Parse(Console.ReadLine());
        string[] map = new string[N];
        for (int i = 0; i < N; ++i) map[i] = Console.ReadLine();

        int W = 61, H = 15;
        var room = new Room(N, map);
        var camera = new Camera(X, Y, A);

        char[,] frame = Renderer.RenderFrame(room, camera, W, H);

        for (int i = 0; i < H; ++i)
        {
            for (int j = 0; j < W; ++j)
                Console.Write(frame[i, j]);
            Console.WriteLine();
        }
    }
}

class Camera
{
    public double X, Y, AngleRad;
    public Camera(int x, int y, int angleDeg)
    {
        X = x; Y = y; AngleRad = angleDeg * Math.PI / 180.0;
    }
}

class RayHit
{
    public double Distance;
    public char WallChar;
    public RayHit(double distance, char wallChar)
    {
        Distance = distance; WallChar = wallChar;
    }
}

class Room
{
    public int N;
    public string[] Map;
    public Room(int n, string[] map)
    {
        N = n; Map = map;
    }
    public bool InBounds(int x, int y) => x >= 0 && x < N && y >= 0 && y < N;
    public bool IsWall(int x, int y) => InBounds(x, y) && Map[y][x] == '#';
}

static class Renderer
{
    public static char[,] RenderFrame(Room room, Camera camera, int ScreenW, int ScreenH)
    {
        var frame = new char[ScreenH, ScreenW];
        for (int i = 0; i < ScreenH; ++i)
            for (int j = 0; j < ScreenW; ++j)
                frame[i, j] = ' ';

        for (int col = 0; col < ScreenW; ++col)
        {
            double angleOffset = -30.0 + 60.0 * col / (ScreenW - 1);
            double rayAngle = camera.AngleRad + angleOffset * Math.PI / 180.0;
            RayHit hit = CastRay(room, camera, rayAngle, angleOffset);
            int wallH = (int)Math.Round(1500.0 / hit.Distance);
            if (wallH < 0) wallH = 0;
            if (2 * wallH + 1 > ScreenH) wallH = (ScreenH - 1) / 2;
            int center = ScreenH / 2;
            for (int y = center - wallH; y <= center + wallH; ++y)
                if (y >= 0 && y < ScreenH)
                    frame[y, col] = hit.WallChar;
        }
        return frame;
    }

    static RayHit CastRay(Room room, Camera camera, double rayAngle, double angleOffset)
    {
        double minDist = double.MaxValue;
        char wallChar = '.';
        var hHit = CastToHWall(room, camera, rayAngle, angleOffset);
        if (hHit != null && hHit.Distance < minDist)
        {
            minDist = hHit.Distance;
            wallChar = hHit.WallChar;
        }
        var vHit = CastToVWall(room, camera, rayAngle, angleOffset);
        if (vHit != null && vHit.Distance < minDist)
        {
            minDist = vHit.Distance;
            wallChar = vHit.WallChar;
        }
        return new RayHit(minDist, wallChar);
    }

    static RayHit CastToHWall(Room room, Camera camera, double angle, double angleOffset)
    {
        double dy = Math.Sin(angle), dx = Math.Cos(angle);
        if (Math.Abs(dy) < 1e-12) return null;
        int stepY = dy > 0 ? 1 : -1;
        double startY = stepY > 0
            ? Math.Floor(camera.Y / 100.0) * 100.0 + 100.0
            : Math.Floor(camera.Y / 100.0) * 100.0;
        for (double y = startY; y >= 0 && y <= (room.N - 1) * 100; y += stepY * 100.0)
        {
            double t = (y - camera.Y) / dy;
            if (t < 0) continue;
            double x = camera.X + t * dx;
            if (x < 0 || x > (room.N - 1) * 100) continue;
            int cellX = (int)(x / 100);
            int cellY = (int)((y + (stepY > 0 ? 0 : -1)) / 100);
            if (!room.InBounds(cellX, cellY)) break;
            if (room.IsWall(cellX, cellY))
            {
                double d = Math.Sqrt((x - camera.X) * (x - camera.X) + (y - camera.Y) * (y - camera.Y));
                double dPerp = d * Math.Cos(angleOffset * Math.PI / 180.0);
                return new RayHit(dPerp, '.');
            }
        }
        return null;
    }

    static RayHit CastToVWall(Room room, Camera camera, double angle, double angleOffset)
    {
        double dx = Math.Cos(angle), dy = Math.Sin(angle);
        if (Math.Abs(dx) < 1e-12) return null;
        int stepX = dx > 0 ? 1 : -1;
        double startX = stepX > 0
            ? Math.Floor(camera.X / 100.0) * 100.0 + 100.0
            : Math.Floor(camera.X / 100.0) * 100.0;
        for (double x = startX; x >= 0 && x <= (room.N - 1) * 100; x += stepX * 100.0)
        {
            double t = (x - camera.X) / dx;
            if (t < 0) continue;
            double y = camera.Y + t * dy;
            if (y < 0 || y > (room.N - 1) * 100) continue;
            int cellX = (int)((x + (stepX > 0 ? 0 : -1)) / 100);
            int cellY = (int)(y / 100);
            if (!room.InBounds(cellX, cellY)) break;
            if (room.IsWall(cellX, cellY))
            {
                double d = Math.Sqrt((x - camera.X) * (x - camera.X) + (y - camera.Y) * (y - camera.Y));
                double dPerp = d * Math.Cos(angleOffset * Math.PI / 180.0);
                return new RayHit(dPerp, ',');
            }
        }
        return null;
    }
}
