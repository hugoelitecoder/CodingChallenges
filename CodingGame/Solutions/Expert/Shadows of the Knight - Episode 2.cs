using System;

class Player {
    static void Main() {
        var parts = Console.ReadLine().Split();
        int width  = int.Parse(parts[0]),
            height = int.Parse(parts[1]);
        Console.ReadLine();

        parts = Console.ReadLine().Split();
        int prevX = int.Parse(parts[0]),
            prevY = int.Parse(parts[1]);

        int currX = prevX,
            currY = prevY;
        var excluded = new bool[width, height];

        int sliceH   = Math.Min(height, 1_000_000 / Math.Max(width, 1));
        int sliceY0  = 0,
            sliceY1  = sliceH;

        while (true) {
            char feedback = Console.ReadLine()[0];
            if (currX >= 0 && currX < width && currY >= 0 && currY < height)
                excluded[currX, currY] = true;

            if (feedback != 'U') {
                for (int x = 0; x < width; x++)
                    for (int y = sliceY0; y < sliceY1; y++) {
                        if (excluded[x, y]) continue;
                        int dPrev = (prevX - x)*(prevX - x) + (prevY - y)*(prevY - y);
                        int dCurr = (currX - x)*(currX - x) + (currY - y)*(currY - y);
                        if ((feedback == 'C' && dCurr <= dPrev) ||
                            (feedback == 'W' && dPrev <= dCurr) ||
                            (feedback == 'S' && dCurr != dPrev))
                            excluded[x, y] = true;
                    }
            }

            prevX = currX;  prevY = currY;
            int totalX = 0, totalY = 0, count = 0;
            int firstX = -1, firstY = -1;

            for (int x = 0; x < width; x++)
                for (int y = sliceY0; y < sliceY1; y++) {
                    if (!excluded[x, y]) {
                        totalX += x;
                        totalY += y;
                        count++;
                        if (firstX < 0) { firstX = x; firstY = y; }
                    }
                }

            if (count == 0) {
                sliceY0 += sliceH;
                sliceY1 = Math.Min(height, sliceY0 + sliceH);
                continue;
            }

            currX = (int)Math.Round((double)totalX / count);
            currY = (int)Math.Round((double)totalY / count);
            currX = Math.Clamp(currX, 0, width - 1);
            currY = Math.Clamp(currY, 0, height - 1);

            if (excluded[currX, currY]) {
                currX = firstX;
                currY = firstY;
            }

            Console.WriteLine($"{currX} {currY}");
        }
    }
}
