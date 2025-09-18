using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var nStr = Console.ReadLine();
        var n = int.Parse(nStr!);
        var solver = new PigFlightSolver();
        for (var i = 0; i < n; i++)
        {
            var statement = Console.ReadLine()!;
            solver.AddStatement(statement);
        }
        var result = solver.DetermineConclusion();
        Console.WriteLine(result);
    }
}

public class PigFlightSolver
{
    private readonly KnowledgeBase _kb = new();
    private bool _isPrepared = false;

    public void AddStatement(string statement)
    {
        var proposition = PropositionParser.Parse(statement);
        _kb.AddProposition(proposition);
        _isPrepared = false;
    }

    public string DetermineConclusion()
    {
        if (!_isPrepared)
        {
            _kb.PrepareForDeduction();
            _isPrepared = true;
        }
        var pigId = _kb.GetConceptId("PIGS");
        var flyId = _kb.GetConceptId("FLY");
        if (pigId < 0 || flyId < 0)
        {
            return "No pigs can fly";
        }
        if (CanAllPigsFly(pigId, flyId))
        {
            return "All pigs can fly";
        }
        if (CanSomePigsFly(pigId, flyId))
        {
            return "Some pigs can fly";
        }
        return "No pigs can fly";
    }

    private bool CanAllPigsFly(int pigId, int flyId)
    {
        var pigUniversalProperties = _kb.DeduceFinalPropertiesForEntity(new HashSet<int> { pigId });
        return pigUniversalProperties.Contains(flyId);
    }

    private bool CanSomePigsFly(int pigId, int flyId)
    {
        var initialFacts = _kb.GetInitialFacts();
        foreach (var fact in initialFacts)
        {
            var entityProperties = _kb.DeduceFinalPropertiesForEntity(fact);
            if (entityProperties.Contains(pigId) && entityProperties.Contains(flyId))
            {
                return true;
            }
        }
        return false;
    }
}

public class KnowledgeBase
{
    private readonly Dictionary<string, int> _conceptToId = new();
    private readonly List<string> _idToConcept = new();
    private readonly List<Rule> _rules = new();
    private readonly List<HashSet<int>> _initialFacts = new();
    private Dictionary<int, HashSet<int>> _transitiveClosure = new();

    public void AddProposition(Proposition proposition)
    {
        var subjectIds = new HashSet<int>(proposition.Subject.Select(GetOrAddConcept));
        _initialFacts.Add(subjectIds);
        foreach (var predConcept in proposition.Predicate)
        {
            var predId = GetOrAddConcept(predConcept);
            _rules.Add(new Rule(subjectIds, predId));
        }
    }
    
    public int GetConceptId(string concept) => _conceptToId.GetValueOrDefault(concept, -1);
    public IReadOnlyList<HashSet<int>> GetInitialFacts() => _initialFacts;

    public void PrepareForDeduction()
    {
        var simpleImplications = new Dictionary<int, HashSet<int>>();
        for (var i = 0; i < _idToConcept.Count; i++)
        {
            simpleImplications[i] = new HashSet<int>();
        }
        foreach (var rule in _rules.Where(r => r.Antecedent.Count == 1))
        {
            simpleImplications[rule.Antecedent.First()].Add(rule.Consequent);
        }
        ComputeTransitiveClosure(simpleImplications);
    }

    public HashSet<int> DeduceFinalPropertiesForEntity(HashSet<int> initialConcepts)
    {
        var currentConcepts = new HashSet<int>(initialConcepts);
        var wasChanged = true;
        while (wasChanged)
        {
            wasChanged = false;
            var currentProperties = GetFullPropertySet(currentConcepts);
            foreach (var rule in _rules)
            {
                if (rule.Antecedent.IsSubsetOf(currentProperties) && currentConcepts.Add(rule.Consequent))
                {
                    wasChanged = true;
                }
            }
        }
        return GetFullPropertySet(currentConcepts);
    }
    
    private void ComputeTransitiveClosure(Dictionary<int, HashSet<int>> graph)
    {
        _transitiveClosure = new Dictionary<int, HashSet<int>>();
        for (var startNode = 0; startNode < _idToConcept.Count; startNode++)
        {
            var q = new Queue<int>();
            var visited = new HashSet<int>();
            if (visited.Add(startNode))
            {
                q.Enqueue(startNode);
            }
            _transitiveClosure[startNode] = visited;
            while (q.Count > 0)
            {
                var u = q.Dequeue();
                if (graph.TryGetValue(u, out var neighbors))
                {
                    foreach (var v in neighbors.Where(visited.Add))
                    {
                        q.Enqueue(v);
                    }
                }
            }
        }
    }
    
    private int GetOrAddConcept(string concept)
    {
        if (_conceptToId.TryGetValue(concept, out var id))
        {
            return id;
        }
        var newId = _idToConcept.Count;
        _conceptToId[concept] = newId;
        _idToConcept.Add(concept);
        return newId;
    }
    
    private HashSet<int> GetFullPropertySet(HashSet<int> concepts)
    {
        var properties = new HashSet<int>();
        foreach (var conceptId in concepts)
        {
            if (_transitiveClosure.TryGetValue(conceptId, out var implications))
            {
                properties.UnionWith(implications);
            }
        }
        return properties;
    }
}

public record Proposition(HashSet<string> Subject, HashSet<string> Predicate);

internal record Rule(HashSet<int> Antecedent, int Consequent);

public static class PropositionParser
{
    public static Proposition Parse(string statement)
    {
        var tokens = statement.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var (subjectConcepts, verbIndex) = ParseObject(tokens, 0);
        var verb = tokens[verbIndex];
        var predicateConcepts = new HashSet<string>();
        if (verb == "are")
        {
            (predicateConcepts, _) = ParseObject(tokens, verbIndex + 1);
        }
        else
        {
            var predicateTokens = tokens.Skip(verbIndex + 1).ToArray();
            predicateConcepts.Add(string.Join(" ", predicateTokens));
        }
        return new Proposition(subjectConcepts, predicateConcepts);
    }

    private static (HashSet<string> concepts, int nextIndex) ParseObject(string[] tokens, int startIndex)
    {
        var concepts = new HashSet<string>();
        var currentIndex = startIndex;
        if (currentIndex >= tokens.Length) return (concepts, currentIndex);
        concepts.Add(tokens[currentIndex++]);
        if (currentIndex < tokens.Length && tokens[currentIndex] == "with")
        {
            currentIndex++;
            concepts.Add(tokens[currentIndex++]);
            while (currentIndex < tokens.Length && tokens[currentIndex] == "and")
            {
                currentIndex++;
                concepts.Add(tokens[currentIndex++]);
            }
        }
        if (currentIndex + 1 < tokens.Length && tokens[currentIndex] == "that" && tokens[currentIndex + 1] == "can")
        {
            currentIndex += 2;
            concepts.Add(tokens[currentIndex++]);
            while (currentIndex < tokens.Length && tokens[currentIndex] == "and")
            {
                currentIndex++;
                concepts.Add(tokens[currentIndex++]);
            }
        }
        return (concepts, currentIndex);
    }
}