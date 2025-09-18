using System;

class Program
{
    enum Dir { Up = 1, Right, Down, Left }
    const char BorderChar = '#';
    const int Pad = 1;
    static readonly (int dr, int dc)[] Offsets = { (-1, 0), (0, 1), (1, 0), (0, -1) };
    const string Markers = "^>v<";

    static int FlipColumn(int x, int width) => x <= width / 2 ? x + width / 2 : x - width / 2;

    static void Main()
    {
        var dims = Console.ReadLine().Split();
        int width = int.Parse(dims[0]), height = int.Parse(dims[1]);
        int totalRows = height + 2 * Pad, totalCols = width + 2 * Pad;
        var grid = new char[totalRows, totalCols];

        for (int i = 0; i < totalRows; i++)
            for (int j = 0; j < totalCols; j++)
                grid[i, j] = BorderChar;

        for (int i = Pad; i < height + Pad; i++)
        {
            var line = Console.ReadLine();
            for (int j = Pad; j < width + Pad; j++)
                grid[i, j] = line[j - Pad];
        }

        char turn = Console.ReadLine()[0];
        int currRow = 0, currCol = 0, currDir = 0;
        for (int i = 0; i < totalRows; i++)
            for (int j = 0; j < totalCols; j++)
                if (Markers.IndexOf(grid[i, j]) >= 0)
                {
                    currDir = Markers.IndexOf(grid[i, j]) + 1;
                    currRow = i; currCol = j;
                    grid[i, j] = '0';
                }

        int startRow = currRow, startCol = currCol;

        for (int i = Pad; i < height + Pad; i++)
        {
            grid[i, 0] = grid[i, width];
            grid[i, width + Pad] = grid[i, 1];
        }

        for (int j = Pad; j < width + Pad; j++)
        {
            int flippedCol = FlipColumn(j - Pad, width) + Pad;
            grid[0, flippedCol] = grid[height, j];
            grid[height + Pad, flippedCol] = grid[1, j];
        }

        do
        {
            int dirIndex = currDir - 1;
            int[] moveOrder = turn == 'R'
                ? new[] { (dirIndex + 1) % 4, dirIndex, (dirIndex + 3) % 4, (dirIndex + 2) % 4 }
                : new[] { (dirIndex + 3) % 4, dirIndex, (dirIndex + 1) % 4, (dirIndex + 2) % 4 };

            foreach (int d in moveOrder)
            {
                var (dr, dc) = Offsets[d];
                int newRow = currRow + dr, newCol = currCol + dc;
                if (grid[newRow, newCol] == BorderChar) continue;

                currRow = newRow; currCol = newCol;
                if (currRow == 0)
                {
                    currRow = height;
                    currCol = FlipColumn(currCol - Pad, width) + Pad;
                }
                else if (currRow == height + Pad)
                {
                    currRow = Pad;
                    currCol = FlipColumn(currCol - Pad, width) + Pad;
                }
                else if (currCol == 0)
                    currCol = width;
                else if (currCol == width + Pad)
                    currCol = Pad;

                grid[currRow, currCol]++;
                currDir = d + 1;
                break;
            }
        }
        while (currRow != startRow || currCol != startCol);

        for (int i = Pad; i < height + Pad; i++)
        {
            for (int j = Pad; j < width + Pad; j++)
                Console.Write(grid[i, j]);
            Console.WriteLine();
        }
    }
}
