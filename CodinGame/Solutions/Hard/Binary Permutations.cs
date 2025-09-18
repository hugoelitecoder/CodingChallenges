using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var tokens = Console.ReadLine().Split();
        var numBits = int.Parse(tokens[0]);
        var numClues = int.Parse(tokens[1]);
        var clues = new List<BitPermutationClue>();
        for (var i = 0; i < numClues; i++)
        {
            var pair = Console.ReadLine().Split();
            var xi = int.Parse(pair[0]);
            var yi = int.Parse(pair[1]);
            clues.Add(BitPermutationClue.FromInts(xi, yi, numBits));
        }
        var solver = new BitPermutationSolver(numBits, clues);
        var mapping = solver.DeduceMapping();
        Console.WriteLine(string.Join(" ", mapping));
    }
}

class BitPermutationClue
{
    public string InputBits { get; }
    public string OutputBits { get; }
    private BitPermutationClue(string inputBits, string outputBits)
    {
        InputBits = inputBits;
        OutputBits = outputBits;
    }
    public static BitPermutationClue FromInts(int input, int output, int numBits)
    {
        var inBits = Convert.ToString(input, 2).PadLeft(numBits, '0');
        var outBits = Convert.ToString(output, 2).PadLeft(numBits, '0');
        return new BitPermutationClue(inBits, outBits);
    }
}

class BitPermutationSolver
{
    private readonly int _numBits;
    private readonly List<BitPermutationClue> _clues;
    public BitPermutationSolver(int numBits, List<BitPermutationClue> clues)
    {
        _numBits = numBits;
        _clues = clues;
    }
    public List<int> DeduceMapping()
    {
        var result = new List<int>();
        for (var i = 0; i < _numBits; i++)
        {
            for (var j = 0; j < _numBits; j++)
            {
                var valid = true;
                foreach (var clue in _clues)
                {
                    if (clue.InputBits[_numBits - 1 - i] != clue.OutputBits[_numBits - 1 - j])
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    result.Add(1 << j);
                    break;
                }
            }
        }
        return result;
    }
}
