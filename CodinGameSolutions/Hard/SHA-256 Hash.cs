using System;
using System.Text;
using System.Buffers.Binary;

class Solution
{
    static void Main(string[] args)
    {
        var message = Console.ReadLine();
        var hasher = new Sha256Hasher();
        var hashed = hasher.ComputeHash(message);
        Console.WriteLine(hashed);
    }
}

public class Sha256Hasher
{
    private static readonly uint[] _initialHash = {
        0xcbbb9d5d, 0x629a292a, 0x9159015a, 0x152fecd8,
        0x67332667, 0x8eb44a87, 0xdb0c2e0d, 0x47b5481d
    };

    private static readonly uint[] _roundConstants = {
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
    };

    public string ComputeHash(string message)
    {
        var paddedMessage = PadMessage(message);
        var hash = (uint[])_initialHash.Clone();
        var w = new uint[64];
        for (var i = 0; i < paddedMessage.Length; i += 64)
        {
            var chunk = new ReadOnlySpan<byte>(paddedMessage, i, 64);
            ProcessChunk(chunk, hash, w);
        }
        var sb = new StringBuilder(64);
        foreach (var val in hash)
        {
            sb.Append(val.ToString("x8"));
        }
        return sb.ToString();
    }

    private void ProcessChunk(ReadOnlySpan<byte> chunk, uint[] hash, uint[] w)
    {
        for (var i = 0; i < 16; i++)
        {
            w[i] = BinaryPrimitives.ReadUInt32BigEndian(chunk.Slice(i * 4, 4));
        }
        for (var i = 16; i < 64; i++)
        {
            var s0 = RotateRight(w[i - 15], 7) ^ RotateRight(w[i - 15], 18) ^ (w[i - 15] >> 3);
            var s1 = RotateRight(w[i - 2], 17) ^ RotateRight(w[i - 2], 19) ^ (w[i - 2] >> 10);
            w[i] = w[i - 16] + s0 + w[i - 7] + s1;
        }
        var a = hash[0];
        var b = hash[1];
        var c = hash[2];
        var d = hash[3];
        var e = hash[4];
        var f = hash[5];
        var g = hash[6];
        var h = hash[7];
        for (var i = 0; i < 64; i++)
        {
            var S1 = RotateRight(e, 6) ^ RotateRight(e, 11) ^ RotateRight(e, 25);
            var ch = (e & f) ^ (~e & g);
            var temp1 = h + S1 + ch + _roundConstants[i] + w[i];
            var S0 = RotateRight(a, 2) ^ RotateRight(a, 13) ^ RotateRight(a, 22);
            var maj = (a & b) ^ (a & c) ^ (b & c);
            var temp2 = S0 + maj;
            h = g;
            g = f;
            f = e;
            e = d + temp1;
            d = c;
            c = b;
            b = a;
            a = temp1 + temp2;
        }
        hash[0] += a;
        hash[1] += b;
        hash[2] += c;
        hash[3] += d;
        hash[4] += e;
        hash[5] += f;
        hash[6] += g;
        hash[7] += h;
    }

    private byte[] PadMessage(string message)
    {
        var originalBytes = Encoding.ASCII.GetBytes(message);
        var originalLengthInBits = (ulong)originalBytes.Length * 8;
        var lengthWithTerminator = originalBytes.Length + 1;
        var paddingZeros = (56 - (lengthWithTerminator % 64) + 64) % 64;
        var finalLength = lengthWithTerminator + paddingZeros + 8;
        var paddedMessage = new byte[finalLength];
        Buffer.BlockCopy(originalBytes, 0, paddedMessage, 0, originalBytes.Length);
        paddedMessage[originalBytes.Length] = 0x80;
        BinaryPrimitives.WriteUInt64BigEndian(paddedMessage.AsSpan(finalLength - 8), originalLengthInBits);
        return paddedMessage;
    }

    private uint RotateRight(uint x, int n)
    {
        return (x >> n) | (x << (32 - n));
    }
}

