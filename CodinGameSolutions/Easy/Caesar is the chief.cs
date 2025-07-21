using System;
using System.Linq;
using System.Text;

class Solution
{
    static void Main()
    {
        string text = Console.ReadLine();

        for (int shift = 0; shift < 26; shift++)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char ch in text)
            {
                if (ch == ' ')
                {
                    sb.Append(' ');
                }
                else
                {
                    int x = ch - 'A';
                    int y = (x - shift + 26) % 26;
                    sb.Append((char)(y + 'A'));
                }
            }

            string decoded = sb.ToString();
            var words = decoded.Split(' ');
            if (words.Any(w => w == "CHIEF"))
            {
                Console.WriteLine(decoded);
                return;
            }
        }

        Console.WriteLine("WRONG MESSAGE");
    }
}
