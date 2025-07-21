using System;
using System.Collections.Generic;

class Champion
{
    public string Name;
    public int Life;
    public int Rage = 0;
    public int Hits = 0;
    public int DamageReceived = 0;
    public Champion Opponent;

    public Champion(string name)
    {
        Name = name;
        Life = name switch
        {
            "KEN" => 25,
            "RYU" => 25,
            "TANK" => 50,
            "VLAD" => 30,
            "JADE" => 20,
            "ANNA" => 18,
            "JUN" => 60,
            _ => 0
        };
    }

    public void SetOpponent(Champion opp) => Opponent = opp;

    public void ApplyHit(string attack)
    {
        Hits++;
        int damage = 0;

        if (attack == "PUNCH")
        {
            damage = Name switch
            {
                "KEN" => 6, "RYU" => 4, "TANK" => 2,
                "VLAD" => 3, "JADE" => 2, "ANNA" => 9, "JUN" => 2
            };
        }
        else if (attack == "KICK")
        {
            damage = Name switch
            {
                "KEN" => 5, "RYU" => 5, "TANK" => 2,
                "VLAD" => 3, "JADE" => 7, "ANNA" => 1, "JUN" => 1
            };
        }
        else if (attack == "SPECIAL")
        {
            damage = Name switch
            {
                "KEN" => 3 * Rage,
                "RYU" => 4 * Rage,
                "TANK" => 2 * Rage,
                "VLAD" => 2 * (Rage + Opponent.Rage),
                "JADE" => Hits * Rage,
                "ANNA" => Rage * DamageReceived,
                "JUN" => Rage
            };
            if (Name == "VLAD") Opponent.Rage = 0;
            if (Name == "JUN") Life += Rage;
            Rage = 0;
        }

        Opponent.Life -= damage;
        Opponent.Rage += 1;
        Opponent.DamageReceived += damage;
    }
}

class Solution
{
    static void Main()
    {
        var names = Console.ReadLine().Split();
        string champ1Name = names[0], champ2Name = names[1];
        int N = int.Parse(Console.ReadLine());

        var c1 = new Champion(champ1Name);
        var c2 = new Champion(champ2Name);
        c1.SetOpponent(c2);
        c2.SetOpponent(c1);

        int c1Hits = 0, c2Hits = 0;

        for (int i = 0; i < N; i++)
        {
            var input = Console.ReadLine().Split();
            string dir = input[0], attack = input[1];

            if (c1.Life <= 0 || c2.Life <= 0) break;

            if (dir == ">")
            {
                c1.ApplyHit(attack);
                c1Hits++;
            }
            else
            {
                c2.ApplyHit(attack);
                c2Hits++;
            }
        }

        if (c1.Life > c2.Life)
            Console.WriteLine($"{champ1Name} beats {champ2Name} in {c1Hits} hits");
        else
            Console.WriteLine($"{champ2Name} beats {champ1Name} in {c2Hits} hits");
    }
}
