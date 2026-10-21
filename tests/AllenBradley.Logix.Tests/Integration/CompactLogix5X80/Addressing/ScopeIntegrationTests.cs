using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Addressing;

/// <summary>
/// The two scopes side by side, on <see cref="TagAddresses.ShadowedDInt"/>, the one tag provisioned under the
/// same name in both. The program tag shadows the controller tag in the program's logic, but an address
/// names exactly one of the two. The suite writes both.
/// </summary>
public sealed class ScopeIntegrationTests(TestControllerFixture fixture, ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(fixture, output, TagScope.Program)
{
    [Fact]
    public async Task TagsOfTheSameNameInBothScopesAreSeparate()
    {
        // Arrange
        var controllerTag = new DIntDataPoint(
            TagPath.Parse(new TagAddresses(TagScope.Controller).ShadowedDInt), DefaultPollFrequency, NoChannels);
        var programTag = new DIntDataPoint(
            TagPath.Parse(new TagAddresses(TagScope.Program).ShadowedDInt), DefaultPollFrequency, NoChannels);
        ILogixDataPointValue[] written = [controllerTag.CreateLogixValue(1111), programTag.CreateLogixValue(2222)];

        // Act
        await Client.WriteAsync(written, TestContext.Current.CancellationToken);
        var bothAfterwards = await ReadAsync([controllerTag, programTag]);

        // Assert
        bothAfterwards.Should().Equal(written);
    }
}
