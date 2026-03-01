using System;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        var line = Console.ReadLine();
        if (line == null) line = string.Empty;
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var length = parts.Length;
        var program = new byte[length];
        for (var i = 0; i < length; i++)
        {
            program[i] = ParseHexByte(parts[i]);
        }
        var cpu = new MiniCpu();
        cpu.LoadProgram(program);
        cpu.Execute();
        stopwatch.Stop();
        var regs = cpu.Registers;
        for (var i = 0; i < 4; i++)
        {
            Console.WriteLine(regs[i]);
        }
        Console.Error.WriteLine("[DEBUG] Input: " + line);
        Console.Error.WriteLine("[DEBUG] Bytes: " + length);
        Console.Error.WriteLine("[DEBUG] ElapsedMs: " + stopwatch.Elapsed.TotalMilliseconds);
    }

    private static byte ParseHexByte(string s)
    {
        var c0 = s[0];
        var c1 = s[1];
        var hi = HexValue(c0);
        var lo = HexValue(c1);
        var value = (hi << 4) | lo;
        return (byte)value;
    }

    private static int HexValue(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        return 0;
    }
}

class MiniCpu
{
    private byte[] _program;
    private byte[] _registers;
    private int _pc;

    public MiniCpu()
    {
        _registers = new byte[4];
        _program = Array.Empty<byte>();
        _pc = 0;
    }

    public byte[] Registers
    {
        get { return _registers; }
    }

    public void LoadProgram(byte[] program)
    {
        _program = program;
        Array.Clear(_registers, 0, 4);
        _pc = 0;
    }

    public void Execute()
    {
        var program = _program;
        var regs = _registers;
        var pc = _pc;
        var n = program.Length;
        while (pc < n)
        {
            var op = program[pc];
            if (op == 0xFF) break;
            if (op == 0x01)
            {
                var x = program[pc + 1] & 0x03;
                var v = program[pc + 2];
                regs[x] = v;
                pc += 3;
            }
            else if (op == 0x02)
            {
                var x = program[pc + 1] & 0x03;
                var y = program[pc + 2] & 0x03;
                var value = (regs[x] + regs[y]) & 0xFF;
                regs[x] = (byte)value;
                pc += 3;
            }
            else if (op == 0x03)
            {
                var x = program[pc + 1] & 0x03;
                var y = program[pc + 2] & 0x03;
                var value = (regs[x] - regs[y]) & 0xFF;
                regs[x] = (byte)value;
                pc += 3;
            }
            else if (op == 0x04)
            {
                var x = program[pc + 1] & 0x03;
                var y = program[pc + 2] & 0x03;
                var value = (regs[x] * regs[y]) & 0xFF;
                regs[x] = (byte)value;
                pc += 3;
            }
            else if (op == 0x05)
            {
                var x = program[pc + 1] & 0x03;
                var value = (regs[x] + 1) & 0xFF;
                regs[x] = (byte)value;
                pc += 2;
            }
            else if (op == 0x06)
            {
                var x = program[pc + 1] & 0x03;
                var value = (regs[x] - 1) & 0xFF;
                regs[x] = (byte)value;
                pc += 2;
            }
            else
            {
                break;
            }
        }
        _pc = pc;
    }
}