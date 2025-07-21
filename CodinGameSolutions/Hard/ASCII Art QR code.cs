using System;
using System.Collections.Generic;

public class Solution {
    
    public static void Main()
    {
        int w = int.Parse(Console.ReadLine());
        int h = int.Parse(Console.ReadLine());
        var txt = new string[h];
        for (int i = 0; i < h; i++)
            txt[i] = Console.ReadLine();
        string input = string.Join("\n", txt);
        var s = new ASCII_QRcode();
        Console.WriteLine(s.Run(input, w, h));
    }
}

public class ASCII_QRcode
{
    public string Run(string qr, int w, int h)
    {
        return ReadQR(qr, w, h);
    }

    private string ReadQR(string asciiqr, int w, int h)
    {
        var lines = asciiqr.Split('\n');
        var binMsg = new List<string>();
        bool up = true;

        for (int x = w - 1; x >= 0; x--)
        {
            if (up)
            {
                for (int y = h - 1; y >= 0; y--)
                {
                    if (DataArea(x, y, w, h))
                    {
                        char q = lines[y][x];
                        char pb = PatternBit(x, y, w, h);
                        string bit;
                        if (pb == '0')
                            bit = (q == ' ') ? "0" : "1";
                        else
                            bit = (q == ' ') ? "1" : "0";
                        binMsg.Add(bit);
                    }
                }
            }
            else
            {
                for (int y = 0; y < h; y++)
                {
                    if (DataArea(x, y, w, h))
                    {
                        char q = lines[y][x];
                        char pb = PatternBit(x, y, w, h);
                        string bit;
                        if (pb == '0')
                            bit = (q == ' ') ? "0" : "1";
                        else
                            bit = (q == ' ') ? "1" : "0";
                        binMsg.Add(bit);
                    }
                }
            }
            up = !up;
        }
        return Decode(binMsg);
    }

    private bool DataArea(int x, int y, int w, int h)
    {
        if (x <= 5 && y <= 3) return false;
        if (x >= w - 6 && y <= 3) return false;
        if (x <= 5 && y >= h - 4) return false;
        if (x >= w - 6 && x < w - 3 && y >= h - 4 && y < h - 1) return false;
        return true;
    }

    private char PatternBit(int x, int y, int w, int h)
    {
        bool bitRow = true;
        for (int yi = h - 1; yi >= 0; yi--)
        {
            bool bit = bitRow;
            for (int xi = w - 1; xi >= 0; xi--)
            {
                if (yi == y && xi == x)
                    return bit ? '1' : '0';
                bit = !bit;
            }
            bitRow = !bitRow;
        }
        return '0';
    }

    private string Crypt(List<string> msg, string key)
    {
        if (key[0] == '1')
            return string.Join("", msg);

        key = key.Substring(1);
        string res = "";
        int keylen = key.Length;
        for (int i = 0; i < msg.Count; i++)
        {
            res += (msg[i][0] != key[i % keylen] ? '1' : '0');
        }
        return res;
    }

    private string Decode(List<string> binMsg)
    {
        string BOM = string.Join("", binMsg.GetRange(0, 8));
        var msg = binMsg.GetRange(8, binMsg.Count - 8);
        string decodedBits = Crypt(msg, BOM);
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i + 7 <= decodedBits.Length; i += 7)
        {
            var bits = decodedBits.Substring(i, 7);
            int val = Convert.ToInt32(bits, 2);
            if (val == 0) break;
            sb.Append((char)val);
        }
        return sb.ToString();
    }
   
}
