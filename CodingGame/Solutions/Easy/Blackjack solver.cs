using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    // Results
    private const string PlayerWin       = "Player";
    private const string BankWin         = "Bank";
    private const string Draw            = "Draw";
    private const string BlackjackExcl   = "Blackjack!";

    // Ten‑value cards
    private static readonly HashSet<string> TenCards = new HashSet<string> { "10", "J", "Q", "K" };

    static void Main()
    {
        var bank   = Console.ReadLine()!.Split();
        var player = Console.ReadLine()!.Split();

        bool bankBJ   = IsBlackjack(bank);
        bool playerBJ = IsBlackjack(player);

        // Blackjack overrides
        if (playerBJ ^ bankBJ)
        {
            Console.WriteLine(playerBJ ? BlackjackExcl : BankWin);
            return;
        }
        if (playerBJ && bankBJ)
        {
            Console.WriteLine(Draw);
            return;
        }

        int bScore = Score(bank);
        int pScore = Score(player);
        bool bBust = bScore > 21;
        bool pBust = pScore > 21;

        // Determine winner
        Console.WriteLine(
            pBust            ? BankWin       :
            bBust            ? PlayerWin     :
            pScore > bScore  ? PlayerWin     :
            pScore < bScore  ? BankWin       :
            Draw
        );
    }

    // True if exactly two cards: one Ace and one ten‑value
    static bool IsBlackjack(string[] cards) =>
        cards.Length == 2
        && cards.Contains("A")
        && cards.Any(c => TenCards.Contains(c));

    // Compute best score with Aces as 11 or 1
    static int Score(string[] cards)
    {
        int aces = cards.Count(c => c == "A");
        int sum  = cards
            .Where(c => c != "A")
            .Sum(c => TenCards.Contains(c) ? 10 : int.Parse(c));

        sum += aces * 11;
        while (sum > 21 && aces-- > 0)
            sum -= 10;
        return sum;
    }
}
