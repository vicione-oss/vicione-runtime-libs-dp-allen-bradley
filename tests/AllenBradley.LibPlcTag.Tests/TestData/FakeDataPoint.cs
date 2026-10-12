using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TestData;

/// <summary>A data point named and compared by <paramref name="Name"/> alone.</summary>
internal sealed record FakeDataPoint(string Name) : IDataPoint
{
    public Channels Channels { get; init; } = new([], []);

    public DataPointIdentifier Identifier => new(Name);

    public DataTypeName DataTypeName => new("FAKE");

    public bool ContainsTransferredChannel(string channel) => false;
}

/// <summary>A whole number read from or written to <paramref name="DataPoint"/>.</summary>
internal sealed record FakeValue(FakeDataPoint DataPoint, int Number);
