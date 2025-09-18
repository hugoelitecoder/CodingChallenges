using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int size = int.Parse(Console.ReadLine());
        var img = new char[size, size];

        for (int i = 0; i < size; i++)
        {
            var tokens = Console.ReadLine().Split();
            for (int j = 0; j < tokens.Length; j++)
            {
                img[i, j] = tokens[j][0];
            }
        }

        var processor = new FractalProcessor();
        processor.PlaceFractals(img);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Console.Write(img[i, j]);
                if (j < size - 1) Console.Write(' ');
            }
            Console.WriteLine();
        }
    }
}

public class FractalProcessor
{
    struct Rule
    {
        public int Dx, Dy;
        public char Value;
        public (int X, int Y)[] Checks;

        public Rule(int dx, int dy, char value, (int, int)[] checks)
        {
            Dx = dx;
            Dy = dy;
            Value = value;
            Checks = checks;
        }
    }

    private readonly Rule[] _rules;

    public FractalProcessor()
    {
        _rules = new[]
        {
            new Rule( 0,-1,'3', new[] {(0,-1),(-1,-1),(-1,0),(1,-1),(1,0)}), // N
            new Rule( 1, 0,'2', new[] {(0,-1),(1,-1),(1,0),(0,1),(1,1)}),  // E
            new Rule( 0, 1,'6', new[] {(0,1),(1,1),(1,0),(-1,0),(-1,1)}),  // S
            new Rule(-1, 0,'9', new[] {(0,-1),(-1,-1),(-1,0),(-1,1),(0,1)}),// W
            new Rule( 1,-1,'4', new[] {(0,-1),(-1,-1),(1,-1),(1,1),(1,0)}), // NE
            new Rule( 1, 1,'7', new[] {(0,1),(-1,1),(1,0),(1,-1),(1,1)}),  // SE
            new Rule(-1, 1,'8', new[] {(0,1),(-1,1),(-1,0),(-1,-1),(1,1)}), // SW
            new Rule(-1,-1,'5', new[] {(0,-1),(1,-1),(-1,-1),(-1,1),(-1,0)}) // NW
        };
    }

    public void PlaceFractals(char[,] img)
    {
        int rows = img.GetLength(0);
        int cols = img.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (img[y, x] != '1') continue;

                foreach (var rule in _rules)
                {
                    int tx = x + rule.Dx;
                    int ty = y + rule.Dy;
                    if (tx < 0 || tx >= cols || ty < 0 || ty >= rows)
                        continue;

                    bool canPlace = true;
                    foreach (var check in rule.Checks)
                    {
                        int cx = x + check.X;
                        int cy = y + check.Y;
                        if (cx < 0 || cx >= cols || cy < 0 || cy >= rows || img[cy, cx] == '1')
                        {
                            canPlace = false;
                            break;
                        }
                    }

                    if (canPlace)
                        img[ty, tx] = rule.Value;
                }
            }
        }
    }
}