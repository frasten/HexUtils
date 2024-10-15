using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace HexUtils;

[CollectionBuilder(typeof(Hex), nameof(Create))]
public ref struct Hex
{
    private Span<byte> _data;

    public ref byte this[int index] => ref _data[index];
    public ref byte this[Index index] => ref _data[index];

    public Hex this[Range range]
    {
        get => new(_data[range]);
        set => value.CopyTo(_data[range]);
    }

    public Hex(string hex)
    {
        _data = FromHexString(hex);
    }

    public Hex(Span<byte> data)
    {
        _data = data;
    }

    public Hex(short data)
    {
        _data = BitConverter.GetBytes(data);
    }

    public Hex(ushort data)
    {
        _data = BitConverter.GetBytes(data);
    }

    public Hex(int data)
    {
        _data = BitConverter.GetBytes(data);
    }

    public Hex(uint data)
    {
        _data = BitConverter.GetBytes(data);
    }

    public Hex(long data)
    {
        _data = BitConverter.GetBytes(data);
    }

    public Hex(ulong data)
    {
        _data = BitConverter.GetBytes(data);
    }

    /// <summary>
    ///     Supporto alle collection expression:
    /// </summary>
    public static Hex Create(ReadOnlySpan<byte> values) => new(values.ToArray());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] Xor(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        return RunOperation(a, b, (b1, b2) => (byte) (b1 ^ b2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] Add(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        return RunOperation(a, b, (b1, b2) => (byte) (b1 + b2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] Subtract(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        return RunOperation(a, b, (b1, b2) => (byte) (b1 - b2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] RunOperation(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, Func<byte, byte, byte> operation)
    {
        var len = Math.Min(a.Length, b.Length);
        var result = new byte[len];
        for (var i = 0; i < len; i++)
        {
            result[i] = operation(a[i], b[i]);
        }

        return result;
    }

    private static byte[] Reverse(ReadOnlySpan<byte> a)
    {
        var arr = a.ToArray();
        Array.Reverse(arr);
        return arr;
    }

    public Hex Reverse()
    {
        return new Hex(Reverse(_data));
    }

    public Hex Slice(int start) => new(_data[start..]);
    public Hex Slice(int start, int length) => new(_data.Slice(start, length));

    public void CopyTo(Span<byte> destination) => _data.CopyTo(destination);
    public void CopyTo(Hex destination) => _data.CopyTo(destination);

    // Must have a GetEnumerator() method that returns an IEnumerator implementation
    public Span<byte>.Enumerator GetEnumerator() => _data.GetEnumerator();

    // @formatter=off
    public static Hex operator ^(Hex a, Hex b)    => new(Xor(a._data, b._data));
    public static Hex operator ^(Hex a, short b)  => a ^ new Hex(b);
    public static Hex operator ^(Hex a, ushort b) => a ^ new Hex(b);
    public static Hex operator ^(Hex a, int b)    => a ^ new Hex(b);
    public static Hex operator ^(Hex a, uint b)   => a ^ new Hex(b);
    public static Hex operator ^(Hex a, long b)   => a ^ new Hex(b);
    public static Hex operator ^(Hex a, ulong b)  => a ^ new Hex(b);

    public static Hex operator +(Hex a, Hex b) => new(Add(a._data, b._data));
    public static Hex operator +(Hex a, short b)  => a + new Hex(b);
    public static Hex operator +(Hex a, ushort b) => a + new Hex(b);
    public static Hex operator +(Hex a, int b)    => a + new Hex(b);
    public static Hex operator +(Hex a, uint b)   => a + new Hex(b);
    public static Hex operator +(Hex a, long b)   => a + new Hex(b);
    public static Hex operator +(Hex a, ulong b)  => a + new Hex(b);

    public static Hex operator -(Hex a, Hex b)    => new(Subtract(a._data, b._data));
    public static Hex operator -(Hex a, short b)  => a - new Hex(b);
    public static Hex operator -(Hex a, ushort b) => a - new Hex(b);
    public static Hex operator -(Hex a, int b)    => a - new Hex(b);
    public static Hex operator -(Hex a, uint b)   => a - new Hex(b);
    public static Hex operator -(Hex a, long b)   => a - new Hex(b);
    public static Hex operator -(Hex a, ulong b)  => a - new Hex(b);
    // @formatter=on

    public static implicit operator ReadOnlySpan<byte>(Hex hex) => hex._data;
    public static implicit operator Span<byte>(Hex hex) => hex._data;
    public static implicit operator Hex(Span<byte> data) => new(data);
    public static implicit operator string(Hex data) => data.ToString();
    public static implicit operator Hex(string data) => new(data);

    public Span<byte> AsSpan() => this;
    public ReadOnlySpan<byte> AsReadOnlySpan() => this;

    public string ToHexString()
    {
        // Se la lunghezza del buffer è 0, restituisci una stringa vuota.
        if (_data.Length == 0)
        {
            return string.Empty;
        }

        // Ogni byte richiede 2 caratteri esadecimali e un separatore (spazio) tranne l'ultimo byte.
        var len = _data.Length * 3 - 1;

        const int maxStackSize = 128; // = 256bytes
        char[]? rentedFromPool = null;
        var buffer =
            len > maxStackSize
                ? (rentedFromPool = ArrayPool<char>.Shared.Rent(len)).AsSpan(0, len)
                : stackalloc char[len];

        try
        {
            var bufferIndex = 0;

            for (var i = 0; i < _data.Length; i++)
            {
                // Converti ogni byte in due caratteri esadecimali.
                var b = _data[i];
                buffer[bufferIndex++] = GetHexChar(b >> 4);  // Prima cifra esadecimale
                buffer[bufferIndex++] = GetHexChar(b & 0x0F); // Seconda cifra esadecimale

                // Inserisci uno spazio tra i byte, tranne dopo l'ultimo.
                if (i < _data.Length - 1)
                {
                    buffer[bufferIndex++] = ' ';
                }
            }

            // Crea la stringa dal buffer.
            return new string(buffer[..bufferIndex]);
        }
        finally
        {
            // Se abbiamo usato l'ArrayPool, restituiamo il buffer.
            if (rentedFromPool != null)
            {
                ArrayPool<char>.Shared.Return(rentedFromPool);
            }
        }

        static char GetHexChar(int value)
        {
            return (char)(value < 10 ? value + '0' : value - 10 + 'A');
        }
    }



    public Hex AsWord
    {
        get => Slice(0, 2);
        set => this[0..2] = value;
    }

    public Hex AsDWord
    {
        get => Slice(0, 4);
        set => this[0..4] = value;
    }

    public Hex AsQWord
    {
        get => Slice(0, 8);
        set => this[0..8] = value;
    }

    public ref byte Byte1 => ref _data[1];
    public ref byte Byte2 => ref _data[2];
    public ref byte Byte3 => ref _data[3];
    public ref byte Byte4 => ref _data[4];
    public ref byte Byte5 => ref _data[5];
    public ref byte Byte6 => ref _data[6];
    public ref byte Byte7 => ref _data[7];
    public ref byte HiByte => ref _data[^1];

    private static byte[] FromHexString(string hex)
    {
        var hexCharCount = CountHexChars(hex);
        // Determina la lunghezza necessaria dell'array di byte
        var byteCount = (hexCharCount + 1) / 2;
        var result = new byte[byteCount];

        var byteIndex = 0;
        var appendLeadingZero = hexCharCount % 2 != 0;

        foreach (var c in hex)
        {
            if (char.IsWhiteSpace(c))
                continue;

            if (appendLeadingZero)
            {
                // Se dobbiamo aggiungere uno zero davanti, lo facciamo qui
                result[byteIndex++] = ConvertHexCharToByte(c);
                appendLeadingZero = false; // Solo per il primo carattere
            }
            else
            {
                // Se siamo su una coppia di caratteri, li convertiamo insieme
                if (byteIndex == 0)
                {
                    result[byteIndex] = (byte)(ConvertHexCharToByte(c) << 4); // Shift sinistro per il primo carattere
                }
                else
                {
                    result[byteIndex++] |= ConvertHexCharToByte(c); // Completa il byte con il secondo carattere
                }
            }
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountHexChars(string hex)
    {
        var count = 0;
        foreach (var c in hex)
        {
            if (!char.IsWhiteSpace(c))
                count++;
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ConvertHexCharToByte(char c)
    {
        return (byte)(c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => 0
        });
    }

    public override string ToString()
    {
        return Encoding.ASCII.GetString(_data);
    }

    // using static Hex;
    public static Hex H(string hex) => new(hex);
    public static Hex H(Span<byte> data) => new(data);
    public static Hex H(short data) => new(data);
    public static Hex H(ushort data) => new(data);
    public static Hex H(int data) => new(data);
    public static Hex H(uint data) => new(data);
    public static Hex H(long data) => new(data);
    public static Hex H(ulong data) => new(data);
}