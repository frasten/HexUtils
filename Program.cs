using HexUtils;
using static HexUtils.Hex;

public static class Program
{
    public static void Main()
    {
        // Esempi di utilizzo:
        var a = H("71 0A 64 B6 FC 62 26 1F");
        a[7] = 159;

        var b = H("B4 CD FD F5 79 34 17 C9") + H("9E 94 6C 7E EC 11 61 9A");
        a.AsDWord += H("19 F1 30 F9");
        a.AsDWord += 0x91D37B0Bu;

        a[4] += 51;
        a[5] += 83;
        a[6] += 35;

        b.AsDWord ^= 0x91D37B0B;
        b[4] ^= 0xcd;
        b[5] ^= 0x62;
        b[6] ^= 0x60;
        b.AsQWord ^= 0xA93274F9845538E;
        a.AsDWord ^= H("A9 AC C5 7B");

        Hex c = "A9 AC C5 7B";

        Hex d = stackalloc byte[] {1, 2, 3};
        Hex e = [3, 2, 1];

        Console.WriteLine(a);
        Console.WriteLine(e.ToHexString());
    }
}