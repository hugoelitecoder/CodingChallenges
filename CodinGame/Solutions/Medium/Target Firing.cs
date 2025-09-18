using System;
using System.Linq;
using System.Collections.Generic;

class Program
{
    enum ShipClass { Fighter, Cruiser }
    class AlienShip
    {
        public ShipClass Class;
        public long HP, Armor, Damage;
        public long PerShot => Math.Max(1, (Class == ShipClass.Fighter ? 20 : 10) - Armor);
        public long Turns  => (HP + PerShot - 1) / PerShot;
    }
    class Encounter
    {
        const long Shields = 5000;
        readonly List<AlienShip> enemies;
        public Encounter(IEnumerable<AlienShip> e) => enemies = e.ToList();
        public long Remaining() =>
            Shields - enemies
                .OrderByDescending(s => (double)s.Damage / s.Turns)
                .Aggregate((t: 0L, d: 0L), (acc, s) => (t: acc.t + s.Turns, d: acc.d + s.Damage * (acc.t + s.Turns))).d;
    }

    static void Main()
    {
        var ships = Enumerable.Range(0, int.Parse(Console.ReadLine()))
            .Select(_ => {
                var p = Console.ReadLine().Split();
                return new AlienShip {
                    Class  = p[0] == "FIGHTER" ? ShipClass.Fighter : ShipClass.Cruiser,
                    HP     = long.Parse(p[1]),
                    Armor  = long.Parse(p[2]),
                    Damage = long.Parse(p[3])
                };
            });

        long rem = new Encounter(ships).Remaining();
        Console.WriteLine(rem >= 0 ? rem.ToString() : "FLEE");
    }
}
