using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        int k = int.Parse(Console.ReadLine());
        int m = int.Parse(Console.ReadLine());
        var players = new string[m];
        for (int i = 0; i < m; i++)
            players[i] = Console.ReadLine().Trim();

        Array.Sort(players, StringComparer.Ordinal);

        var builder = new TeamBuilder(n, k, players);
        foreach (var line in builder.GetTeamings())
            Console.WriteLine(line);
    }
}

public class TeamBuilder
{
    readonly int _n, _k, _m;
    readonly string[] _players;
    readonly bool[] _used;
    readonly List<int>[] _teams;
    readonly List<string> _builds = new List<string>();

    public TeamBuilder(int n, int k, string[] players)
    {
        _n = n;
        _k = k;
        _players = players;
        _m = players.Length;
        _used = new bool[_m];
        _teams = Enumerable.Range(0, _n)
                           .Select(_ => new List<int>())
                           .ToArray();
    }

    public List<string> GetTeamings()
    {
        FindTeamBuilds();
        _builds.Sort(StringComparer.Ordinal);
        return _builds;
    }

    void FindTeamBuilds(int teamIndex = 0, int playerOffset = 0)
    {
        if (_teams[_n - 1].Count == _k)
        {
            var sortedNames = _teams
                .Select(t => new string(t.Select(i => _players[i][0]).OrderBy(c => c).ToArray()))
                .OrderBy(name => name, StringComparer.Ordinal);
            _builds.Add(string.Join(",", sortedNames));
            return;
        }

        var current = _teams[teamIndex];
        int needed = _k - current.Count;
        if (needed == 0)
        {
            playerOffset = _teams[teamIndex][0] + 1;
            teamIndex++;
            current = _teams[teamIndex];
            needed = _k - current.Count;
        }

        for (int i = playerOffset; i <= _m - needed; i++)
        {
            if (_used[i]) continue;
            current.Add(i);
            _used[i] = true;

            FindTeamBuilds(teamIndex, i + 1);

            _used[i] = false;
            current.RemoveAt(current.Count - 1);
        }
    }
}
