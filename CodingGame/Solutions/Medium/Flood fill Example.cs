using System;
using System.Collections.Generic;
using System.Text;

namespace DefenseAreaCoverage
{
    class Program
    {
        static void Main()
        {
            int width = int.Parse(Console.ReadLine());
            int height = int.Parse(Console.ReadLine());

            bool[,] isPassable = new bool[height, width];
            int[,] distanceMap = new int[height, width];
            char[,] territoryMap = new char[height, width];
            var sources = new List<(int row, int col, char id)>();

            for (int row = 0; row < height; row++)
            {
                string line = Console.ReadLine();
                for (int col = 0; col < width; col++)
                {
                    char symbol = line[col];

                    isPassable[row, col] = symbol != '#';
                    territoryMap[row, col] = '.';

                    if (symbol != '.' && symbol != '#')
                        sources.Add((row, col, symbol));

                    distanceMap[row, col] = width + height + 1;
                }
            }

            var processor = new VoronoiProcessor(
                width,
                height,
                isPassable,
                distanceMap,
                territoryMap,
                sources);

            processor.Run();

            var output = new StringBuilder();
            for (int row = 0; row < height; row++)
            {
                output.Clear();
                for (int col = 0; col < width; col++)
                {
                    if (!isPassable[row, col])
                        output.Append('#');
                    else
                        output.Append(territoryMap[row, col]);
                }
                Console.WriteLine(output);
            }
        }
    }

    class VoronoiProcessor
    {
        private int _width;
        private int _height;
        private bool[,] _isPassable;
        private int[,] _distanceMap;
        private char[,] _territoryMap;
        private List<(int row, int col, char id)> _sources;
        private static readonly int[,] _directions = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

        public VoronoiProcessor(
            int width,
            int height,
            bool[,] isPassable,
            int[,] distanceMap,
            char[,] territoryMap,
            List<(int row, int col, char id)> sources)
        {
            _width = width;
            _height = height;
            _isPassable = isPassable;
            _distanceMap = distanceMap;
            _territoryMap = territoryMap;
            _sources = sources;
        }

        public void Run()
        {
            foreach (var (sourceRow, sourceCol, id) in _sources)
            {
                var currentFrontier = new List<(int row, int col)> { (sourceRow, sourceCol) };
                var visited = new bool[_height, _width];
                visited[sourceRow, sourceCol] = true;
                int time = 0;

                while (currentFrontier.Count > 0)
                {
                    var nextFrontier = new List<(int row, int col)>();

                    foreach (var (row, col) in currentFrontier)
                    {
                        if (_distanceMap[row, col] == time)
                            _territoryMap[row, col] = '+';
                        else if (_distanceMap[row, col] > time)
                        {
                            _distanceMap[row, col] = time;
                            _territoryMap[row, col] = id;
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            int newRow = row + _directions[i, 0];
                            int newCol = col + _directions[i, 1];

                            if (newRow < 0 || newRow >= _height || newCol < 0 || newCol >= _width)
                                continue;

                            if (visited[newRow, newCol])
                                continue;

                            if (!_isPassable[newRow, newCol])
                                continue;

                            if (_distanceMap[newRow, newCol] < time + 1)
                                continue;

                            visited[newRow, newCol] = true;
                            nextFrontier.Add((newRow, newCol));
                        }
                    }

                    currentFrontier = nextFrontier;
                    time++;
                }
            }
        }
    }
}
