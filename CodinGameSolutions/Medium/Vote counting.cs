using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        int M = int.Parse(Console.ReadLine());

        var registry = new VoterRegistry();
        for (int i = 0; i < N; i++)
        {
            var p = Console.ReadLine().Split();
            registry.Register(p[0], int.Parse(p[1]));
        }

        var votes = new List<RawVote>();
        for (int i = 0; i < M; i++)
        {
            var p = Console.ReadLine().Split();
            votes.Add(new RawVote(p[0], p[1]));
        }

        var counter = new VoteCounter(registry);
        counter.Tally(votes);

        Console.WriteLine(counter.Result());
    }
}

class RawVote
{
    public string Name { get; }
    public string Choice { get; }
    public RawVote(string name, string choice)
    {
        Name = name;
        Choice = choice;
    }
}

class Voter
{
    public int Allowed { get; }
    public int Used { get; private set; }
    public bool Overvoted { get; private set; }
    private readonly List<string> _votes = new List<string>();

    public Voter(int allowed)
    {
        Allowed = allowed;
    }

    public void Cast(string choice)
    {
        Used++;
        if (Used > Allowed)
            Overvoted = true;
        _votes.Add(choice);
    }

    public IEnumerable<string> ValidVotes
    {
        get
        {
            if (Overvoted)
                yield break;
            foreach (var v in _votes)
                if (v == "Yes" || v == "No")
                    yield return v;
        }
    }
}

class VoterRegistry
{
    private readonly Dictionary<string, Voter> _map = new Dictionary<string, Voter>(StringComparer.OrdinalIgnoreCase);
    public void Register(string name, int allowed)
    {
        _map[name] = new Voter(allowed);
    }
    public bool TryGet(string name, out Voter voter)
    {
        return _map.TryGetValue(name, out voter);
    }
    public IEnumerable<Voter> AllVoters => _map.Values;
}

class VoteCounter
{
    private readonly VoterRegistry _registry;
    private int _yes;
    private int _no;

    public VoteCounter(VoterRegistry registry)
    {
        _registry = registry;
    }

    public void Tally(IEnumerable<RawVote> votes)
    {
        foreach (var vote in votes)
        {
            if (!_registry.TryGet(vote.Name, out var voter))
                continue;
            voter.Cast(vote.Choice);
        }
        foreach (var voter in _registry.AllVoters)
            foreach (var c in voter.ValidVotes)
            {
                if (c == "Yes") _yes++;
                else if (c == "No") _no++;
            }
    }

    public string Result() => $"{_yes} {_no}";
}
