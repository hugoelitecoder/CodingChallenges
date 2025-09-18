using System;
using System.Linq;

class Solution {
    public static void Main() {
        var n = int.Parse(Console.ReadLine());
        var t = n == 0 ? Array.Empty<int>() : Console.ReadLine().Split().Select(int.Parse);
        var res = 0;
        var min = int.MaxValue;
        foreach (var x in t) {
            var d = Math.Abs(x);
            if (d < min || (d == min && x > res)) {
                min = d;
                res = x;
            }
        }
        Console.WriteLine(res);
    }
}
