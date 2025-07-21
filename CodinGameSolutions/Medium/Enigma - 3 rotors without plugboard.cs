using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var r1 = new List<string>(Console.ReadLine().Split());
        char t1 = Console.ReadLine()[0];
        var r2 = new List<string>(Console.ReadLine().Split());
        char t2 = Console.ReadLine()[0];
        var r3 = new List<string>(Console.ReadLine().Split());
        char t3 = Console.ReadLine()[0];
        var refl = new List<string>(Console.ReadLine().Split());
        var sp = Console.ReadLine().Split();
        var msg = Console.ReadLine();
        var machine = new EnigmaMachine(r1, t1, sp[0][0],
                                        r2, t2, sp[1][0],
                                        r3, t3, sp[2][0],
                                        refl);
        Console.WriteLine(machine.Encode(msg));
    }
}

class EnigmaMachine
{
    private Rotor[] rotors;
    private Dictionary<char, char> reflector;

    public EnigmaMachine(List<string> wiring1, char trigger1, char start1,
                         List<string> wiring2, char trigger2, char start2,
                         List<string> wiring3, char trigger3, char start3,
                         List<string> reflectorWiring)
    {
        rotors = new Rotor[3];
        rotors[0] = new Rotor(wiring1, trigger1, start1);
        rotors[1] = new Rotor(wiring2, trigger2, start2);
        rotors[2] = new Rotor(wiring3, trigger3, start3);
        reflector = new Dictionary<char, char>();
        foreach (var w in reflectorWiring)
        {
            var parts = w.Split('-');
            char a = parts[0][0], b = parts[1][0];
            reflector[a] = b;
            reflector[b] = a;
        }
    }

    public string Encode(string message)
    {
        var output = new char[message.Length];
        bool next2 = false, next3 = false;
        for (int i = 0; i < message.Length; i++)
        {
            char c = message[i];
            rotors[0].Step();
            if (next2) { rotors[1].Step(); next2 = false; }
            if (next3) { rotors[1].Step(); rotors[2].Step(); next3 = false; }
            if (rotors[0].AtTrigger()) next2 = true;
            if (rotors[1].AtTrigger()) next3 = true;
            for (int j = 0; j < 3; j++) c = rotors[j].Forward(c);
            c = reflector[c];
            for (int j = 2; j >= 0; j--) c = rotors[j].Backward(c);
            output[i] = c;
        }
        return new string(output);
    }

    private class Rotor
    {
        private char[] fwd = new char[26];
        private char[] bwd = new char[26];
        private char trigger;
        private int pos;

        public Rotor(List<string> wiring, char trigger, char start)
        {
            this.trigger = trigger;
            pos = start - 'A';
            for (int i = 0; i < 26; i++)
            {
                var parts = wiring[i].Split('-');
                char a = parts[0][0], b = parts[1][0];
                fwd[a - 'A'] = b;
                bwd[b - 'A'] = a;
            }
        }

        public void Step() => pos = (pos + 1) % 26;
        public bool AtTrigger() => (char)('A' + pos) == trigger;

        public char Forward(char c)
        {
            int idx = (c - 'A' + pos) % 26;
            char wired = fwd[idx];
            int outIdx = (wired - 'A' - pos + 26) % 26;
            return (char)('A' + outIdx);
        }

        public char Backward(char c)
        {
            int idx = (c - 'A' + pos) % 26;
            char wired = bwd[idx];
            int outIdx = (wired - 'A' - pos + 26) % 26;
            return (char)('A' + outIdx);
        }
    }
}

