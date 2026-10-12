using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData;

/// <summary>Builds data points directly, so a client test does not need the mapper.</summary>
internal static class LegacyDataPointTestDataFactory
{
    internal static readonly FileNumber DefaultIntegerFile = new(7);
    internal static readonly PollFrequency DefaultPollFrequency = PollFrequency.FromMilliseconds(100);
    internal static readonly Channels NoChannels = new([], []);

    /// <summary>
    /// Element <paramref name="elementNumber"/> of the integer file <c>N7</c>. For another element, create a
    /// new data point. <c>with</c> does not update the data-file address.
    /// </summary>
    internal static IntegerDataPoint N7Element(ushort elementNumber) =>
        new(DefaultIntegerFile, new ElementNumber(elementNumber), DefaultPollFrequency, NoChannels);
}
