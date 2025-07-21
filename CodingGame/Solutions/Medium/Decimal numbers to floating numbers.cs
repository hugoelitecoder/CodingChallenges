using System;
using System.Globalization;

class P
{
    static void Main()
    {
        var s = Console.ReadLine();
        if (!float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v))
        {
            Console.WriteLine("[0][11111111][11111111111111111111111]");
            Console.WriteLine("NaN");
            return;
        }

        uint bits = v == 0f ? 0u : BitConverter.SingleToUInt32Bits(v);
        int sign = (int)((bits >> 31) & 1);
        int exp  = (int)((bits >> 23) & 0xFF);
        int man  = (int)(bits & 0x7FFFFF);

        Console.WriteLine($"[{sign}][{Convert.ToString(exp, 2).PadLeft(8, '0')}][{Convert.ToString(man, 2).PadLeft(23, '0')}]");
        Console.WriteLine($"0x{bits:X8}");
    }
}
