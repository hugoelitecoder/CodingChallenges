using System;

class Solution
{
    static void Main(string[] args)
    {
        string x = Console.ReadLine();
        int n = int.Parse(Console.ReadLine());

        // Split the input license plate into its components
        string[] parts = x.Split('-');
        string ab = parts[0];
        int cde = int.Parse(parts[1]);
        string fg = parts[2];

        // Increment the license plate
        for (int i = 0; i < n; i++)
        {
            cde++;
            if (cde > 999)
            {
                cde = 1;
                fg = IncrementAlpha(fg);
                if (fg == "AA")
                {
                    ab = IncrementAlpha(ab);
                }
            }
        }

        // Format the result
        string result = $"{ab}-{cde:D3}-{fg}";
        Console.WriteLine(result);
    }

    static string IncrementAlpha(string alpha)
    {
        char[] chars = alpha.ToCharArray();
        for (int i = chars.Length - 1; i >= 0; i--)
        {
            if (chars[i] == 'Z')
            {
                chars[i] = 'A';
            }
            else
            {
                chars[i]++;
                break;
            }
        }
        return new string(chars);
    }
}
