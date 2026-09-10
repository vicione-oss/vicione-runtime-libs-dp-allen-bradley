using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

internal static class LogixDataPointTestDataFactory
{
    internal static PollFrequency DefaultPollFrequency { get; } = PollFrequency.FromMilliseconds(100);

    /// <summary>No channel routing — nothing below the data port reads the channels.</summary>
    internal static Channels NoChannels { get; } = new([], []);

    internal static TagName DefaultTagName { get; } = new("AnyTag");
}
