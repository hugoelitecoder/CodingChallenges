using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

class Solution
{
  
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        _width = int.Parse(inputs[0]);
        _height = int.Parse(inputs[1]);
        var numPlayers = int.Parse(Console.ReadLine());
        var numDoors = int.Parse(Console.ReadLine());
        var numTurns = int.Parse(Console.ReadLine());
        var rockets = new List<Point>();
        for (var i = 0; i < numPlayers; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var rX = int.Parse(inputs[0]);
            var rY = int.Parse(inputs[1]);
            rockets.Add(new Point(rX, rY));
        }
        var doors = new List<Director>();
        for (var i = 0; i < numDoors; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var coord = int.Parse(inputs[0]);
            var wall = inputs[1];
            doors.Add(DirectorFor(coord, wall));
        }
        var animals = new List<Animal>();
        var arrows = new List<Director>();
        var scores = Enumerable.Repeat(0, numPlayers).ToList();
        for (var i = 0; i <= numTurns; i++)
        {
            animals.AddRange(((i + 1) % 10 == 0 ? doors.Select(_ => (Animal)new Cat(_)) : doors.Select(_ => (Animal)new Mouse(_))));
            animals.ForEach(_ => _.Move());
            scores = scores.Select((s, j) => Math.Max(0,
                s + animals.Count(m => !m.IsCat && m.Location == rockets[j]) -
                    (10 * animals.Count(c => c.IsCat && c.Location == rockets[j]))))
                .ToList();
            if (i == numTurns) break;
            animals.RemoveAll(a => rockets.Any(r => r == a.Location));
            animals.RemoveAll(_ => IsPit(_.Location));
            animals.RemoveAll(m => !m.IsCat &&
                animals.Any(c => c.IsCat &&
                    (c.Location == m.Location || (c.Location == m.PreviousLocation && c.PreviousLocation == m.Location))));
            arrows.ForEach(_ => animals.Where(a => a.Location == _.Location).ToList().ForEach(a => a.Direction = _.Direction));
            animals.ForEach(a => RedirectByWall(a));
            inputs = Console.ReadLine().Split(' ');
            var tX = int.Parse(inputs[0]);
            var tY = int.Parse(inputs[1]);
            var direction = inputs[2];
            arrows.Add(new Director(new Point(tX, tY), _arrowDirs[direction]));
            if (arrows.Count > numPlayers * 3) arrows.RemoveAt(0);
        }
        Console.WriteLine(string.Join("\n", scores));
    }

      private static readonly Dictionary<string, Point> _arrowDirs = new Dictionary<string, Point> {
        {"W", new Point(-1, 0)}, {"E", new Point(1, 0)},
        {"N", new Point(0, -1)}, {"S", new Point(0, 1)}
    };
    private static int _width;
    private static int _height;
    private class Director
    {
        public Point Location { get; }
        public Point Direction { get; }
        public Director(Point location, Point direction)
        {
            Location = location;
            Direction = direction;
        }
    }
    private abstract class Animal
    {
        public Point Location { get; private set; }
        public Point Direction { get; set; }
        public Point PreviousLocation { get; private set; }
        public bool IsCat { get; protected set; }
        public Animal(Director dir)
        {
            Location = dir.Location;
            Direction = dir.Direction;
            PreviousLocation = new Point(-1, -1);
        }
        public void Move()
        {
            PreviousLocation = Location;
            Location = new Point(Location.X + Direction.X, Location.Y + Direction.Y);
        }
    }
    private class Cat : Animal
    {
        public Cat(Director dir) : base(dir) { IsCat = true; }
    }
    private class Mouse : Animal
    {
        public Mouse(Director dir) : base(dir) { IsCat = false; }
    }
    private static Director DirectorFor(int coord, string wall)
    {
        if (wall == "N") return new Director(new Point(coord, -1), new Point(0, 1));
        if (wall == "S") return new Director(new Point(coord, _height), new Point(0, -1));
        if (wall == "E") return new Director(new Point(_width, coord), new Point(-1, 0));
        if (wall == "W") return new Director(new Point(-1, coord), new Point(1, 0));
        throw new Exception();
    }
    private static bool IsPit(Point loc)
    {
        return (loc.X == 0 && loc.Y == 0)
            || (loc.X == _width - 1 && loc.Y == 0)
            || (loc.X == 0 && loc.Y == _height - 1)
            || (loc.X == _width - 1 && loc.Y == _height - 1);
    }
    private static void RedirectByWall(Animal a)
    {
        if (a.Location.X == 0 && a.Direction.X == -1) a.Direction = new Point(0, 1);
        else if (a.Location.Y == 0 && a.Direction.Y == -1) a.Direction = new Point(-1, 0);
        else if (a.Location.X == _width - 1 && a.Direction.X == 1) a.Direction = new Point(0, -1);
        else if (a.Location.Y == _height - 1 && a.Direction.Y == 1) a.Direction = new Point(1, 0);
    }
}
