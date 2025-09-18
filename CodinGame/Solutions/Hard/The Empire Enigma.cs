using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var offset = long.Parse(Console.ReadLine());
        var length = int.Parse(Console.ReadLine());
        var encoded = new int[length];
        for (var i = 0; i < length; ++i)
            encoded[i] = int.Parse(Console.ReadLine());
        var messageCoder = new EmpireMessageCoder();
        var decoded = messageCoder.Decode(offset, length, encoded);
        Console.WriteLine(decoded);
    }
}

class EmpireMessageCoder
{
    const int Mod = 7140;
    const long Mul = 7562100L;
    const long Add = 907598307L;

    public string Decode(long offset, int length, int[] encoded)
    {
        var firstRand8 = encoded[0] ^ 64;
        var candidates = new List<int>();
        for (var state = 0; state < Mod; ++state)
            if ((state & 0xFF) == firstRand8)
                candidates.Add(state);
        foreach (var s in candidates)
        {
            var curr = s;
            var valid = true;
            var sb = new StringBuilder(length - 1);
            for (var i = 1; i < length; ++i)
            {
                curr = (int)((Mul * curr + Add) % Mod);
                var ch = encoded[i] ^ (curr & 0xFF);
                if (ch < 32 || ch > 126)
                {
                    valid = false;
                    break;
                }
                sb.Append((char)ch);
            }
            if (valid) return sb.ToString();
        }
        return string.Empty;
    }

    public int[] Encode(long seed, long offset, string message)
    {
        var full = "@" + message;
        var len = full.Length;
        var encoded = new int[len];
        var rng = GetRng(seed, offset, len);
        for (var i = 0; i < len; ++i)
            encoded[i] = full[i] ^ rng[i];
        return encoded;
    }

    int[] GetRng(long seed, long offset, int count)
    {
        var arr = new int[count];
        var state = seed;
        for (var i = 0L; i < offset; ++i)
            state = (Mul * state + Add) % Mod;
        for (var i = 0; i < count; ++i)
        {
            state = (Mul * state + Add) % Mod;
            arr[i] = (int)(state & 0xFF);
        }
        return arr;
    }
}
