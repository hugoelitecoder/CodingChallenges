using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class Player
{
    static void Main(string[] args)
    {
        string magicPhrase = Console.ReadLine();
        var solver = new CodeOfTheRings(magicPhrase);
        string result = solver.Solve();
        Console.WriteLine(result);
    }
}

class CodeOfTheRings
{
    private readonly string _magicPhrase;
    private const int MEMORY_SIZE = 30;
    private const int ALPHABET_SIZE = 27;
    private const int LOOKAHEAD = 60;
    private const int MAX_LOOP_COUNT = 26;

    public CodeOfTheRings(string phrase)
    {
        _magicPhrase = phrase;
    }

    public string Solve()
    {
        var output = new StringBuilder();
        var memory = new int[MEMORY_SIZE];
        int pointer = 0;
        int phraseIdx = 0;

        while (phraseIdx < _magicPhrase.Length)
        {
            var greedyChoice = GenerateGreedyRun(_magicPhrase[phraseIdx], pointer, memory);
            var bestChoice = (
                k: 1,
                ops: greedyChoice.ops,
                finalPointer: greedyChoice.finalPointer,
                finalMemory: greedyChoice.finalMemory,
                type: "Greedy"
            );
            double minCostPerChar = (double)bestChoice.ops.Length;

            int limit = Math.Min(_magicPhrase.Length, phraseIdx + LOOKAHEAD);
            for (int k = 2; k <= limit - phraseIdx; k++)
            {
                string sub = _magicPhrase.Substring(phraseIdx, k);

                var (isArith, diff) = CheckArithmetic(sub);
                if (isArith)
                {
                    var arithResult = GenerateArithmeticSequence(sub, diff, pointer, memory);
                    if (arithResult.ops != null)
                    {
                        double costPerChar = (double)arithResult.ops.Length / k;
                        if (costPerChar < minCostPerChar)
                        {
                            minCostPerChar = costPerChar;
                            bestChoice = (k, arithResult.ops, arithResult.finalPointer, arithResult.finalMemory, "Arithmetic Seq");
                        }
                    }
                }
                
                var (unit, count) = FindRepeatingUnit(sub);
                if (count > 1)
                {
                    int k_to_gen = Math.Min(count, MAX_LOOP_COUNT);
                    int real_k = unit.Length * k_to_gen;
                    var loopResult = GenerateLoop(unit, k_to_gen, pointer, memory);
                    if (loopResult.ops != null)
                    {
                        double costPerChar = (double)loopResult.ops.Length / real_k;
                        if (costPerChar < minCostPerChar)
                        {
                            minCostPerChar = costPerChar;
                            bestChoice = (real_k, loopResult.ops, loopResult.finalPointer, loopResult.finalMemory, "Subsequence Loop");
                        }
                    }
                }
            }
            
            output.Append(bestChoice.ops);
            pointer = bestChoice.finalPointer;
            memory = bestChoice.finalMemory;
            phraseIdx += bestChoice.k;
        }
        
        string finalSolution = output.ToString();
        double ratio = _magicPhrase.Length > 0 ? (double)finalSolution.Length / _magicPhrase.Length : 0;

        Console.Error.WriteLine("[DEBUG] ==============================================");
        Console.Error.WriteLine("[DEBUG]                  FINAL STATS                 ");
        Console.Error.WriteLine("[DEBUG] ==============================================");
        Console.Error.WriteLine($"[DEBUG] Input Phrase: \"{_magicPhrase}\"");
        Console.Error.WriteLine($"[DEBUG] Output Commands: \"{finalSolution}\"");
        Console.Error.WriteLine("[DEBUG] ----------------------------------------------");
        Console.Error.WriteLine($"[DEBUG] Input Length:    {_magicPhrase.Length}");
        Console.Error.WriteLine($"[DEBUG] Output Length:   {finalSolution.Length}");
        Console.Error.WriteLine($"[DEBUG] Compression:     {ratio:F3} (output/input)");
        Console.Error.WriteLine("[DEBUG] ==============================================");

        return finalSolution;
    }
    
    private (string ops, int finalPointer, int[] finalMemory) GenerateGreedyRun(char c, int startPointer, int[] startMemory)
    {
        return FindBestGreedyStep(startPointer, startMemory, c);
    }

    private (string ops, int finalPointer, int[] finalMemory) GenerateLoop(string unit, int k, int startPointer, int[] startMemory)
    {
        string bestOps = null;
        int bestCost = int.MaxValue;
        (int, int[]) bestFinalState = (0, null);

        for (int p_start = 0; p_start < MEMORY_SIZE; p_start++) {
            for (int p_counter_offset = unit.Length; p_counter_offset < MEMORY_SIZE; p_counter_offset++) {
                int p_counter = (p_start + p_counter_offset) % MEMORY_SIZE;
                
                var currentOps = new StringBuilder();
                var tempMemory = (int[])startMemory.Clone();
                int tempPointer = startPointer;

                for (int i = 0; i < unit.Length; i++) {
                    int charCell = (p_start + i) % MEMORY_SIZE;
                    currentOps.Append(GetMoveOps(tempPointer, charCell));
                    tempPointer = charCell;
                    currentOps.Append(GetChangeOps(tempMemory[tempPointer], CharToVal(unit[i])));
                    tempMemory[tempPointer] = CharToVal(unit[i]);
                }

                currentOps.Append(GetMoveOps(tempPointer, p_counter));
                tempPointer = p_counter;
                currentOps.Append(GetChangeOps(tempMemory[tempPointer], k));
                tempMemory[tempPointer] = k;
                
                var loopBody = new StringBuilder();
                int loopPtr = p_counter;
                for (int i = 0; i < unit.Length; i++) {
                    int charCell = (p_start + i) % MEMORY_SIZE;
                    loopBody.Append(GetMoveOps(loopPtr, charCell) + ".");
                    loopPtr = charCell;
                }
                loopBody.Append(GetMoveOps(loopPtr, p_counter) + "-");
                
                currentOps.Append($"[{loopBody}]");
                tempMemory[p_counter] = 0;
                tempPointer = p_counter;

                if (currentOps.Length < bestCost) {
                    bestCost = currentOps.Length;
                    bestOps = currentOps.ToString();
                    bestFinalState = (tempPointer, tempMemory);
                }
            }
        }
        return (bestOps, bestFinalState.Item1, bestFinalState.Item2);
    }

