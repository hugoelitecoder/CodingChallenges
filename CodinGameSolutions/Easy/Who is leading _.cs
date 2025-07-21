using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var teamNames = Console.ReadLine().Split(',');
        var rawScores1 = Console.ReadLine();
        var rawScores2 = Console.ReadLine();

        var team1 = new RugbyTeam(teamNames[0], rawScores1);
        var team2 = new RugbyTeam(teamNames[1], rawScores2);

        var match = new RugbyMatch(team1, team2);
        match.PlayMinutes(80);

        Console.WriteLine($"{team1.Name}: {team1.TotalScore} {team1.AdvantageTime}");
        Console.WriteLine($"{team2.Name}: {team2.TotalScore} {team2.AdvantageTime}");
    }
}

enum RugbyScoreType
{
    Try = 5,
    Conversion = 2,
    Penalty = 3,
    DroppedGoal = 3
}

class RubgyScoreEvent
{
    public int Minute { get; }
    public int Points { get; }

    public RubgyScoreEvent(int minute, RugbyScoreType type)
    {
        Minute = minute;
        Points = (int)type;
    }
}

class RugbyTeam
{
    public string Name { get; }
    public RubgyScoreEvent[] Events { get; }
    public int TotalScore { get; set; }
    public int AdvantageTime { get; set; }

    public RugbyTeam(string name, string rawScores)
    {
        Name = name;
        Events = ParseEvents(rawScores);
    }

    private RubgyScoreEvent[] ParseEvents(string input)
    {
        var parts = input.Split(',');
        var all = new List<RubgyScoreEvent>();

        void addEvents(string segment, RugbyScoreType type)
        {
            if (string.IsNullOrWhiteSpace(segment)) return;
            foreach (var tok in segment.Split(' '))
                all.Add(new RubgyScoreEvent(int.Parse(tok), type));
        }

        addEvents(parts[0], RugbyScoreType.Try);
        addEvents(parts[1], RugbyScoreType.Conversion);
        addEvents(parts[2], RugbyScoreType.Penalty);
        addEvents(parts[3], RugbyScoreType.DroppedGoal);

        return all.ToArray();
    }

    public int PointsAtMinute(int minute)
    {
        int pts = 0;
        foreach (var e in Events)
            if (e.Minute == minute) pts += e.Points;
        return pts;
    }
}

class RugbyMatch
{
    private readonly RugbyTeam _team1;
    private readonly RugbyTeam _team2;
    private int _cum1, _cum2;

    public RugbyMatch(RugbyTeam team1, RugbyTeam team2)
    {
        _team1 = team1;
        _team2 = team2;
    }

    public void PlayMinutes(int totalMinutes)
    {
        for (int m = 1; m <= totalMinutes; m++)
        {
            _cum1 += _team1.PointsAtMinute(m);
            _cum2 += _team2.PointsAtMinute(m);

            _team1.TotalScore = _cum1;
            _team2.TotalScore = _cum2;

            if (_cum1 > _cum2) _team1.AdvantageTime++;
            else if (_cum2 > _cum1) _team2.AdvantageTime++;
        }
    }
}
