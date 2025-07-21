using System;

class Solution
{
    static void Main()
    {
        string[] dims = Console.ReadLine().Split();
        int rows = int.Parse(dims[0]);
        int cols = int.Parse(dims[1]);

        Pixel[][] image = ReadImage(rows, cols);
        Pixel[][] blurred = BlurImage(image, rows, cols);
        PrintImage(blurred, rows, cols);
    }

    struct Pixel
    {
        public int R, G, B;
        public Pixel(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    static Pixel[][] ReadImage(int rows, int cols)
    {
        var result = new Pixel[rows][];
        for (int r = 0; r < rows; r++)
        {
            result[r] = new Pixel[cols];
            for (int c = 0; c < cols; c++)
            {
                string[] parts = Console.ReadLine().Split();
                int red   = int.Parse(parts[0]);
                int green = int.Parse(parts[1]);
                int blue  = int.Parse(parts[2]);
                result[r][c] = new Pixel(red, green, blue);
            }
        }
        return result;
    }

    static Pixel[][] BlurImage(Pixel[][] image, int rows, int cols)
    {
        var output = new Pixel[rows][];
        var offsets = new (int dr, int dc)[]
        {
            (0, 0), (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        for (int r = 0; r < rows; r++)
        {
            output[r] = new Pixel[cols];
            for (int c = 0; c < cols; c++)
            {
                int sumR = 0, sumG = 0, sumB = 0, count = 0;
                foreach (var (dr, dc) in offsets)
                {
                    int nr = r + dr, nc = c + dc;
                    if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                    {
                        var p = image[nr][nc];
                        sumR += p.R;
                        sumG += p.G;
                        sumB += p.B;
                        count++;
                    }
                }
                output[r][c] = new Pixel(sumR / count, sumG / count, sumB / count);
            }
        }
        return output;
    }

    static void PrintImage(Pixel[][] image, int rows, int cols)
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                Console.WriteLine($"{image[r][c].R} {image[r][c].G} {image[r][c].B}");
    }
}
