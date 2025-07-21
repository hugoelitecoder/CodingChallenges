using System;

class Solution
{
    static void Main(string[] args)
    {
        string program = Console.ReadLine();
        var emulator = new Emulator();
        emulator.Run(program);
        Console.WriteLine(emulator.GetRegisters());
    }

    class Emulator
    {
        private readonly int[] reg = new int[3];
        private const int EXIT = 0x00;
        private const int LD_START = 0x10, LD_END = 0x12;
        private const int ADD = 0x20;
        private const int SUB = 0x30;
        private const int OR = 0x40;
        private const int AND = 0x50;
        private const int XOR = 0x60;
        private const int SE_NN_START = 0x70, SE_NN_END = 0x72;
        private const int SNE_NN_START = 0x80, SNE_NN_END = 0x82;
        private const int SE_REG = 0x90;
        private const int SNE_REG = 0xA0;

        public void Run(string program)
        {
            for (int i = 0; i < program.Length; i += 4)
            {
                string instr = program.Substring(i, 4);
                int op = Convert.ToInt32(instr, 16);

                int opcode = (op & 0xFF00) >> 8;
                int nn = op & 0x00FF;
                int x = (op & 0x00F0) >> 4;
                int y = op & 0x000F;
                int k = (op & 0x0F00) >> 8;

                if (opcode == EXIT) break;

                if (opcode >= LD_START && opcode <= LD_END)
                    reg[k] = nn;
                else if (opcode == ADD)
                    Add(x, y);
                else if (opcode == SUB)
                    Sub(x, y);
                else if (opcode == OR)
                    reg[x] |= reg[y];
                else if (opcode == AND)
                    reg[x] &= reg[y];
                else if (opcode == XOR)
                    reg[x] ^= reg[y];
                else if (opcode >= SE_NN_START && opcode <= SE_NN_END)
                {
                    if (reg[k] == nn) i += 4;
                }
                else if (opcode >= SNE_NN_START && opcode <= SNE_NN_END)
                {
                    if (reg[k] != nn) i += 4;
                }
                else if (opcode == SE_REG)
                {
                    if (reg[x] == reg[y]) i += 4;
                }
                else if (opcode == SNE_REG)
                {
                    if (reg[x] != reg[y]) i += 4;
                }
            }
        }

        private void Add(int x, int y)
        {
            reg[x] += reg[y];
            reg[2] = reg[x] > 255 ? 1 : 0;
            reg[x] %= 256;
        }

        private void Sub(int x, int y)
        {
            reg[2] = reg[x] < reg[y] ? 1 : 0;
            reg[x] = (256 + reg[x] - reg[y]) % 256;
        }

        public string GetRegisters() => $"{reg[0]} {reg[1]} {reg[2]}";
    }
}
