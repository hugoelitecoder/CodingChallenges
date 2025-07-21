using System;

class Solution
{
    static void Main()
    {
        string color = Console.ReadLine();
        var board = new char[8, 8];
        int qx = -1, qy = -1;

        for (int i = 0; i < 8; i++)
        {
            string line = Console.ReadLine();
            for (int j = 0; j < 8; j++)
            {
                board[i, j] = line[j];
                if (line[j] == 'Q')
                {
                    qx = i;
                    qy = j;
                }
            }
        }

        char own = color == "white" ? 'w' : 'b';
        char enemy = color == "white" ? 'b' : 'w';

        var directions = new (int dx, int dy)[]
        {
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        int controlled = 0;

        foreach (var (dx, dy) in directions)
        {
            int x = qx + dx;
            int y = qy + dy;

            while (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                char cell = board[x, y];
                if (cell == '.')
                    controlled++;
                else if (cell == enemy)
                {
                    controlled++;
                    break;
                }
                else break;

                x += dx;
                y += dy;
            }
        }

        Console.WriteLine(controlled);
    }
}
