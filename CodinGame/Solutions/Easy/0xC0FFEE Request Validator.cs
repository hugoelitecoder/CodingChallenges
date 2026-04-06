using System;
using System.Diagnostics;
using System.Collections.Generic;

public class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line1 = Console.ReadLine();
        if (line1 == null) return;
        var frameLength = int.Parse(line1);
        var frame = Console.ReadLine();
        Console.Error.WriteLine($"[DEBUG] Frame Length: {frameLength}");
        Console.Error.WriteLine($"[DEBUG] Frame: {frame}");
        var validator = new CoffeeOrderValidator();
        var result = validator.ProcessFrame(frameLength, frame);
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Process Time: {sw.ElapsedMilliseconds}ms");
        if (result == null)
        {
            Console.WriteLine("403 Forbidden");
        }
        else
        {
            foreach (var line in result)
            {
                Console.WriteLine(line);
            }
        }
    }
}

public class CoffeeOrderValidator
{
    private const string HEADER = "DECAFBAD";
    public List<string> ProcessFrame(int length, string frame)
    {
        if (length < 12 || frame.Length != length) return null;
        if (!frame.StartsWith(HEADER)) return null;
        var sizeHex = frame.Substring(8, 3);
        var orderSize = Convert.ToInt32(sizeHex, 16);
        if (orderSize <= 0 || length != 8 + 3 + orderSize + 1) return null;
        var totalSum = 0;
        for (var i = 0; i < length; i++)
        {
            totalSum += GetHexValue(frame[i]);
        }
        if (totalSum % 16 != 0) return null;
        var order = frame.Substring(11, orderSize);
        var counts = new Dictionary<char, int>();
        var appearance = new List<char>();
        for (var i = 0; i < order.Length; i++)
        {
            var id = order[i];
            if (!counts.ContainsKey(id))
            {
                counts[id] = 0;
                appearance.Add(id);
            }
            counts[id]++;
        }
        var output = new List<string>();
        for (var i = 0; i < appearance.Count; i++)
        {
            var id = appearance[i];
            output.Add($"{counts[id]} {id}");
        }
        return output;
    }
    private int GetHexValue(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        return c - 'A' + 10;
    }
}