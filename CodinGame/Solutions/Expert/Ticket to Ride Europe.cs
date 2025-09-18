using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var trainCars = int.Parse(inputs[0]);
        var numTickets = int.Parse(inputs[1]);
        var numRoutes = int.Parse(inputs[2]);
        var cardCounts = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var hand = new Hand(trainCars, cardCounts);

        var tickets = new Ticket[numTickets];
        var routes = new Route[numRoutes];

        var cities = new Dictionary<string, int>();
        var cityCounter = 0;
        int AddOrGetCity(string city)
        {
            if (!cities.ContainsKey(city))
            {
                cities[city] = cityCounter++;
            }
            return cities[city];
        }

        for (var i = 0; i < numTickets; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var points = int.Parse(inputs[0]);
            var cityA = AddOrGetCity(inputs[1]);
            var cityB = AddOrGetCity(inputs[2]);
            tickets[i] = new Ticket(points, cityA, cityB);
        }

        var colors = new[] { "Red", "Yellow", "Green", "Blue", "White", "Black", "Orange", "Pink", "Gray" };
        for (var i = 0; i < numRoutes; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var length = int.Parse(inputs[0]);
            var requiredEngines = int.Parse(inputs[1]);
            var color = Array.IndexOf(colors, inputs[2]);
            var cityA = AddOrGetCity(inputs[3]);
            var cityB = AddOrGetCity(inputs[4]);
            routes[i] = new Route(1 << i, length, requiredEngines, color, cityA, cityB);
        }

        var solver = new TicketToRideolver(hand, tickets, routes);
        var maxScore = solver.Solve();
        Console.WriteLine(maxScore);
    }
}

class TicketToRideolver
{
    private readonly Hand _avail;
    private readonly Ticket[] _tickets;
    private readonly Route[] _routes;
    private readonly int[][] _paths;

    private const int ColorCount = 9;
    private const int Gray = 8;
    private static readonly int[] RouteScore = { 0, 1, 2, 4, 7, 0, 15 };

    public TicketToRideolver(Hand avail, Ticket[] tickets, Route[] routes)
    {
        _avail = avail;
        _tickets = tickets;
        _routes = routes;

        _paths = new int[_tickets.Length][];
        for (var i = 0; i < _paths.Length; i++)
        {
            _paths[i] = ComputePaths(_tickets[i].From, _tickets[i].To);
        }
    }

    public int Solve()
    {
        var maxScore = int.MinValue;
        for (var omit = 0; omit <= _routes.Length; omit++)
        {
            var score = GetMaxScore(0, 0, omit);
            if (score > int.MinValue)
            {
                if (maxScore > score) break;
                maxScore = score;
            }
        }
        return maxScore;
    }

    private int GetMaxScore(int index, int build, int omitCount)
    {
        if (omitCount == 0)
        {
            for (var i = index; i < _routes.Length; i++)
            {
                build |= _routes[i].Pow;
            }
            return GetScore(build);
        }

        if (omitCount > _routes.Length - index)
        {
            return int.MinValue;
        }
        if (omitCount == _routes.Length - index)
        {
            return GetScore(build);
        }

        var scoreWithout = GetMaxScore(index + 1, build, omitCount - 1);
        var scoreWith = GetMaxScore(index + 1, build | _routes[index].Pow, omitCount);
        return Math.Max(scoreWith, scoreWithout);
    }

    private int GetScore(int build)
    {
        if (!CanBuild(build))
        {
            return int.MinValue;
        }

        var score = 0;
        for (var i = 0; i < _tickets.Length; i++)
        {
            var ticketScore = -_tickets[i].Points;
            for (var j = 0; j < _paths[i].Length; j++)
            {
                if ((build & _paths[i][j]) == _paths[i][j])
                {
                    ticketScore = _tickets[i].Points;
                    break;
                }
            }
            score += ticketScore;
        }

        for (var i = 0; i < _routes.Length; i++)
        {
            if ((build & _routes[i].Pow) != 0)
            {
                score += RouteScore[_routes[i].Length];
            }
        }
        return score;
    }

    private bool CanBuild(int build)
    {
        var trainCars = 0;
        var costs = new int[ColorCount];
        var open = new List<int>();

        for (var i = 0; i < _routes.Length; i++)
        {
            if ((_routes[i].Pow & build) == 0) continue;

            trainCars += _routes[i].Length;
            costs[Gray] += _routes[i].Engines;
            var rest = _routes[i].Length - _routes[i].Engines;
            if (rest == 0) continue;

            if (_routes[i].Color == Gray)
            {
                open.Add(rest);
            }
            else
            {
                var availColor = _avail.Cards[_routes[i].Color] - costs[_routes[i].Color];
                if (rest <= availColor)
                {
                    costs[_routes[i].Color] += rest;
                }
                else
                {
                    costs[Gray] += rest - availColor;
                    costs[_routes[i].Color] = _avail.Cards[_routes[i].Color];
                }
            }
        }

        if (trainCars > _avail.TrainCars) return false;
        if (costs[Gray] > _avail.Cards[Gray]) return false;

        open.Sort((a, b) => b.CompareTo(a));

        foreach (var gCost in open)
        {
            var bestColorIdx = -1;
            var maxAvail = -1;
            for (var j = 0; j < ColorCount - 1; j++)
            {
                var currentAvail = _avail.Cards[j] - costs[j];
                if (currentAvail > maxAvail)
                {
                    maxAvail = currentAvail;
                    bestColorIdx = j;
                }
            }

            if (bestColorIdx != -1 && maxAvail > 0)
            {
                var useAmount = Math.Min(gCost, maxAvail);
                costs[bestColorIdx] += useAmount;
                costs[Gray] += gCost - useAmount;
            }
            else
            {
                costs[Gray] += gCost;
            }
        }
        return costs[Gray] <= _avail.Cards[Gray];
    }

    private int[] ComputePaths(int from, int to)
    {
        var paths = new List<int>();
        var queue = new (int city, int path)[4096];
        var head = 0;
        var tail = 1;
        queue[0] = (from, 0);

        while (head < tail)
        {
            var (cur, path) = queue[head++];

            var skip = false;
            foreach (var p in paths)
            {
                if ((path & p) == p)
                {
                    skip = true;
                    break;
                }
            }
            if (skip) continue;

            if (cur == to)
            {
                paths.Add(path);
                continue;
            }

            for (var i = 0; i < _routes.Length; i++)
            {
                if ((path & _routes[i].Pow) != 0) continue;

                if (_routes[i].From == cur)
                {
                    if (tail < queue.Length) queue[tail++] = (_routes[i].To, path | _routes[i].Pow);
                }
                else if (_routes[i].To == cur)
                {
                    if (tail < queue.Length) queue[tail++] = (_routes[i].From, path | _routes[i].Pow);
                }
            }
        }
        return paths.ToArray();
    }
}

readonly struct Route
{
    public readonly int Pow;
    public readonly int Length;
    public readonly int Engines;
    public readonly int Color;
    public readonly int From;
    public readonly int To;
    public Route(int pow, int length, int engines, int color, int from, int to)
    {
        Pow = pow; Length = length; Engines = engines; Color = color; From = from; To = to;
    }
}

readonly struct Ticket
{
    public readonly int Points;
    public readonly int From;
    public readonly int To;
    public Ticket(int points, int from, int to)
    {
        Points = points; From = from; To = to;
    }
}

readonly struct Hand
{
    public readonly int TrainCars;
    public readonly int[] Cards;
    public Hand(int trainCars, int[] cards)
    {
        TrainCars = trainCars; Cards = cards;
    }
}