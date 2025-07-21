using System;

class Solution
{
    static void Main()
    {
        var inputs = Console.ReadLine().Split();
        int wallyWidth = int.Parse(inputs[0]);
        int wallyHeight = int.Parse(inputs[1]);

        var wally = new string[wallyHeight];
        for (int i = 0; i < wallyHeight; i++)
            wally[i] = Console.ReadLine();

        inputs = Console.ReadLine().Split();
        int pictureWidth = int.Parse(inputs[0]);
        int pictureHeight = int.Parse(inputs[1]);

        var picture = new string[pictureHeight];
        for (int i = 0; i < pictureHeight; i++)
            picture[i] = Console.ReadLine();

        for (int y = 0; y <= pictureHeight - wallyHeight; y++)
        {
            for (int x = 0; x <= pictureWidth - wallyWidth; x++)
            {
                bool match = true;
                for (int wy = 0; wy < wallyHeight && match; wy++)
                {
                    for (int wx = 0; wx < wallyWidth && match; wx++)
                    {
                        char wc = wally[wy][wx];
                        if (wc != ' ' && picture[y + wy][x + wx] != wc)
                            match = false;
                    }
                }

                if (match)
                {
                    Console.WriteLine($"{x} {y}");
                    return;
                }
            }
        }
    }
}
