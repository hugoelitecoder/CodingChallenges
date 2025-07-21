using System;

class Solution {
    static int _n, _mask, _count;
    static void Main() {
        _n = int.Parse(Console.ReadLine());
        _mask = (1 << _n) - 1;
        _count = 0;
        Place(0, 0, 0, 0);
        Console.WriteLine(_count);
    }

    private static void Place(int row, int cols, int diagsL, int diagsR) {
        if (row == _n) {
            _count++;
            return;
        }
        int available = _mask & ~(cols | diagsL | diagsR);
        while (available != 0) {
            int bit = available & -available;
            available -= bit;
            Place(row + 1,
                  cols | bit,
                  (diagsL | bit) << 1,
                  (diagsR | bit) >> 1);
        }
    }
}
