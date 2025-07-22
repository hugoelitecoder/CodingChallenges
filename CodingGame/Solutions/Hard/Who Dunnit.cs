using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var L = int.Parse(inputs[0]);
        var N = int.Parse(inputs[1]);
        var evidence = new List<HashSet<string>>();
        for (var i = 0; i < L; i++)
        {
            var line = Console.ReadLine();
            var clues = line.Split(new[] { ", " }, StringSplitOptions.None).ToHashSet();
            evidence.Add(clues);
        }
        var solver = new MurderMysterySolver();
        var culprit = solver.FindCulprit(evidence);
        Console.WriteLine(culprit);
    }
}

public class MurderMysterySolver
{
    public string FindCulprit(List<HashSet<string>> evidence)
    {
        var suspects = evidence[0];
        foreach (var suspect in suspects)
        {
            var found = new HashSet<string> { suspect };
            var knowledgeRevision = ReviseKnowledge(suspect, new HashSet<string>(), evidence);
            if (CheckHypothesis(found, knowledgeRevision.Cleared, knowledgeRevision.UncheckedEvidence))
            {
                return suspect;
            }
        }
        return null;
    }
    
    private bool CheckHypothesis(
        HashSet<string> found, 
        HashSet<string> cleared, 
        List<HashSet<string>> uncheckedEvidence)
    {
        if (uncheckedEvidence.Count == 0)
        {
            return true;
        }
        if (uncheckedEvidence.Any(clues => clues.IsSubsetOf(cleared)))
        {
            return false;
        }
        var setToProcess = uncheckedEvidence
            .OrderBy(s => s.Count(clue => !cleared.Contains(clue)))
            .First();
        var potentialTruths = setToProcess.Where(clue => !cleared.Contains(clue)).ToList();
        foreach (var hypothetical in potentialTruths)
        {
            found.Add(hypothetical);
            var knowledgeRevision = ReviseKnowledge(hypothetical, cleared, uncheckedEvidence);
            if (CheckHypothesis(found, knowledgeRevision.Cleared, knowledgeRevision.UncheckedEvidence))
            {
                return true;
            }
            found.Remove(hypothetical);
        }
        return false;
    }

    private KnowledgeRevisionResult ReviseKnowledge(
        string hypothetical, 
        HashSet<string> cleared, 
        List<HashSet<string>> allEvidence)
    {
        var newCleared = new HashSet<string>(cleared);
        var newUnchecked = new List<HashSet<string>>();
        foreach (var clues in allEvidence)
        {
            if (clues.Contains(hypothetical))
            {
                newCleared.UnionWith(clues);
            }
            else
            {
                newUnchecked.Add(clues);
            }
        }
        newCleared.Remove(hypothetical);
        return new KnowledgeRevisionResult(newCleared, newUnchecked);
    }
}

public class KnowledgeRevisionResult
{
    public HashSet<string> Cleared { get; }
    public List<HashSet<string>> UncheckedEvidence { get; }

    public KnowledgeRevisionResult(HashSet<string> cleared, List<HashSet<string>> uncheckedEvidence)
    {
        Cleared = cleared;
        UncheckedEvidence = uncheckedEvidence;
    }
}
