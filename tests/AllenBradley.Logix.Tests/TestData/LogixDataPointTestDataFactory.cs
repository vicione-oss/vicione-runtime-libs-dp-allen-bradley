using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs the data points, groups and values the suites drive the client with, so a test reads as
/// what it is testing rather than as a constructor call.
/// </summary>
internal static class LogixDataPointTestDataFactory
{
    /// <summary>The frequency every point the factory makes is polled at.</summary>
    internal static PollFrequency DefaultPollFrequency { get; } = PollFrequency.FromMilliseconds(100);

    /// <summary>No channel routing — nothing below the data port reads the channels.</summary>
    internal static Channels NoChannels { get; } = new([], []);

    internal static TagName DefaultTagName { get; } = new("AnyTag");
}
