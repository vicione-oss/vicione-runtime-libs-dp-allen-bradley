using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Booleans;

/// <summary>
/// A <c>BOOL</c> array is packed 32 to a word on the controller. The elements are chosen to catch a bit
/// read at the wrong shift or out of the wrong byte.
/// </summary>
public abstract class BooleanArrayIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task ABoolArrayReadsBackAsWritten()
    {
        // Arrange
        BoolArrayDataPoint flags = new(
            TagPath.Parse(Tags.BoolArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.BoolArrayElementCount);
        // Both ends of every byte of the word, eight to a line.
        bool[] thirtyTwoFlags =
        [
            true, true, false, false, false, false, false, true,
            true, false, false, false, false, false, false, true,
            true, false, false, false, false, false, false, false,
            false, false, false, false, false, false, true, true
        ];

        // Act
        var roundTripResult = await RoundTripAsync(flags, thirtyTwoFlags);

        // Assert
        // The controller declares this array differently from every other: the type as DWORD, which
        // decodes to Bool, and the extent as the one word its 32 bits fill.
        var expectedDeclaredType = new DeclaredType(flags.TagAddress, AllenBradleyDataType.Bool, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.BoolArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(flags, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<bool[]>().Which.Should().Equal(thirtyTwoFlags);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : BooleanArrayIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : BooleanArrayIntegrationTests(fixture, output, TagScope.Program);
}
