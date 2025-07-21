using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        while (true)
        {
            var pitInfo = Console.ReadLine().Split(' ');
            int pitWidth = int.Parse(pitInfo[0]);
            int pitHeight = int.Parse(pitInfo[1]);
            int pitDepth = int.Parse(pitInfo[2]);
            string pitShape = pitInfo[3];

            var game = new BlockoutGame(pitWidth, pitHeight, pitDepth, pitShape);

            int blockCount = int.Parse(Console.ReadLine());
            var blocks = new List<Block>(blockCount);
            for (var i = 0; i < blockCount; i++)
            {
                var blockInfo = Console.ReadLine().Split(' ');
                blocks.Add(new Block(blockInfo));
            }

            BlockMove bestMove = game.FindBestMove(blocks);

            Console.WriteLine($"{bestMove.BlockIndex} {bestMove.X} {bestMove.Z}");
        }
    }
}

public class BlockoutGame
{
    private readonly int _pitWidth;
    private readonly int _pitHeight;
    private readonly int _pitDepth;
    private readonly bool[,,] _pitGrid;

    public BlockoutGame(int width, int height, int depth, string pitShape)
    {
        _pitWidth = width;
        _pitHeight = height;
        _pitDepth = depth;
        _pitGrid = ParsePit(pitShape);
    }

