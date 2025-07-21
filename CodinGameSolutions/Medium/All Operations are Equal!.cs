using System;
class Solution {
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0) {
            int X = int.Parse(Console.ReadLine());
            bool good = false;
            for (int e = 1; ; e++) {
                int s = (e + 1) * (e + 1);
                if (s * 2 > X) break;
                if (X % s == 0) { good = true; break; }
            }
            Console.WriteLine(good ? "YES" : "NO");
        }
    }
}
