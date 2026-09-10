using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X70;

/// <summary>
/// An end-to-end read through the production client stack, held against a raw libplctag handle on the
/// same tag.
/// </summary>
public sealed class LogixClientReadTests : LogixIntegrationTestBase
{
    [Fact]
    public async Task ADintDecodesToTheSameValueLibplctagReadsRaw()
    {
        // Arrange
        // The target is a COUNTER member, absent from the symbol table, so nothing checked its type.
        var expected = ReadDintWithRawLibplctag(BenchControllerTags.CounterPreset);
        ILogixDataPoint[] dataPoints =
        [
            new DIntDataPoint(new TagName(BenchControllerTags.CounterPreset), DefaultPollFrequency, NoChannels),
        ];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var values = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        values.Should().ContainSingle().Which.Value.Should().Be(expected);
    }

    // libplctag's own typed getter, so the expectation does not come from the decode under test.
    private static int ReadDintWithRawLibplctag(string tagName)
    {
        using var tag = BenchController.RawTagFor(tagName);
        tag.Read();

        return tag.GetInt32(0);
    }
}
