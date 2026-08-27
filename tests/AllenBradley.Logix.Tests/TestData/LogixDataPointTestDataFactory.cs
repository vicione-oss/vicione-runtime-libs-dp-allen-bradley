using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs the data points and values the suites drive the client with, so a test reads as what it
/// is testing rather than as a constructor call.
/// </summary>
/// <remarks>
/// <see cref="CreateValue{TDomain}"/> goes through the data point's own
/// <c>CreateLogixValue</c> rather than <see cref="ILogixDataPoint.ConvertValue"/>: a value the suites
/// hand to a converter should be one the model made, but routing it through the untyped conversion
/// first would put a second thing under test in every converter assertion.
/// <para>
/// Poll frequency and channels are supplied because a data point cannot be built without them, not
/// because any test asserts on them. They are the same for every point the factory makes, which keeps
/// two points naming the same tag equal — the tag cache keys on the whole record.
/// </para>
/// </remarks>
internal static class LogixDataPointTestDataFactory
{
    /// <summary>The frequency every point the factory makes is polled at.</summary>
    internal static PollFrequency DefaultPollFrequency { get; } = PollFrequency.FromMilliseconds(100);

    /// <summary>No channel routing — nothing below the data port reads the channels.</summary>
    internal static Channels NoChannels { get; } = new([], []);

    internal static DIntDataPoint CreateDInt(string tagName) => CreateDInt(new TagName(tagName));

    internal static DIntDataPoint CreateDInt(TagName tagName) =>
        new(tagName, DefaultPollFrequency, NoChannels);

    internal static RealDataPoint CreateReal(string tagName) =>
        new(new TagName(tagName), DefaultPollFrequency, NoChannels);

    /// <summary>The point's own typed value, carrying <paramref name="value"/> as it stands.</summary>
    internal static ILogixDataPointValue CreateValue<TDomain>(
        LogixDataPoint<TDomain> dataPoint, TDomain value) =>
        dataPoint.CreateLogixValue(value);
}
