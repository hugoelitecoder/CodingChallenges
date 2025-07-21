using System;

class Player
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);

        var grid = new char[height, width];

        for (int i = 0; i < height; i++)
        {
            var line = Console.ReadLine();
            for (int j = 0; j < width; j++)
                grid[i, j] = line[j];
        }

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        for (int y = 0; y < height; y++)
        {
            string output = "";
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] == '#')
                {
                    output += '#';
                }
                else
                {
                    int count = 0;
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d], ny = y + dy[d];
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height && grid[ny, nx] == '0')
                            count++;
                    }
                    output += count.ToString();
                }
            }
            Console.WriteLine(output);
        }
    }
}
