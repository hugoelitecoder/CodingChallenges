using System;
using System.Collections.Generic;

class Solution {
    static void Main() {
        var parts = Console.ReadLine().Split();
        int L = int.Parse(parts[0]), C = int.Parse(parts[1]), N = int.Parse(parts[2]);
        var P = new int[N];
        for (int i = 0; i < N; i++) P[i] = int.Parse(Console.ReadLine());
        var earn = new long[N];
        var next = new int[N];
        for (int i = 0; i < N; i++) {
            long sum = 0;
            int cnt = 0, j = i;
            while (cnt < N && sum + P[j] <= L) {
                sum += P[j];
                j = (j + 1) % N;
                cnt++;
            }
            earn[i] = sum;
            next[i] = j;
        }
        long total = 0;
        int pos = 0, ride = 0;
        var seen = new Dictionary<int, (int ride, long total)>();
        while (ride < C) {
            if (seen.TryGetValue(pos, out var info)) {
                int cycleLen = ride - info.ride;
                long cycleSum = total - info.total;
                int reps = (C - ride) / cycleLen;
                total += cycleSum * reps;
                ride += cycleLen * reps;
                seen.Clear();
            } else {
                seen[pos] = (ride, total);
            }
            if (ride < C) {
                total += earn[pos];
                pos = next[pos];
                ride++;
            }
        }
        Console.WriteLine(total);
    }
}
