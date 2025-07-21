using System;
using System.Collections.Generic;

class Program
{
    private const string CMD_WAIT = "WAIT";
    private const string CMD_BLOCK = "BLOCK";
    private const string CMD_ELEVATOR = "ELEVATOR";
    private const int DELAY_BLOCK = 4;
    private const int DELAY_ELEVATOR = 4;
    private const int DELAY_MOVE = 1;

    int width, height, rounds;
    int exitX, exitY;
    int cloneCount, elevatorCount;
    bool[,,,,] visited;
    List<Unit>[] timeline;
    bool[,] hasLift;

    static void Main()
    {
        var solver = new Program();
        solver.Init();
        while (true) solver.Play();
    }

    void Init()
    {
        var parts = Console.ReadLine().Split();
        height       = int.Parse(parts[0]);
        width        = int.Parse(parts[1]);
        rounds       = int.Parse(parts[2]) + 1;
        exitY        = int.Parse(parts[3]);
        exitX        = int.Parse(parts[4]);
        cloneCount   = int.Parse(parts[5]);
        elevatorCount= int.Parse(parts[6]);
        int lifts    = int.Parse(parts[7]);

        hasLift = new bool[width, height];
        for (int i = 0; i < lifts; i++)
        {
            var data = Console.ReadLine().Split();
            int y = int.Parse(data[0]);
            int x = int.Parse(data[1]);
            if (x >= 0 && x < width && y >= 0 && y < height)
                hasLift[x, y] = true;
        }
    }

    void Play()
    {
        var parts = Console.ReadLine().Split();
        int y   = int.Parse(parts[0]);
        int x   = int.Parse(parts[1]);
        int dir = parts[2] == "LEFT" ? 0 : 1;

        if (x < 0)
        {
            Console.WriteLine(CMD_WAIT);
            rounds--;
            return;
        }

        var action = Decide(x, y, dir);
        if (action == CMD_BLOCK) cloneCount--;
        else if (action == CMD_ELEVATOR) { cloneCount--; elevatorCount--; }

        Console.WriteLine(action);
        rounds--;
    }

    string Decide(int startX, int startY, int startDir)
    {
        visited  = new bool[width, height, cloneCount+1, elevatorCount+1, 2];
        timeline = new List<Unit>[rounds];
        for (int i = 0; i < rounds; i++) timeline[i] = new List<Unit>();

        timeline[0].Add(new Unit(startX, startY, cloneCount, elevatorCount, startDir, CMD_WAIT));
        visited[startX, startY, cloneCount, elevatorCount, startDir] = true;

        for (int t = 0; t < rounds; t++)
        {
            foreach (var u in timeline[t])
            {
                if (u.X == exitX && u.Y == exitY)
                    return u.FirstCommand;
                Expand(u, t);
            }
        }
        return CMD_WAIT;
    }

    void Expand(Unit u, int t)
    {
        int dx = u.Dir == 0 ? -1 : 1;

        // BLOCK
        if (u.Clones > 0 && !hasLift[u.X, u.Y])
        {
            int bx = u.X - dx;
            if (bx >= 0 && bx < width && t + DELAY_BLOCK < rounds && !visited[bx, u.Y, u.Clones-1, u.Elevators, 1-u.Dir])
            {
                timeline[t+DELAY_BLOCK].Add(new Unit(bx, u.Y, u.Clones-1, u.Elevators, 1-u.Dir, t==0?CMD_BLOCK:u.FirstCommand));
                visited[bx, u.Y, u.Clones-1, u.Elevators, 1-u.Dir] = true;
            }
        }
        
        // ELEVATOR
        if (u.Clones > 0 && u.Elevators > 0 && u.Y < exitY && !hasLift[u.X, u.Y])
        {
            if (t + DELAY_ELEVATOR < rounds && !visited[u.X, u.Y+1, u.Clones-1, u.Elevators-1, u.Dir])
            {
                timeline[t+DELAY_ELEVATOR].Add(new Unit(u.X, u.Y+1, u.Clones-1, u.Elevators-1, u.Dir, t==0?CMD_ELEVATOR:u.FirstCommand));
                visited[u.X, u.Y+1, u.Clones-1, u.Elevators-1, u.Dir] = true;
            }
        }

        // CLIMB
        if (hasLift[u.X, u.Y])
        {
            if (t + DELAY_MOVE < rounds && !visited[u.X, u.Y+1, u.Clones, u.Elevators, u.Dir])
            {
                timeline[t+DELAY_MOVE].Add(new Unit(u.X, u.Y+1, u.Clones, u.Elevators, u.Dir, t==0?CMD_WAIT:u.FirstCommand));
                visited[u.X, u.Y+1, u.Clones, u.Elevators, u.Dir] = true;
            }
        }

        // MOVE
        if (!hasLift[u.X, u.Y])
        {
            int nx = u.X + dx;
            if (nx >= 0 && nx < width && t + DELAY_MOVE < rounds && !visited[nx, u.Y, u.Clones, u.Elevators, u.Dir])
            {
                timeline[t+DELAY_MOVE].Add(new Unit(nx, u.Y, u.Clones, u.Elevators, u.Dir, t==0?CMD_WAIT:u.FirstCommand));
                visited[nx, u.Y, u.Clones, u.Elevators, u.Dir] = true;
            }
        }
    }

    class Unit
    {
        public int X, Y, Clones, Elevators, Dir;
        public string FirstCommand;
        public Unit(int x, int y, int c, int e, int d, string f)
        {
            X = x; Y = y; Clones = c; Elevators = e; Dir = d; FirstCommand = f;
        }
    }
}