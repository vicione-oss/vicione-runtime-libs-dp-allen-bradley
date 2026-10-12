using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TestData;

/// <summary>
/// Codes a <see cref="FakeValue"/> as a 32-bit little-endian integer and refuses to encode a negative
/// one. Hand-rolled because Decode takes a ReadOnlySpan&lt;byte&gt;, and a ref struct cannot travel through
/// the object array a substitute routes its arguments in.
/// </summary>
internal sealed class FakeConverter : IDataPointConverter<FakeDataPoint, FakeValue>
{
    internal const string NegativeRefusal = "a negative number will not encode";

    internal int Decodes { get; private set; }

    /// <summary>Replaces the decode, so a test can make it throw.</summary>
    internal Func<FakeValue>? OnDecode { get; init; }

    public FakeValue Decode(FakeDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        Decodes++;
        return OnDecode is not null
            ? OnDecode()
            : new FakeValue(dataPoint, BinaryPrimitives.ReadInt32LittleEndian(buffer));
    }

    public byte[] Encode(FakeValue value)
    {
        if (value.Number < 0)
        {
            throw new InvalidOperationException(NegativeRefusal);
        }

        var bytes = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value.Number);
        return bytes;
    }
}
