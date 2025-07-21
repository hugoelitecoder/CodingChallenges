using System;

class Solution
{
    static void Main()
    {
        int ROUNDS = int.Parse(Console.ReadLine() ?? "0");
        int cash = int.Parse(Console.ReadLine() ?? "0");

        for (int i = 0; i < ROUNDS; i++)
        {
            var parts = (Console.ReadLine() ?? string.Empty).Split(' ');
            if (parts.Length < 2) continue;

            int ball = int.Parse(parts[0]);
            string call = parts[1];
            int number = parts.Length == 3 ? int.Parse(parts[2]) : -1;
            int bet = (cash + 3) / 4;
            switch (call)
            {
                case "EVEN":
                    if (ball != 0 && ball % 2 == 0)
                        cash += bet;
                    else
                        cash -= bet;
                    break;

                case "ODD":
                    if (ball % 2 == 1)
                        cash += bet;
                    else
                        cash -= bet;
                    break;

                case "PLAIN":
                    if (ball == number)
                        cash += bet * 35;
                    else
                        cash -= bet;
                    break;

                default:
                    cash -= bet;
                    break;
            }
        }

        Console.WriteLine(cash);
    }
}
