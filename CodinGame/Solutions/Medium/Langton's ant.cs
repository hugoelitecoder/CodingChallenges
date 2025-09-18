using System;

class Solution
{
    static void Main()
    {
        var dims = Console.ReadLine().Split();
        int width = int.Parse(dims[0]);
        int height = int.Parse(dims[1]);

        var pos = Console.ReadLine().Split();
        int antX = int.Parse(pos[0]);
        int antY = int.Parse(pos[1]);

        char dirChar = Console.ReadLine()[0];
        int turns = int.Parse(Console.ReadLine());

        var grid = new string[height];
        for (int row = 0; row < height; row++)
            grid[row] = Console.ReadLine();

        var board = new LangtonAntBoard(width, height, antX, antY, dirChar, grid);
        board.Simulate(turns);

        foreach (var line in board.GetGrid())
            Console.WriteLine(line);
    }
}

class LangtonAntBoard
{
    private readonly int width, height;
    private readonly int[] rowBits;
    private int antX, antY, direction;

    private static readonly (int dx, int dy)[] Directions =
    {
        (0, -1),
        (1, 0),
        (0, 1),
        (-1, 0)
    };

    public LangtonAntBoard(int width, int height, int startX, int startY, char dirChar, string[] grid)
    {
        this.width = width;
        this.height = height;
        this.antX = startX;
        this.antY = startY;
        this.rowBits = new int[height];

        for (int r = 0; r < height; r++)
            for (int c = 0; c < width; c++)
                if (grid[r][c] == '#')
                    rowBits[r] |= 1 << c;

        direction = dirChar switch
        {
            'N' => 0,
            'E' => 1,
            'S' => 2,
            'W' => 3,
            _   => 0
        };
    }

    public void Simulate(int turns)
    {
        for (int i = 0; i < turns; i++)
        {
            int currentColor = (rowBits[antY] >> antX) & 1;
            direction = (direction + (currentColor == 0 ? 3 : 1)) & 3;
            rowBits[antY] ^= 1 << antX;

            antX += Directions[direction].dx;
            antY += Directions[direction].dy;
        }
    }

    public string[] GetGrid()
    {
        var result = new string[height];

        for (int r = 0; r < height; r++)
        {
            var chars = new char[width];
            for (int c = 0; c < width; c++)
                chars[c] = ((rowBits[r] >> c) & 1) == 1 ? '#' : '.';
            result[r] = new string(chars);
        }

        return result;
    }
}