    private (string ops, int finalPointer, int[] finalMemory) GenerateArithmeticSequence(string s, int diff, int startPointer, int[] startMemory)
    {
        string bestOps = null;
        int bestCost = int.MaxValue;
        (int, int[]) bestFinalState = (0, null);

        for (int i = 0; i < MEMORY_SIZE; i++) {
            var currentOps = new StringBuilder();
            var tempMemory = (int[])startMemory.Clone();
            int tempPointer = startPointer;
            currentOps.Append(GetMoveOps(tempPointer, i));
            tempPointer = i;
            
            int firstVal = CharToVal(s[0]);
            currentOps.Append(GetChangeOps(tempMemory[tempPointer], firstVal));
            tempMemory[tempPointer] = firstVal;
            currentOps.Append('.');

            string diffOps = GetChangeOps(0, diff);
            int currentVal = firstVal;
            for (int j = 1; j < s.Length; j++) {
                currentOps.Append(diffOps);
                currentOps.Append('.');
                currentVal = (currentVal + diff + ALPHABET_SIZE) % ALPHABET_SIZE;
            }
            tempMemory[tempPointer] = currentVal;

            if (currentOps.Length < bestCost) {
                bestCost = currentOps.Length;
                bestOps = currentOps.ToString();
                bestFinalState = (tempPointer, tempMemory);
            }
        }
        return (bestOps, bestFinalState.Item1, bestFinalState.Item2);
    }
    
    private (string ops, int finalPointer, int[] finalMemory) FindBestGreedyStep(int startPointer, int[] startMemory, char targetChar)
    {
        int targetVal = CharToVal(targetChar);
        string bestOps = "";
        int bestFinalPointer = 0;
        int minCost = int.MaxValue;

        for (int i = 0; i < MEMORY_SIZE; i++) {
            string moveOps = GetMoveOps(startPointer, i);
            string changeOps = GetChangeOps(startMemory[i], targetVal);
            int currentCost = moveOps.Length + changeOps.Length + 1;
            if (currentCost < minCost) {
                minCost = currentCost;
                bestOps = moveOps + changeOps + ".";
                bestFinalPointer = i;
            }
        }
        var finalMemory = (int[])startMemory.Clone();
        finalMemory[bestFinalPointer] = targetVal;
        return (bestOps, bestFinalPointer, finalMemory);
    }

    private (bool, int) CheckArithmetic(string s)
    {
        if (s.Length < 3) return (false, 0);
        int val1 = CharToVal(s[0]);
        int val2 = CharToVal(s[1]);
        int diff = (val2 - val1 + ALPHABET_SIZE) % ALPHABET_SIZE;
        if (diff == 0) return (false, 0);
        for (int i = 2; i < s.Length; i++) {
            int prevVal = CharToVal(s[i - 1]);
            int currentVal = CharToVal(s[i]);
            if (currentVal != (prevVal + diff + ALPHABET_SIZE) % ALPHABET_SIZE) return (false, 0);
        }
        return (true, diff);
    }
    
    private (string unit, int count) FindRepeatingUnit(string s)
    {
        int n = s.Length;
        for (int l = 1; l <= n / 2; l++) {
            if (n % l == 0) {
                string unit = s.Substring(0, l);
                int count = n / l;
                bool isRepeat = true;
                for (int i = 1; i < count; i++) {
                    if (s.Substring(i * l, l) != unit) { isRepeat = false; break; }
                }
                if (isRepeat) return (unit, count);
            }
        }
        return (s, 1);
    }

    private string GetMoveOps(int from, int to)
    {
        int distRight = (to - from + MEMORY_SIZE) % MEMORY_SIZE;
        int distLeft = (from - to + MEMORY_SIZE) % MEMORY_SIZE;
        return distRight <= distLeft ? new string('>', distRight) : new string('<', distLeft);
    }

    private string GetChangeOps(int from, int to)
    {
        int diffUp = (to - from + ALPHABET_SIZE) % ALPHABET_SIZE;
        int diffDown = (from - to + ALPHABET_SIZE) % ALPHABET_SIZE;
        return diffUp <= diffDown ? new string('+', diffUp) : new string('-', diffDown);
    }

    private static int CharToVal(char c) => c == ' ' ? 0 : c - 'A' + 1;
}