using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Floats;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData;

/// <summary>Builds data points directly, so a client test does not need the mapper.</summary>
internal static class LegacyDataPointTestDataFactory
{
    internal static readonly FileNumber DefaultIntegerFile = new(7);
    internal static readonly FileNumber DefaultLongFile = new(9);
    internal static readonly FileNumber DefaultFloatFile = new(8);
    internal static readonly FileNumber DefaultStringFile = new(9);
    internal static readonly FileNumber DefaultBinaryFile = new(3);
    internal static readonly PollFrequency DefaultPollFrequency = PollFrequency.FromMilliseconds(100);
    internal static readonly Channels NoChannels = new([], []);

    /// <summary>
    /// Element <paramref name="elementNumber"/> of the integer file <c>N7</c>. For another element, create a
    /// new data point. <c>with</c> does not update the data-file address.
    /// </summary>
    internal static IntegerDataPoint N7Element(ushort elementNumber) =>
        new(DefaultIntegerFile, new ElementNumber(elementNumber), DefaultPollFrequency, NoChannels);

    /// <summary>
    /// Element <paramref name="elementNumber"/> of the long file <c>L9</c>. For another element, create a
    /// new data point. <c>with</c> does not update the data-file address.
    /// </summary>
    internal static LongDataPoint L9Element(ushort elementNumber) =>
        new(DefaultLongFile, new ElementNumber(elementNumber), DefaultPollFrequency, NoChannels);

    /// <summary>
    /// Element <paramref name="elementNumber"/> of the float file <c>F8</c>. For another element, create a
    /// new data point. <c>with</c> does not update the data-file address.
    /// </summary>
    internal static FloatDataPoint F8Element(ushort elementNumber) =>
        new(DefaultFloatFile, new ElementNumber(elementNumber), DefaultPollFrequency, NoChannels);

    /// <summary>
    /// Element <paramref name="elementNumber"/> of the string file <c>ST9</c>. For another element, create a
    /// new data point. <c>with</c> does not update the data-file address.
    /// </summary>
    internal static StringDataPoint ST9Element(ushort elementNumber) =>
        new(DefaultStringFile, new ElementNumber(elementNumber), DefaultPollFrequency, NoChannels);

    /// <summary>
    /// Bit <paramref name="bitNumber"/> of element <paramref name="elementNumber"/> in the binary file <c>B3</c>.
    /// For another bit, create a new data point. <c>with</c> does not update the data-file address.
    /// </summary>
    internal static BitDataPoint B3Bit(ushort elementNumber, ushort bitNumber) =>
        new(DefaultBinaryFile, new ElementNumber(elementNumber), new BitNumber(bitNumber), DefaultPollFrequency, NoChannels);
}
