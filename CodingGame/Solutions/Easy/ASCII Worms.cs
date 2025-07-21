using System;
using System.Text;

class Solution
{
    static void Main()
    {
        int thickness = int.Parse(Console.ReadLine());
        int length    = int.Parse(Console.ReadLine());
        int turns     = int.Parse(Console.ReadLine());
        
        if (length < 2) return;
        
        int half  = turns / 2;
        int width = 2 * thickness + 1;
        var sb    = new StringBuilder();
        sb.Append(' ').Append('_', thickness).Append(' ');
        for (int i = 0; i < half; i++)
            sb.Append('_', width).Append(' ');
        if (turns % 2 == 1)
            sb.Append('_', thickness);
        Console.WriteLine(sb.ToString().TrimEnd());
        
        sb.Clear();
        sb.Append('|').Append(' ', thickness);
        for (int i = 0; i < half; i++)
            sb.Append('|').Append(' ', width);
        sb.Append('|');
        if (turns % 2 == 1)
            sb.Append(' ', thickness).Append('|');
        Console.WriteLine(sb);
        
        sb.Clear();
        for (int i = 0; i < turns + 1; i++)
            sb.Append('|').Append(' ', thickness);
        sb.Append('|');
        var mid = sb.ToString();
        for (int row = 0; row < length - 2; row++)
            Console.WriteLine(mid);
        
        sb.Clear();
        if (turns % 2 == 0)
        {
            for (int i = 0; i < half; i++)
                sb.Append('|').Append('_', width);
            sb.Append('|').Append('_', thickness).Append('|');
        }
        else
        {
            for (int i = 0; i < half + 1; i++)
                sb.Append('|').Append('_', width);
            sb.Append('|');
        }
        Console.WriteLine(sb);
    }
}
