using System;

class Solution
{
    public static void Main(string[] args)
    {
        var stack1 = ParseStack(Console.ReadLine());
        var stack2 = ParseStack(Console.ReadLine());

        var round = 1;
        while (stack1.Amount > 0 && stack2.Amount > 0)
        {
            Console.WriteLine($"Round {round}");
            var oldDefAmt = stack2.Amount;
            var dmg1 = stack1.Amount * stack1.Damage;
            var cas1 = ApplyDamage(stack2, dmg1);
            Console.WriteLine($"{stack1.Amount} {stack1.Name}(s) attack(s) {oldDefAmt} {stack2.Name}(s) dealing {dmg1} damage");
            Console.WriteLine($"{cas1} unit(s) perish");
            Console.WriteLine("----------");
            if (stack2.Amount == 0)
            {
                Console.WriteLine($"{stack1.Name} won! {stack1.Amount} unit(s) left");
                break;
            }
            oldDefAmt = stack1.Amount;
            var dmg2 = stack2.Amount * stack2.Damage;
            var cas2 = ApplyDamage(stack1, dmg2);
            Console.WriteLine($"{stack2.Amount} {stack2.Name}(s) attack(s) {oldDefAmt} {stack1.Name}(s) dealing {dmg2} damage");
            Console.WriteLine($"{cas2} unit(s) perish");
            Console.WriteLine("##########");
            if (stack1.Amount == 0)
            {
                Console.WriteLine($"{stack2.Name} won! {stack2.Amount} unit(s) left");
                break;
            }
            round++;
        }
    }

    private static int ApplyDamage(StackInfo defender, int damage)
    {
        defender.TotalHealth -= damage;
        if (defender.TotalHealth < 0)
            defender.TotalHealth = 0;
        var newAmt = (int)Math.Ceiling(defender.TotalHealth / (double)defender.Health);
        var casualties = defender.Amount - newAmt;
        defender.Amount = newAmt;
        return casualties;
    }

    private class StackInfo
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }
        public int TotalHealth { get; set; }
    }

    private static StackInfo ParseStack(string line)
    {
        var parts = line.Split(';');
        var amt = int.Parse(parts[1]);
        var hp = int.Parse(parts[2]);
        return new StackInfo
        {
            Name = parts[0],
            Amount = amt,
            Health = hp,
            Damage = int.Parse(parts[3]),
            TotalHealth = amt * hp
        };
    }
}