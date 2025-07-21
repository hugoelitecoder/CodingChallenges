using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var pools = new List<Pool>();
        for (int i = 0; i < 5; i++)
        {
            var names = Console.ReadLine().Split(',');
            var teams = names.Select(n => new Team(n)).ToList();
            pools.Add(new Pool(i, teams));
        }

        for (int i = 0; i < 60; i++)
        {
            var p = Console.ReadLine().Split(',');
            var score = new MatchScore(
                homePoints:  int.Parse(p[1]),
                homeTries:   int.Parse(p[2]),
                awayPoints:  int.Parse(p[4]),
                awayTries:   int.Parse(p[5])
            );

            var home = pools.SelectMany(pl => pl.Teams)
                            .First(t => t.Name == p[0]);
            var away = pools.SelectMany(pl => pl.Teams)
                            .First(t => t.Name == p[3]);

            var match = new Match(home, away, score);

            pools.First(pl => pl.Contains(home))
                 .AddMatch(match);
        }

        var calculator = new ChampionCupCalculator(pools);
        var quarterFinals = calculator.CalculateQuarterFinals();
        foreach (var q in quarterFinals)
            Console.WriteLine($"{q.Home.Name} - {q.Away.Name}");
    }
}

class Team
{
    public string Name { get; }
    public int RankingPoints { get; set; }
    public int PointsFor      { get; set; }
    public int PointsAgainst { get; set; }
    public int PointDifference => PointsFor - PointsAgainst;

    public Team(string name) => Name = name;
}

class Pool
{
    public int Id { get; }
    public List<Team> Teams { get; }
    private readonly List<Match> _matches = new List<Match>();
    public IReadOnlyList<Match> Matches => _matches;

    public Pool(int id, List<Team> teams)
    {
        Id = id;
        Teams = teams;
    }
    public void AddMatch(Match m) => _matches.Add(m);
    public bool Contains(Team t)    => Teams.Contains(t);
}

class MatchScore
{
    public int HomePoints { get; }
    public int HomeTries  { get; }
    public int AwayPoints { get; }
    public int AwayTries  { get; }

    public MatchScore(int homePoints, int homeTries, int awayPoints, int awayTries)
    {
        HomePoints = homePoints;
        HomeTries  = homeTries;
        AwayPoints = awayPoints;
        AwayTries  = awayTries;
    }
}

class Match
{
    public Team Home  { get; }
    public Team Away  { get; }
    public MatchScore Score { get; }

    public Match(Team home, Team away, MatchScore score)
    {
        Home  = home;
        Away  = away;
        Score = score;
    }
}

class MatchPoints
{
    public int Outcome    { get; }
    public int TryBonus   { get; }
    public int LosingBonus{ get; }
    public int Total      => Outcome + TryBonus + LosingBonus;

    public MatchPoints(int outcome, int tryBonus, int losingBonus)
    {
        Outcome     = outcome;
        TryBonus    = tryBonus;
        LosingBonus = losingBonus;
    }
}

class QuarterFinal
{
    public Team Home { get; }
    public Team Away { get; }

    public QuarterFinal(Team home, Team away)
    {
        Home = home;
        Away = away;
    }
}

class ChampionCupCalculator
{
    private readonly List<Pool> _pools;

    public ChampionCupCalculator(List<Pool> pools)
    {
        _pools = pools;
        AwardAllPoints();
    }

    private void AwardAllPoints()
    {
        foreach (var pool in _pools)
            foreach (var match in pool.Matches)
            {
                AwardMatchPoints(match.Home, match);
                AwardMatchPoints(match.Away, match);
                AccumulateScoreTotals(match);
            }
    }

    private void AwardMatchPoints(Team team, Match match)
    {
        var mp = CalculateMatchPoints(team, match);
        team.RankingPoints += mp.Total;
    }

    private MatchPoints CalculateMatchPoints(Team team, Match m)
    {
        bool isHome = m.Home == team;
        int scored    = isHome ? m.Score.HomePoints : m.Score.AwayPoints;
        int conceded  = isHome ? m.Score.AwayPoints : m.Score.HomePoints;
        int tries     = isHome ? m.Score.HomeTries  : m.Score.AwayTries;

        int outcome = scored > conceded ? 4
                    : scored == conceded ? 2 : 0;
        int tryBonus    = tries >= 4 ? 1 : 0;
        int losingBonus = (scored < conceded && conceded - scored <= 7) ? 1 : 0;

        return new MatchPoints(outcome, tryBonus, losingBonus);
    }

    private void AccumulateScoreTotals(Match m)
    {
        m.Home.PointsFor      += m.Score.HomePoints;
        m.Home.PointsAgainst += m.Score.AwayPoints;
        m.Away.PointsFor      += m.Score.AwayPoints;
        m.Away.PointsAgainst += m.Score.HomePoints;
    }

    public List<QuarterFinal> CalculateQuarterFinals()
    {
        var rankedPools = _pools.Select(RankPool).ToList();

        var leaders = rankedPools
            .Select(r => r[0])
            .OrderByDescending(t => t.RankingPoints)
            .ThenByDescending(t => t.PointDifference)
            .ToList();

        var runnersUp = rankedPools
            .Select(r => r[1])
            .OrderByDescending(t => t.RankingPoints)
            .ThenByDescending(t => t.PointDifference)
            .Take(3)
            .ToList();

        return new List<QuarterFinal>
        {
            new QuarterFinal(leaders[0], runnersUp[2]),
            new QuarterFinal(leaders[1], runnersUp[1]),
            new QuarterFinal(leaders[2], runnersUp[0]),
            new QuarterFinal(leaders[3], leaders[4])
        };
    }

    private List<Team> RankPool(Pool pool)
    {
        var sorted = pool.Teams
                         .OrderByDescending(t => t.RankingPoints)
                         .ToList();
        var result = new List<Team>();

        for (int i = 0; i < sorted.Count; )
        {
            int j = i + 1;
            while (j < sorted.Count && sorted[j].RankingPoints == sorted[i].RankingPoints)
                j++;

            var tieBlock = sorted.GetRange(i, j - i);
            if (tieBlock.Count > 1)
            {
                var resolved = tieBlock
                    .Select(team => new {
                        Team  = team,
                        Hrp   = HeadToHeadPoints(team, tieBlock, pool.Matches),
                        Hdiff = HeadToHeadDifference(team, tieBlock, pool.Matches)
                    })
                    .OrderByDescending(x => x.Hrp)
                    .ThenByDescending(x => x.Hdiff)
                    .Select(x => x.Team)
                    .ToList();

                result.AddRange(resolved);
            }
            else
            {
                result.Add(tieBlock[0]);
            }

            i = j;
        }

        return result;
    }

    private int HeadToHeadPoints(Team team, List<Team> block, IEnumerable<Match> matches)
        => matches
           .Where(m => block.Contains(m.Home) && block.Contains(m.Away))
           .Sum(m => CalculateMatchPoints(team, m).Total);

    private int HeadToHeadDifference(Team team, List<Team> block, IEnumerable<Match> matches)
        => matches
           .Where(m => block.Contains(m.Home) && block.Contains(m.Away))
           .Sum(m => m.Home == team
                   ? m.Score.HomePoints - m.Score.AwayPoints
                   : m.Score.AwayPoints - m.Score.HomePoints);
}
