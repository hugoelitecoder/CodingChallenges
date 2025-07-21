using System;
using System.Text;

class Solution {
    static void Main() {
        int x = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());
        Console.WriteLine(DeBruijn(x, n));
    }

    static string DeBruijn(int k, int n) {
        var a = new int[k * n];
        var sb = new StringBuilder();
        void db(int t, int p) {
            if (t > n) {
                if (n % p == 0) {
                    for (int i = 1; i <= p; i++)
                        sb.Append((char)('0' + a[i]));
                }
            } else {
                a[t] = a[t - p];
                db(t + 1, p);
                for (int j = a[t - p] + 1; j < k; j++) {
                    a[t] = j;
                    db(t + 1, t);
                }
            }
        }
        db(1, 1);
        var seq = sb.ToString();
        return seq + seq.Substring(0, n - 1);
    }
}
