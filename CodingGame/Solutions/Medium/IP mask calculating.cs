using System;

class Solution
{
    static void Main()
    {
        var network = IPv4Network.Parse(Console.ReadLine().Trim());
        Console.WriteLine(IPv4Network.ToDotted(network.Network));
        Console.WriteLine(IPv4Network.ToDotted(network.Broadcast));
    }
}
public readonly struct IPv4Network
{
    private readonly uint _addr;
    public int Prefix { get; }

    public IPv4Network(uint addr, int prefix)
    {
        if (prefix < 0 || prefix > 32)
            throw new ArgumentOutOfRangeException(nameof(prefix));

        _addr = addr;
        Prefix = prefix;
    }

    public static IPv4Network Parse(string cidr)
    {
        var parts = cidr.Split('/');
        uint addr = ParseAddress(parts[0]);
        int prefix = int.Parse(parts[1]);
        return new IPv4Network(addr, prefix);
    }

    public uint Mask => Prefix == 0
        ? 0u
        : Prefix == 32
            ? 0xFFFFFFFFu
            : 0xFFFFFFFFu << (32 - Prefix);

    public uint Network => _addr & Mask;
    public uint Broadcast => Network | ~Mask;

    public override string ToString() => $"{ToDotted(Network)}/{Prefix}";

    public static string ToDotted(uint addr) =>
        $"{(addr >> 24) & 0xFF}.{(addr >> 16) & 0xFF}.{(addr >> 8) & 0xFF}.{addr & 0xFF}";

    private static uint ParseAddress(string dotted)
    {
        var o = dotted.Split('.');
        return (uint.Parse(o[0]) << 24)
                | (uint.Parse(o[1]) << 16)
                | (uint.Parse(o[2]) << 8)
                |  uint.Parse(o[3]);
    }
}