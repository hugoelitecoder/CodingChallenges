using System;
class Solution
{
    public static void Main(string[] args)
    {
        var line0 = Console.ReadLine();
        var line1 = Console.ReadLine();
        var line2 = Console.ReadLine();
        var front = line0[1] - '0';
        var left  = line1[0] - '0';
        var down  = line1[1] - '0';
        var right = line1[2] - '0';
        var up    = line1[3] - '0';
        var back  = line2[1] - '0';
        var commands = Console.ReadLine();
        foreach (var cmd in commands)
        {
            var oldFront = front; var oldBack = back; var oldLeft = left;
            var oldRight = right; var oldUp = up; var oldDown = down;
            if (cmd == 'U')
            {
                down  = oldFront; up    = oldBack;  front = oldUp;
                back  = oldDown;  left  = oldLeft; right = oldRight;
            }
            else if (cmd == 'D')
            {
                down  = oldBack;  up    = oldFront; front = oldUp;
                back  = oldDown;  left  = oldRight;right = oldLeft;
            }
            else if (cmd == 'L')
            {
                down  = oldLeft;  up    = oldRight;front = oldUp;
                back  = oldDown;  left  = oldBack; right = oldFront;
            }
            else
            {
                down  = oldRight; up    = oldLeft; front = oldUp;
                back  = oldDown;  left  = oldFront;right = oldBack;
            }
        }
        Console.WriteLine(down);
    }
}
