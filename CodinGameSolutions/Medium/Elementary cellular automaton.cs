using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var rule  = int.Parse(Console.ReadLine().Trim());
        var N     = int.Parse(Console.ReadLine().Trim());
        var state = Console.ReadLine().Trim();
        var L     = state.Length;
        var map   = Convert.ToString(rule, 2).PadLeft(8, '0');

        Enumerable.Range(0, N)
                  .ToList()
                  .ForEach(_ =>
                  {
                      Console.WriteLine(state);
                      state = string.Concat(
                          Enumerable.Range(0, L)
                                    .Select(i =>
                                    {
                                        var l   = state[(i - 1 + L) % L] == '@' ? 1 : 0;
                                        var c   = state[i] == '@' ? 1 : 0;
                                        var r   = state[(i + 1) % L] == '@' ? 1 : 0;
                                        var idx = (l << 2) | (c << 1) | r;
                                        return map[7 - idx] == '1' ? '@' : '.';
                                    })
                      );
                  });
    }
}
