using System;


class Solution
{
    static void Main(string[] args)
    {
        var line1Inputs = Console.ReadLine().Split(' ');
        var W = int.Parse(line1Inputs[0]);
        var H = int.Parse(line1Inputs[1]);

        var votersTable = new int[H, W];
        for (var i = 0; i < H; i++)
        {
            var rowInputs = Console.ReadLine().Split(' ');
            for (var j = 0; j < W; j++)
            {
                votersTable[i, j] = int.Parse(rowInputs[j]);
            }
        }

        var optimizer = new DistrictOptimizer(W, H, votersTable);
        var result = optimizer.GetMaxVoters();
        Console.WriteLine(result);
    }
}

class DistrictOptimizer
{
    private readonly int _initialW;
    private readonly int _initialH;
    private readonly int[,] _votersTable;
    private int[,] _dpTable;

    public DistrictOptimizer(int initialW, int initialH, int[,] votersTable)
    {
        _initialW = initialW;
        _initialH = initialH;
        _votersTable = votersTable;
        _dpTable = new int[initialW + 1, initialH + 1];
    }

    public int GetMaxVoters()
    {
        for (var currentW = 1; currentW <= _initialW; currentW++)
        {
            for (var currentH = 1; currentH <= _initialH; currentH++)
            {
                var maxVoters = _votersTable[currentH - 1, currentW - 1];
                if (currentH > 1)
                {
                    for (var h1 = 1; h1 < currentH; h1++)
                    {
                        var votersFromSplit = _dpTable[currentW, h1] + _dpTable[currentW, currentH - h1];
                        maxVoters = Math.Max(maxVoters, votersFromSplit);
                    }
                }

                if (currentW > 1)
                {
                    for (var w1 = 1; w1 < currentW; w1++)
                    {
                        var votersFromSplit = _dpTable[w1, currentH] + _dpTable[currentW - w1, currentH];
                        maxVoters = Math.Max(maxVoters, votersFromSplit);
                    }
                }
                _dpTable[currentW, currentH] = maxVoters;
            }
        }
        return _dpTable[_initialW, _initialH];
    }
}