    public BlockMove FindBestMove(List<Block> blocks)
    {
        BlockMove bestMove = null;
        var bestScore = double.NegativeInfinity;

        foreach (var block in blocks)
        {
            for (var x = 0; x <= _pitWidth - block.Width; x++)
            {
                for (var z = 0; z <= _pitDepth - block.Depth; z++)
                {
                    var landingY = FindLandingY(_pitGrid, block, x, z);

                    if (landingY + block.Height > _pitHeight) continue;

                    var tempPit = (bool[,,])_pitGrid.Clone();
                    PlaceBlock(tempPit, block, x, landingY, z);
                    var (clearedLines, finalPit) = SimulateLineClears(tempPit);

                    var score = EvaluatePit(finalPit, clearedLines);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new BlockMove(block.Index, x, z);
                    }
                }
            }
        }
        return bestMove ?? new BlockMove(blocks[0].Index, 0, 0);
    }

    private bool[,,] ParsePit(string pitShape)
    {
        var grid = new bool[_pitWidth, _pitHeight, _pitDepth];
        var i = 0;
        for (var z = 0; z < _pitDepth; z++)
        for (var y = 0; y < _pitHeight; y++)
        for (var x = 0; x < _pitWidth; x++)
        {
            if (pitShape[i++] == '#')
            {
                grid[x, y, z] = true;
            }
        }
        return grid;
    }

    private double EvaluatePit(bool[,,] pit, int clearedLines)
    {
        var colHeights = GetColumnHeights(pit);
        var aggHeight = colHeights.Length > 0 ? colHeights.Max() : 0;
        var holes = CountHoles(pit);
        var bumpiness = CalculateBumpiness(colHeights);

        return (clearedLines * clearedLines) * 100.0
             - aggHeight * 4.0
             - holes * 10.0
             - bumpiness * 1.5;
    }

    private int FindLandingY(bool[,,] pitGrid, Block block, int x, int z)
    {
        var y = _pitHeight;
        while (true)
        {
            if (CheckCollision(pitGrid, block, x, y - 1, z))
            {
                return y;
            }
            y--;
        }
    }

    private bool CheckCollision(bool[,,] pitGrid, Block block, int x, int y, int z)
    {
        for (var bz = 0; bz < block.Depth; bz++)
        for (var by = 0; by < block.Height; by++)
        for (var bx = 0; bx < block.Width; bx++)
        {
            if (block.Shape[bx, by, bz])
            {
                int pitX = x + bx;
                int pitY = y + by;
                int pitZ = z + bz;

                if (pitY < 0) return true;
                if (pitY >= _pitHeight) continue;
                if (pitGrid[pitX, pitY, pitZ]) return true;
            }
        }
        return false;
    }

    private void PlaceBlock(bool[,,] pitGrid, Block block, int x, int y, int z)
    {
        for (var bz = 0; bz < block.Depth; bz++)
        for (var by = 0; by < block.Height; by++)
        for (var bx = 0; bx < block.Width; bx++)
        {
            if (block.Shape[bx, by, bz])
            {
                var pitY = y + by;
                if (pitY < _pitHeight)
                {
                    pitGrid[x + bx, pitY, z + bz] = true;
                }
            }
        }
    }

    private (int, bool[,,]) SimulateLineClears(bool[,,] tempPit)
    {
        var linesCleared = 0;
        var pit = (bool[,,])tempPit.Clone();

        for (var y = 0; y < _pitHeight; y++)
        {
            var isLayerFull = true;
            for (var x = 0; x < _pitWidth && isLayerFull; x++)
            for (var z = 0; z < _pitDepth; z++)
            {
                if (!pit[x, y, z])
                {
                    isLayerFull = false;
                    break;
                }
            }
            
            if (isLayerFull)
            {
                linesCleared++;
                for (var yShift = y; yShift < _pitHeight - 1; yShift++)
                for (var x = 0; x < _pitWidth; x++)
                for (var z = 0; z < _pitDepth; z++)
                {
                    pit[x, yShift, z] = pit[x, yShift + 1, z];
                }
                
                for (var x = 0; x < _pitWidth; x++)
                for (var z = 0; z < _pitDepth; z++)
                {
                    pit[x, _pitHeight - 1, z] = false;
                }
                y--;
            }
        }
        return (linesCleared, pit);
    }

    private int[] GetColumnHeights(bool[,,] pit)
    {
        var heights = new int[_pitWidth * _pitDepth];
        for (var x = 0; x < _pitWidth; x++)
        for (var z = 0; z < _pitDepth; z++)
        {
            var h = 0;
            for (var y = _pitHeight - 1; y >= 0; y--)
            {
                if (pit[x, y, z])
                {
                    h = y + 1;
                    break;
                }
            }
            heights[z * _pitWidth + x] = h;
        }
        return heights;
    }
    
    private int CountHoles(bool[,,] pit)
    {
        var holes = 0;
        for (var x = 0; x < _pitWidth; x++)
        for (var z = 0; z < _pitDepth; z++)
        {
            var roofFound = false;
            for (var y = _pitHeight - 1; y >= 0; y--)
            {
                if (pit[x, y, z])
                {
                    roofFound = true;
                }
                else if (roofFound)
                {
                    holes++;
                }
            }
        }
        return holes;
    }
    
    private int CalculateBumpiness(int[] colHeights)
    {
        var bumpiness = 0;
        for (var z = 0; z < _pitDepth; z++)
        for (var x = 0; x < _pitWidth; x++)
        {
            var h = colHeights[z * _pitWidth + x];
            if (x + 1 < _pitWidth)
            {
                bumpiness += Math.Abs(h - colHeights[z * _pitWidth + x + 1]);
            }
            if (z + 1 < _pitDepth)
            {
                bumpiness += Math.Abs(h - colHeights[(z + 1) * _pitWidth + x]);
            }
        }
        return bumpiness;
    }
}

public class Block
{
    public int Index { get; }
    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }
    public bool[,,] Shape { get; }

    public Block(string[] input)
    {
        Index = int.Parse(input[0]);
        Width = int.Parse(input[1]);
        Height = int.Parse(input[2]);
        Depth = int.Parse(input[3]);
        var shapeStr = input[4];
        Shape = new bool[Width, Height, Depth];
        var i = 0;
        for (var z = 0; z < Depth; z++)
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            if (shapeStr[i++] == '#')
            {
                Shape[x, y, z] = true;
            }
        }
    }
}

public class BlockMove
{
    public int BlockIndex { get; }
    public int X { get; }
    public int Z { get; }

    public BlockMove(int blockIndex, int x, int z)
    {
        BlockIndex = blockIndex;
        X = x;
        Z = z;
    }
}