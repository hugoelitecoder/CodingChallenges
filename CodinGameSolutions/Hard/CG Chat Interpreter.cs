using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Solution
{
    private static string _context;

    static void Main(string[] args)
    {
        Console.ReadLine();
        var goodNouns = Console.ReadLine().Split(' ')
            .Where(_ => !string.IsNullOrEmpty(_)).ToArray();
        var badNouns = Console.ReadLine().Split(' ')
            .Where(_ => !string.IsNullOrEmpty(_)).ToArray();
        var lines = Enumerable.Range(0, int.Parse(Console.ReadLine()))
            .Select(_ => Console.ReadLine()).ToList();

        var stacks = new Dictionary<string, Stack<int>>();
        var contexts = new Dictionary<string, string>();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            line = Regex.Replace(line, Patterns.Punctuation, string.Empty);
            Console.Error.WriteLine($"{i} {line}");
            var speaker = GetSpeakerForLine(line);
            if (string.IsNullOrEmpty(speaker)) continue;
            if (!stacks.ContainsKey(speaker)) stacks[speaker] = new Stack<int>();
            contexts = CheckForContextChangeAtStartOfMessage(contexts, speaker, line);
            Stack<int> stack = null;
            string context;
            if (contexts.TryGetValue(speaker, out context))
            {
                if (!stacks.ContainsKey(context)) stacks[context] = new Stack<int>();
                stack = stacks[context];
                _context = contexts[speaker];
                Console.Error.WriteLine($"-> {_context}");
            }
            line = ReplaceAllConstants(line, goodNouns, badNouns);
            line = ReplaceAllArithmeticOperations(line, speaker, stack, stacks);
            line = PrepareAssignment(line, speaker, stack, stacks);
            stack = ProcessAssignment(line, stack);
            stack = ProcessDup(line, stack);
            stack = ProcessPop(line, stack);
            stack = ProcessSwap(line, stack);
            stack = ProcessPrintChar(line, stack);
            stack = ProcessPrintInt(line, stack);
            contexts = CheckForContextChangeAnywhereInMessage(contexts, speaker, line);
            if (Regex.Match(line, Patterns.Stop, RegexOptions.IgnoreCase).Success) return;
        }
    }

    static class Patterns
    {
        public const string Parameter = @"\*[^\*]+\*|-?\b\d+\b|\bme\b|\byou\b|\bu\b";
        public const string GoodOrBadNounPhrase = @"\ban?\b(?<phrase>(?:[\w\s,]+?)\b(?:" + NounListPlaceholder + @"){1}\b)";
        public const string AdjectiveNounInPhrase = @"^(?<adjectives>[\w\s,]+?)\b(?<noun>" + NounListPlaceholder + @"){1}\b";
        public const string SpeakerLine = @"^<(?<speaker>[^>]+)>";
        public const string ContextChangeStart = @"^<[^>]+> \*(?<nick>[^\*]+)\*";
        public const string ContextChangeAny = @"\*(?<nick>[^\*]+)\*";
        public const string Assignment = @"\b(?:youre|your|ur)\b.*?(?<constant>\*[^\*]+\*|-?\b\d+\b|\bme\b|\byou\b)";
        public const string AssignmentValue = @"\b(?:youre|your|ur)\b.*?(?<constant>-?\b\d+\b)";
        public const string Dup = @"\blisten\b";
        public const string Pop = @"\bforget\b";
        public const string Swap = @"\bflip\b";
        public const string PrintChar = @"\bsay\b";
        public const string PrintInt = @"\btell(ing)?\b";
        public const string Stop = @" stop";
        public const string Punctuation = @"[!?.,;:""']";
        public const string NounListPlaceholder = "{NOUNLIST}";
    }

    static class PatternBuilder
    {
        public static string GoodOrBadNounPhrase(string[] good, string[] bad) =>
            Patterns.GoodOrBadNounPhrase.Replace(Patterns.NounListPlaceholder, string.Join("|", good.Concat(bad)));
        public static string AdjectiveNounInPhrase(string[] good, string[] bad) =>
            Patterns.AdjectiveNounInPhrase.Replace(Patterns.NounListPlaceholder, string.Join("|", good.Concat(bad)));
    }

    private static string ReplaceAllArithmeticOperations(string line, string speaker,
        Stack<int> stack, Dictionary<string, Stack<int>> stacks)
    {
        if (stack == null) return line;
        var result = line;
        string before;
        do
        {
            before = result;
            result = ReplaceAllSquares(result, speaker, stack, stacks);
            result = ReplaceAllCubes(result, speaker, stack, stacks);
            result = ReplaceAllSums(result, speaker, stack, stacks);
            result = ReplaceAllDifferences(result, speaker, stack, stacks);
            result = ReplaceAllProducts(result, speaker, stack, stacks);
        } while (result != before);
        return result;
    }

    private static string GetSpeakerForLine(string line)
    {
        var match = Regex.Match(line, Patterns.SpeakerLine);
        return match.Success ? match.Groups["speaker"].Value : string.Empty;
    }

    private static Dictionary<string, string> CheckForContextChangeAtStartOfMessage(
        Dictionary<string, string> contexts, string speaker, string line)
    {
        var match = Regex.Match(line, Patterns.ContextChangeStart);
        if (!match.Success) return contexts;
        contexts[speaker] = match.Groups["nick"].Value;
        return contexts;
    }

    private static Dictionary<string, string> CheckForContextChangeAnywhereInMessage(
        Dictionary<string, string> contexts, string speaker, string line)
    {
        var match = Regex.Match(line, Patterns.ContextChangeAny);
        if (!match.Success) return contexts;
        contexts[speaker] = match.Groups["nick"].Value;
        return contexts;
    }

    private static string ReplaceAllConstants(string line, string[] good, string[] bad)
    {
        var result = line;
        var nounPhrase = PatternBuilder.GoodOrBadNounPhrase(good, bad);
        result = ProcessAllMatches(result, nounPhrase, match =>
        {
            var phrase = match.Groups["phrase"].Value;
            var phraseMatchStr = PatternBuilder.AdjectiveNounInPhrase(good, bad);
            var phraseMatch = Regex.Match(phrase, phraseMatchStr, RegexOptions.IgnoreCase);
            var adjectives = phraseMatch.Groups["adjectives"].Value.Trim();
            var noun = phraseMatch.Groups["noun"].Value;
            var baseValue = good.Contains(noun) ? 1 : -1;
            var exp = string.IsNullOrEmpty(adjectives) ? 0 : adjectives.Split(' ').Length;
            return $"{Math.Pow(2, exp) * baseValue}";
        });
        return result;
    }

    private static string ProcessAllMatches(string line, string matchStr,
        Func<Match, string> processor)
    {
        var result = line;
        var matches = Regex.Matches(result, matchStr, RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            result = result.Replace(match.Value, processor(match));
            Console.Error.WriteLine(result);
        }
        return result;
    }

    private static string ReplaceAllSquares(string line, string speaker, Stack<int> stack,
        Dictionary<string, Stack<int>> stacks)
    {
        var matchStr = $"(?<parameter>{Patterns.Parameter})\\s+squared\\b";
        return ProcessAllMatches(line, matchStr, match =>
        {
            var value = GetConstantValue(match.Groups["parameter"].Value, speaker, stack, stacks);
            return $"{Math.Pow(value, 2)}";
        });
    }

    private static int GetConstantValue(string constant, string speaker, Stack<int> stack,
        Dictionary<string, Stack<int>> stacks)
    {
        var c = constant;
        if (c.ToLower() == "me" && stacks[speaker].Any()) return stacks[speaker].Peek();
        if ((c.ToLower() == "you" || c.ToLower() == "u") && stack.Any()) return stack.Peek();
        if (c.StartsWith("*") && stacks[c.Trim('*')].Any()) return stacks[c.Trim('*')].Peek();
        var i = 0;
        return int.TryParse(c, out i) ? i : 0;
    }

    private static string ReplaceAllCubes(string line, string speaker, Stack<int> stack,
        Dictionary<string, Stack<int>> stacks)
    {
        var matchStr = $"(?<parameter>{Patterns.Parameter})\\s+cubed\\b";
        return ProcessAllMatches(line, matchStr, match =>
        {
            var value = GetConstantValue(match.Groups["parameter"].Value, speaker, stack, stacks);
            return $"{Math.Pow(value, 3)}";
        });
    }

    private static string ReplaceAllSums(string line, string speaker, Stack<int> stack,
        Dictionary<string, Stack<int>> stacks)
    {
        var matchStr = $"(?<parameter1>{Patterns.Parameter})\\s+and\\s+(?<parameter2>{Patterns.Parameter})\\s+too\\b";
        return ProcessAllMatches(line, matchStr, match =>
        {
            var value1 = GetConstantValue(match.Groups["parameter1"].Value, speaker, stack, stacks);
            var value2 = GetConstantValue(match.Groups["parameter2"].Value, speaker, stack, stacks);
            return $"{value1 + value2}";
        });
    }

    private static string ReplaceAllDifferences(string line, string speaker,
        Stack<int> stack, Dictionary<string, Stack<int>> stacks)
    {
        var matchStr = $"(?<parameter1>{Patterns.Parameter})\\s+but not\\s+(?<parameter2>{Patterns.Parameter})\\s+though\\b";
        return ProcessAllMatches(line, matchStr, match =>
        {
            var value1 = GetConstantValue(match.Groups["parameter1"].Value, speaker, stack, stacks);
            var value2 = GetConstantValue(match.Groups["parameter2"].Value, speaker, stack, stacks);
            return $"{value1 - value2}";
        });
    }

    private static string ReplaceAllProducts(string line, string speaker, Stack<int> stack,
        Dictionary<string, Stack<int>> stacks)
    {
        var matchStr = $"(?<parameter1>{Patterns.Parameter})\\s+by\\s+(?<parameter2>{Patterns.Parameter})\\s+multiplied\\b";
        return ProcessAllMatches(line, matchStr, match =>
        {
            var value1 = GetConstantValue(match.Groups["parameter1"].Value, speaker, stack, stacks);
            var value2 = GetConstantValue(match.Groups["parameter2"].Value, speaker, stack, stacks);
            return $"{value1 * value2}";
        });
    }

    private static string PrepareAssignment(string line, string speaker,
        Stack<int> stack, Dictionary<string, Stack<int>> stacks)
    {
        if (stack == null) return line;
        var matchStr = Patterns.Assignment;
        var match = Regex.Match(line, matchStr, RegexOptions.IgnoreCase);
        if (!match.Success) return line;
        var constant = match.Groups["constant"].Value;
        var value = GetConstantValue(constant, speaker, stack, stacks);
        var replaced = match.Value.Replace(constant, value.ToString());
        return line.Replace(match.Value, replaced);
    }

    private static Stack<int> ProcessAssignment(string line, Stack<int> stack)
    {
        if (stack == null) return null;
        var matchStr = Patterns.AssignmentValue;
        var match = Regex.Match(line, matchStr, RegexOptions.IgnoreCase);
        if (!match.Success) return stack;
        var value = int.Parse(match.Groups["constant"].Value);
        stack.Push(value);
        Console.Error.WriteLine($"{_context}: {value} [ {string.Join(" ", stack.Reverse())} ]");
        return stack;
    }

    private static Stack<int> ProcessDup(string line, Stack<int> stack)
    {
        if (stack == null) return null;
        if (Regex.Match(line, Patterns.Dup, RegexOptions.IgnoreCase).Success)
        {
            stack.Push(stack.Peek());
            Console.Error.WriteLine($"Dup {stack.Peek()}");
        }
        return stack;
    }

    private static Stack<int> ProcessPop(string line, Stack<int> stack)
    {
        if (stack == null) return null;
        if (Regex.Match(line, Patterns.Pop, RegexOptions.IgnoreCase).Success) stack.Pop();
        return stack;
    }

    private static Stack<int> ProcessSwap(string line, Stack<int> stack)
    {
        if (stack == null) return null;
        if (Regex.Match(line, Patterns.Swap, RegexOptions.IgnoreCase).Success)
        {
            var tmp1 = stack.Pop();
            var tmp2 = stack.Pop();
            stack.Push(tmp1);
            stack.Push(tmp2);
        }
        return stack;
    }

    private static Stack<int> ProcessPrintChar(string line, Stack<int> stack)
    {
        if (stack == null) return null;
        var matches = Regex.Matches(line, Patterns.PrintChar, RegexOptions.IgnoreCase);
        for (var i = 0; i < matches.Count; i++)
        {
            Console.Write(((char)stack.Pop()).ToString());
        }
        return stack;
    }

    private static Stack<int> ProcessPrintInt(string line, Stack<int> stack)
    {
        if (stack == null) return null;
        var matches = Regex.Matches(line, Patterns.PrintInt, RegexOptions.IgnoreCase);
        for (var i = 0; i < matches.Count; i++)
        {
            Console.Write(stack.Pop());
        }
        return stack;
    }
}
