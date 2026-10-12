namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

/// <summary>
/// The wire format of one elementary value: how many bytes it occupies and how they are read and
/// written. CIP and .NET are both little-endian, so no codec swaps bytes. A codec knows nothing about data
/// points, which is what lets every dataport and both shapes, scalar and array, share it.
/// </summary>
internal abstract class ElementCodec<T>
{
    /// <summary>How many bytes one value occupies on the wire.</summary>
    internal abstract int Size { get; }

    /// <summary>
    /// Reads one value from the first <see cref="Size"/> bytes of <paramref name="buffer"/>, and throws
    /// when the buffer is shorter.
    /// </summary>
    internal abstract T Decode(ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Writes <paramref name="value"/> into the first <see cref="Size"/> bytes of <paramref name="buffer"/>,
    /// and throws when the buffer is shorter.
    /// </summary>
    internal abstract void Encode(T value, Span<byte> buffer);

    /// <summary>Returns the <see cref="Size"/> bytes that <paramref name="value"/> occupies on the wire.</summary>
    internal byte[] Encode(T value)
    {
        var bytes = new byte[Size];
        Encode(value, bytes);
        return bytes;
    }
}
